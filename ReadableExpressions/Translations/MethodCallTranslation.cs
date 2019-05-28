namespace AgileObjects.ReadableExpressions.Translations
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
    using Extensions;
    using Interfaces;
    using NetStandardPolyfills;
    using System.Reflection;
    using System.Runtime.CompilerServices;

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
            if (IsPropertyGetterOrSetterCall(methodCall, out var property))
            {
                var getterTranslation = new PropertyGetterTranslation(methodCall, property, context);

                if (methodCall.Method.ReturnType != typeof(void))
                {
                    return getterTranslation;
                }

                return new PropertySetterTranslation(methodCall, getterTranslation, context);
            }

            if (IsStringConcatCall(methodCall))
            {
                return new StringConcatenationTranslation(Call, methodCall.Arguments, context);
            }

            var method = new BclMethodWrapper(methodCall.Method);
            var parameters = new ParameterSetTranslation(method, methodCall.Arguments, context);

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

        private static bool IsPropertyGetterOrSetterCall(MethodCallExpression methodCall, out PropertyInfo property)
        {
            if (methodCall.Method.HasAttribute<CompilerGeneratedAttribute>())
            {
                // Find declaring property
                property = GetPropertyOrNull(methodCall.Method);

                if (property != null)
                {
                    return true;
                }
            }

            property = null;
            return false;
        }

        private static PropertyInfo GetPropertyOrNull(MethodInfo method)
        {
            var hasSingleArgument = method.GetParameters().Length == 1;
            var hasReturnType = method.ReturnType != typeof(void);

            if (hasSingleArgument == hasReturnType)
            {
                return null;
            }

            var type = method.DeclaringType;

            var allProperties =
                        type.GetPublicInstanceProperties()
                .Concat(type.GetNonPublicInstanceProperties())
                .Concat(type.GetPublicStaticProperties())
                .Concat(type.GetNonPublicStaticProperties());

            return allProperties.FirstOrDefault(property => Equals(
                hasReturnType
                    ? property.GetGetter(nonPublic: true)
                    : property.GetSetter(nonPublic: true),
                method));
        }

        private static bool IsStringConcatCall(MethodCallExpression methodCall)
        {
            return methodCall.Method.IsStatic &&
                  (methodCall.Method.DeclaringType == typeof(string)) &&
                  (methodCall.Method.Name == nameof(string.Concat));
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

        private class PropertyGetterTranslation : MemberAccessTranslation
        {
            public PropertyGetterTranslation(
                MethodCallExpression getterCall,
                PropertyInfo property,
                ITranslationContext context)
                : base(
                    GetSubjectOrNull(getterCall.Object, property, context),
                    property.Name,
                    property.PropertyType)
            {
            }
        }

        private class PropertySetterTranslation : AssignmentTranslation
        {
            public PropertySetterTranslation(
                MethodCallExpression setterCall,
                ITranslation getterTranslation,
                ITranslationContext context)
                : base(Assign, getterTranslation, setterCall.Arguments.First(), context)
            {
            }
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

            public void WriteTo(TranslationBuffer buffer)
            {
                _subjectTranslation.WriteInParenthesesIfRequired(buffer);

                if (_isPartOfMethodCallChain)
                {
                    buffer.WriteNewLineToTranslation();
                    buffer.Indent();
                }

                buffer.WriteToTranslation('.');
                _methodInvocationTranslatable.WriteTo(buffer);

                if (_isPartOfMethodCallChain)
                {
                    buffer.Unindent();
                }
            }
        }

        private class MethodInvocationTranslatable : ITranslatable
        {
            private readonly IMethod _method;
            private readonly ParameterSetTranslation _parameters;
            private readonly ITranslatable[] _explicitGenericArguments;

            public MethodInvocationTranslatable(IMethod method, ParameterSetTranslation parameters, ITranslationContext context)
            {
                _method = method;
                _parameters = parameters;
                _explicitGenericArguments = GetRequiredExplicitGenericArguments(context, out var totalLength);
                EstimatedSize = method.Name.Length + totalLength + parameters.EstimatedSize;
            }

            private ITranslatable[] GetRequiredExplicitGenericArguments(ITranslationContext context, out int totalLength)
            {
                if (!_method.IsGenericMethod)
                {
                    totalLength = 0;
                    return Enumerable<ITranslatable>.EmptyArray;
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
                    return Enumerable<ITranslatable>.EmptyArray;
                }

                var argumentsLength = 0;

                var arguments = _method
                    .GetGenericArguments()
                    .Project(argumentType =>
                    {
                        if (argumentType.FullName == null)
                        {
                            return null;
                        }

                        ITranslatable argumentTypeTranslation = context.GetTranslationFor(argumentType);

                        argumentsLength += argumentTypeTranslation.EstimatedSize + 2;

                        return argumentTypeTranslation;
                    })
                    .Filter(argument => argument != null)
                    .ToArray();

                totalLength = argumentsLength;

                return (totalLength != 0) ? arguments : Enumerable<ITranslatable>.EmptyArray;
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

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation(_method.Name);
                WriteGenericArgumentNamesIfNecessary(buffer);
                _parameters.WriteTo(buffer);
            }

            private void WriteGenericArgumentNamesIfNecessary(TranslationBuffer buffer)
            {
                if (_explicitGenericArguments.Length == 0)
                {
                    return;
                }

                buffer.WriteToTranslation('<');

                for (int i = 0, l = _explicitGenericArguments.Length; ;)
                {
                    _explicitGenericArguments[i++].WriteTo(buffer);

                    if (i == l)
                    {
                        break;
                    }

                    buffer.WriteToTranslation(", ");
                }

                buffer.WriteToTranslation('>');
            }
        }
    }
}
