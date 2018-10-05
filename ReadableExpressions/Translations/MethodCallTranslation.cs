namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
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
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;
        private readonly Action<ITranslationContext> _translationWriter;

        public MethodCallTranslation(MethodCallExpression methodCall, ITranslationContext context)
        {
            _methodCall = methodCall;

            _subject =
                context.GetTranslationFor(methodCall.GetSubject()) ??
                context.GetTranslationFor(methodCall.Method.DeclaringType);

            _parameters = new ParameterSetTranslation(
                new BclMethodInfoWrapper(methodCall.Method),
                methodCall.Arguments,
                context);

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
            _parameters.WriteTo(context);
        }

        public void WriteTo(ITranslationContext context) => _translationWriter.Invoke(context);
    }
}
