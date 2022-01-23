#if FEATURE_VALUE_TUPLE
namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq.Expressions;

    internal class ValueTupleNewingTranslation : NewingTranslationBase, ITranslation
    {
        public ValueTupleNewingTranslation(NewExpression newing, ITranslationContext context)
            : base(newing, context)
        {
            Type = newing.Type;
            Parameters.WithParentheses();

            TranslationSize = Parameters.TranslationSize + 2;
        }

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize => Parameters.FormattingSize;

        public int GetIndentSize() => Parameters.GetIndentSize();

        public int GetLineCount() => Parameters.GetLineCount();

        public void WriteTo(TranslationWriter writer)
            => Parameters.WriteTo(writer);
    }
}
#endif