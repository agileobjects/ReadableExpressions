namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using NetStandardPolyfills;

    internal class NewingTranslation : NewingTranslationBase, ITranslation
    {
        private readonly ITranslation _typeNameTranslation;

        private NewingTranslation(
            NewExpression newing,
            ITranslationContext context,
            bool omitParenthesesIfParameterless)
            : base(newing, context)
        {
            _typeNameTranslation = context.GetTranslationFor(newing.Type).WithObjectTypeName();

            if (omitParenthesesIfParameterless && Parameters.None)
            {
                Parameters.WithoutParentheses();
            }
            else
            {
                Parameters.WithParentheses();
            }

            TranslationSize =
                "new ".Length +
                _typeNameTranslation.TranslationSize +
                Parameters.TranslationSize;

            FormattingSize =
                context.GetKeywordFormattingSize() +
                _typeNameTranslation.FormattingSize +
                Parameters.FormattingSize;
        }

        public static ITranslation For(
            NewExpression newing,
            ITranslationContext context,
            bool omitParenthesesIfParameterless = false)
        {
            if (newing.Type.IsAnonymous())
            {
                return new AnonymousTypeNewingTranslation(newing, context);
            }

#if FEATURE_VALUE_TUPLE
            if (newing.Type.IsValueTuple())
            {
                return new ValueTupleNewingTranslation(newing, context);
            }
#endif
            return new NewingTranslation(newing, context, omitParenthesesIfParameterless);
        }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            return Parameters.None
                ? _typeNameTranslation.GetIndentSize()
                : Parameters.GetIndentSize();
        }

        public int GetLineCount()
        {
            return Parameters.None
                ? _typeNameTranslation.GetLineCount()
                : Parameters.GetLineCount();
        }

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteNewToTranslation();
            _typeNameTranslation.WriteTo(writer);
            Parameters.WriteTo(writer);
        }

        public Type Type => _typeNameTranslation.Type;
    }
}