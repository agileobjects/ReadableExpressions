using AgileObjects.ReadableExpressions.Translators;

namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class MethodCallTranslation : ITranslation
    {
        private readonly MethodCallExpression _methodCall;
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;

        public MethodCallTranslation(MethodCallExpression methodCall, ITranslationContext context)
        {
            _methodCall = methodCall;

            _subject = context.GetTranslationFor(methodCall.GetSubject());

            _parameters = new ParameterSetTranslation(
                new BclMethodInfoWrapper(methodCall.Method),
                methodCall.Arguments,
                context).WithParentheses();

            EstimatedSize = _subject.EstimatedSize + ".".Length + _parameters.EstimatedSize;
        }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _subject.WriteTo(context);
            context.WriteToTranslation('.');
            context.WriteToTranslation(_methodCall.Method.Name);
            _parameters.WriteTo(context);
        }
    }
}
