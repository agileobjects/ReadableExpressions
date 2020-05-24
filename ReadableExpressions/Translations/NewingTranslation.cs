namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using NetStandardPolyfills;
    using Interfaces;

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

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteNewToTranslation();
            _typeNameTranslation.WriteTo(buffer);
            Parameters.WriteTo(buffer);
        }

        public Type Type => _typeNameTranslation.Type;
    }
}