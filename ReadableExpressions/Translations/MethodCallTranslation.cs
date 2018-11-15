﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using Translators;
    using Extensions;
    using Interfaces;
    using NetStandardPolyfills;

    internal static class MethodCallTranslation
    {
        public static ITranslation For(InvocationExpression invocation, ITranslationContext context)
        {
            var invocationMethod = invocation.Expression.Type.GetPublicInstanceMethod("Invoke");

            var method = new BclMethodWrapper(invocationMethod);
            var parameters = new ParameterSetTranslation(method, invocation.Arguments, context).WithParentheses();
            var subject = context.GetTranslationFor(invocation.Expression);

            if (subject.NodeType == Lambda)
            {
                subject = subject.WithParentheses();
            }

            return new StandardMethodCallTranslation(Invoke, subject, method, parameters, context);
        }

        public static ITranslation For(MethodCallExpression methodCall, ITranslationContext context)
        {
            var method = new BclMethodWrapper(methodCall.Method);
            var parameters = new ParameterSetTranslation(method, methodCall.Arguments, context);

            if (IsStringConcatCall(methodCall))
            {
                return new StringConcatenationTranslation(Call, methodCall.Arguments, context);
            }

            if (methodCall.Method.IsImplicitOperator())
            {
                return new CodeBlockTranslation(parameters[0]).WithNodeType(Call);
            }

            var subject = GetSubjectTranslation(methodCall, context);

            if (IsIndexedPropertyAccess(methodCall))
            {
                return new IndexAccessTranslation(subject, parameters, methodCall.Type);
            }

            parameters = parameters.WithParentheses();

            if (methodCall.Method.IsExplicitOperator())
            {
                return CastTranslation.ForExplicitOperator(
                    parameters[0],
                    context.GetTranslationFor(methodCall.Method.ReturnType));
            }

            var methodCallTranslation = new StandardMethodCallTranslation(Call, subject, method, parameters, context);

            if (context.IsPartOfMethodCallChain(methodCall))
            {
                methodCallTranslation.AsPartOfMethodCallChain();
            }

            return methodCallTranslation;
        }

        private static bool IsStringConcatCall(MethodCallExpression methodCall)
        {
            return methodCall.Method.IsStatic &&
                   (methodCall.Method.DeclaringType == typeof(string)) &&
                   (methodCall.Method.Name == "Concat");
        }

        public static ITranslation GetSubjectTranslation(MethodCallExpression methodCall, ITranslationContext context)
        {
            return context.GetTranslationFor(methodCall.GetSubject()) ??
                   context.GetTranslationFor(methodCall.Method.DeclaringType);
        }

        private static bool IsIndexedPropertyAccess(MethodCallExpression methodCall)
        {
            var property = methodCall
                .Object?
                .Type
                .GetPublicInstanceProperties()
                .FirstOrDefault(p => p.IsIndexer() && p.GetAccessors().Contains(methodCall.Method));

            return property?.GetIndexParameters().Any() == true;
        }

        public static ITranslation ForCustomMethodCast(
            ITranslation typeNameTranslation,
            IMethod castMethod,
            ITranslation castValue,
            ITranslationContext context)
        {
            return new StandardMethodCallTranslation(
                Call,
                typeNameTranslation,
                castMethod,
                new ParameterSetTranslation(castValue).WithParentheses(),
                context);
        }

        public static ITranslation ForDynamicMethodCall(
            ITranslation subjectTranslation,
            IMethod method,
            ICollection<Expression> arguments,
            ITranslationContext context)
        {
            return new StandardMethodCallTranslation(
                Dynamic,
                subjectTranslation,
                method,
                new ParameterSetTranslation(arguments, context).WithParentheses(),
                context);
        }

        private class StandardMethodCallTranslation : ITranslation
        {
            private readonly ITranslation _subjectTranslation;
            private readonly MethodInvocationTranslatable _methodInvocationTranslatable;
            private bool _isPartOfMethodCallChain;

            public StandardMethodCallTranslation(
                ExpressionType nodeType,
                ITranslation subjectTranslation,
                IMethod method,
                ParameterSetTranslation parameters,
                ITranslationContext context)
            {
                NodeType = nodeType;
                _subjectTranslation = subjectTranslation;
                _methodInvocationTranslatable = new MethodInvocationTranslatable(method, parameters, context);
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
                => _subjectTranslation.EstimatedSize + ".".Length + _methodInvocationTranslatable.EstimatedSize;

            public ExpressionType NodeType { get; }

            public Type Type => _methodInvocationTranslatable.Type;

            public int EstimatedSize { get; }

            public void AsPartOfMethodCallChain() => _isPartOfMethodCallChain = true;

            public void WriteTo(ITranslationContext context)
            {
                _subjectTranslation.WriteInParenthesesIfRequired(context);

                if (_isPartOfMethodCallChain)
                {
                    context.WriteNewLineToTranslation();
                    context.Indent();
                }

                context.WriteToTranslation('.');
                _methodInvocationTranslatable.WriteTo(context);

                if (_isPartOfMethodCallChain)
                {
                    context.Unindent();
                }
            }
        }

        private class MethodInvocationTranslatable : ITranslatable
        {
            private readonly IMethod _method;
            private readonly ParameterSetTranslation _parameters;
            private readonly string[] _explicitGenericArgumentNames;

            public MethodInvocationTranslatable(IMethod method, ParameterSetTranslation parameters, ITranslationContext context)
            {
                _method = method;
                _parameters = parameters;
                _explicitGenericArgumentNames = GetRequiredExplicitGenericArgumentNames(context, out var totalLength);
                EstimatedSize = method.Name.Length + totalLength + parameters.EstimatedSize;
            }

            private string[] GetRequiredExplicitGenericArgumentNames(ITranslationContext context, out int totalLength)
            {
                if (!_method.IsGenericMethod)
                {
                    totalLength = 0;
                    return Enumerable<string>.EmptyArray;
                }

                var methodGenericDefinition = _method.GetGenericMethodDefinition();
                var genericParameterTypes = methodGenericDefinition.GetGenericArguments().ToList();

                if (context.Settings.UseImplicitGenericParameters)
                {
                    RemoveSuppliedGenericTypeParameters(
                        methodGenericDefinition.GetParameters().Project(p => p.ParameterType),
                        genericParameterTypes);
                }

                if (!genericParameterTypes.Any())
                {
                    totalLength = 0;
                    return Enumerable<string>.EmptyArray;
                }

                var argumentNamesLength = 0;

                var argumentNames = _method
                    .GetGenericArguments()
                    .Project(a =>
                    {
                        var argumentName = a.GetFriendlyName(context.Settings);

                        if (argumentName != null)
                        {
                            argumentNamesLength += argumentName.Length + 2;
                        }

                        return argumentName;
                    })
                    .Filter(name => name != null)
                    .ToArray();

                totalLength = argumentNamesLength;

                return (totalLength != 0) ? argumentNames : Enumerable<string>.EmptyArray;
            }

            private static void RemoveSuppliedGenericTypeParameters(
                IEnumerable<Type> types,
                ICollection<Type> genericParameterTypes)
            {
                foreach (var type in types.Project(t => t.IsByRef ? t.GetElementType() : t))
                {
                    if (type.IsGenericParameter && genericParameterTypes.Contains(type))
                    {
                        genericParameterTypes.Remove(type);
                    }

                    if (type.IsGenericType())
                    {
                        RemoveSuppliedGenericTypeParameters(type.GetGenericTypeArguments(), genericParameterTypes);
                    }
                }
            }

            public Type Type => _method.ReturnType;

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation(_method.Name);
                WriteGenericArgumentNamesIfNecessary(context);
                _parameters.WriteTo(context);
            }

            private void WriteGenericArgumentNamesIfNecessary(ITranslationContext context)
            {
                if (_explicitGenericArgumentNames.Length == 0)
                {
                    return;
                }

                context.WriteToTranslation('<');

                for (int i = 0, l = _explicitGenericArgumentNames.Length - 1; ; ++i)
                {
                    context.WriteToTranslation(_explicitGenericArgumentNames[i]);

                    if (i == l)
                    {
                        break;
                    }

                    context.WriteToTranslation(", ");
                }

                context.WriteToTranslation('>');
            }
        }
    }
}
