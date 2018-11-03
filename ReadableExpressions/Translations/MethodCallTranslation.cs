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

    internal class MethodCallTranslation : ITranslation
    {
        private readonly IMethod _method;
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;
        private readonly Action<ITranslationContext> _translationWriter;

        public MethodCallTranslation(InvocationExpression invocation, ITranslationContext context)
        {
            NodeType = Invoke;

            var invocationMethod = invocation.Expression.Type.GetPublicInstanceMethod("Invoke");

            _method = new BclMethodWrapper(invocationMethod);
            _parameters = new ParameterSetTranslation(_method, invocation.Arguments, context).WithParentheses();
            _subject = context.GetTranslationFor(invocation.Expression);

            if (_subject.NodeType == Lambda)
            {
                _subject = _subject.WithParentheses();
            }

            EstimatedSize = GetEstimatedSize();
        }

        public MethodCallTranslation(MethodCallExpression methodCall, ITranslationContext context)
        {
            NodeType = Call;
            _method = new BclMethodWrapper(methodCall.Method);
            _parameters = new ParameterSetTranslation(_method, methodCall.Arguments, context);

            if (methodCall.Method.IsImplicitOperator())
            {
                _subject = new CodeBlockTranslation(_parameters[0]);
                _translationWriter = _subject.WriteTo;
                EstimatedSize = _subject.EstimatedSize;
                return;
            }

            _subject =
                context.GetTranslationFor(methodCall.GetSubject()) ??
                context.GetTranslationFor(methodCall.Method.DeclaringType);

            if (IsIndexedPropertyAccess(methodCall))
            {
                _subject = new IndexAccessTranslation(_subject, _parameters);
                _translationWriter = _subject.WriteTo;
                EstimatedSize = _subject.EstimatedSize;
                return;
            }

            _parameters = _parameters.WithParentheses();

            if (methodCall.Method.IsExplicitOperator())
            {
                var castTypeNameTranslation = context.GetTranslationFor(methodCall.Method.ReturnType);
                _subject = CastTranslation.ForExplicitOperator(_parameters[0], castTypeNameTranslation);
                _translationWriter = _subject.WriteTo;
                EstimatedSize = _subject.EstimatedSize;
                return;
            }

            if (BinaryTranslation.IsBinary(_subject.NodeType))
            {
                _subject = _subject.WithParentheses();
            }

            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize()
            => _subject.EstimatedSize + ".".Length + _parameters.EstimatedSize;

        private MethodCallTranslation(
            ITranslation typeNameTranslation,
            IMethod staticMethod,
            ITranslation castValue)
        {
            NodeType = Call;
            _subject = typeNameTranslation;
            _method = staticMethod;
            _parameters = new ParameterSetTranslation(castValue).WithParentheses();
        }

        public static ITranslation ForCustomMethodCast(
            ITranslation typeNameTranslation,
            IMethod castMethod,
            ITranslation castValue)
        {
            return new MethodCallTranslation(typeNameTranslation, castMethod, castValue);
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

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_translationWriter != null)
            {
                _translationWriter.Invoke(context);
                return;
            }

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
