namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Translators;
    using Extensions;
    using NetStandardPolyfills;

    internal class MethodCallTranslation : ITranslation
    {
        private readonly MethodCallExpression _methodCall;
        private readonly IMethod _method;
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;
        private readonly Action<ITranslationContext> _translationWriter;

        public MethodCallTranslation(MethodCallExpression methodCall, ITranslationContext context)
        {
            _methodCall = methodCall;
            _method = new BclMethodWrapper(methodCall.Method);

            _subject =
                context.GetTranslationFor(methodCall.GetSubject()) ??
                context.GetTranslationFor(methodCall.Method.DeclaringType);

            _parameters = new ParameterSetTranslation(_method, methodCall.Arguments, context);

            EstimatedSize = _subject.EstimatedSize + ".".Length + _parameters.EstimatedSize;

            if (IsIndexedPropertyAccess())
            {
                _translationWriter = WriteIndexAccess;
                return;
            }

            _parameters = _parameters.WithParentheses();
            _translationWriter = WriteMethodCall;
        }

        private bool IsIndexedPropertyAccess()
        {
            var property = _methodCall
                .Object?
                .Type
                .GetPublicInstanceProperties()
                .FirstOrDefault(p => p.IsIndexer() && p.GetAccessors().Contains(_methodCall.Method));

            return property?.GetIndexParameters().Any() == true;
        }

        public int EstimatedSize { get; }

        private void WriteIndexAccess(ITranslationContext context)
        {
            new IndexAccessTranslation(_subject, _parameters).WriteTo(context);
        }

        private void WriteMethodCall(ITranslationContext context)
        {
            _subject.WriteTo(context);
            context.WriteToTranslation('.');
            context.WriteToTranslation(_methodCall.Method.Name);
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

            for (var i = 0; ; ++i)
            {
                context.WriteToTranslation(argumentNames[i]);

                if (i == (argumentNames.Length - 1))
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

        public void WriteTo(ITranslationContext context) => _translationWriter.Invoke(context);
    }
}
