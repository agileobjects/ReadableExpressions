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
    using Translators;
    using Extensions;
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

            return new StandardMethodCallTranslation(Invoke, subject, method, parameters);
        }

        public static ITranslation For(MethodCallExpression methodCall, ITranslationContext context)
        {
            var method = new BclMethodWrapper(methodCall.Method);
            var parameters = new ParameterSetTranslation(method, methodCall.Arguments, context);

            if (methodCall.Method.IsImplicitOperator())
            {
                return new TranslationWrapper(new CodeBlockTranslation(parameters[0])).WithNodeType(Call);
            }

            var subject =
                context.GetTranslationFor(methodCall.GetSubject()) ??
                context.GetTranslationFor(methodCall.Method.DeclaringType);

            if (IsIndexedPropertyAccess(methodCall))
            {
                return new IndexAccessTranslation(subject, parameters);
            }

            parameters = parameters.WithParentheses();

            if (methodCall.Method.IsExplicitOperator())
            {
                return CastTranslation.ForExplicitOperator(
                    parameters[0],
                    context.GetTranslationFor(methodCall.Method.ReturnType));
            }

            if (subject.IsBinary())
            {
                subject = subject.WithParentheses();
            }

            return new StandardMethodCallTranslation(Call, subject, method, parameters);
        }

        public static ITranslation ForCustomMethodCast(
            ITranslation typeNameTranslation,
            IMethod castMethod,
            ITranslation castValue)
        {
            return new StandardMethodCallTranslation(
                Call,
                typeNameTranslation,
                castMethod,
                new ParameterSetTranslation(castValue).WithParentheses());
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

        private class StandardMethodCallTranslation : ITranslation
        {
            private readonly IMethod _method;
            private readonly ITranslation _subject;
            private readonly ParameterSetTranslation _parameters;

            public StandardMethodCallTranslation(
                ExpressionType nodeType,
                ITranslation subject,
                IMethod method,
                ParameterSetTranslation parameters)
            {
                NodeType = nodeType;
                _method = method;
                _subject = subject;
                _parameters = parameters;
                EstimatedSize = GetEstimatedSize();
            }

            public ExpressionType NodeType { get; }

            public int EstimatedSize { get; }

            private int GetEstimatedSize()
                => _subject.EstimatedSize + _method.Name.Length + ".".Length + _parameters.EstimatedSize;

            public void WriteTo(ITranslationContext context)
            {

                _subject.WriteTo(context);
                context.WriteToTranslation('.');
                context.WriteToTranslation(_method.Name);
                WriteGenericArgumentsIfNecessary(context);
                _parameters.WriteTo(context);
            }

            private void WriteGenericArgumentsIfNecessary(ITranslationContext context)
            {
                if (!_method.IsGenericMethod)
                {
                    return;
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
                    return;
                }

                var argumentNames = _method
                    .GetGenericArguments()
                    .Project(a => a.GetFriendlyName(context.Settings))
                    .Filter(name => name != null)
                    .ToArray();

                if (argumentNames.Length == 0)
                {
                    return;
                }

                context.WriteToTranslation('<');

                for (int i = 0, l = argumentNames.Length - 1; ; ++i)
                {
                    context.WriteToTranslation(argumentNames[i]);

                    if (i == l)
                    {
                        break;
                    }

                    context.WriteToTranslation(", ");
                }

                context.WriteToTranslation('>');
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
        }
    }
}
