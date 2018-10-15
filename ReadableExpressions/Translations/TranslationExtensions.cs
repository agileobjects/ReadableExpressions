namespace AgileObjects.ReadableExpressions.Translations
{
    internal static class TranslationExtensions
    {
        public static bool IsMultiStatement(this ITranslatable translation)
        {
            return (translation is IPotentialMultiStatementTranslatable multiStatementTranslatable) &&
                    multiStatementTranslatable.IsMultiStatement;
        }

        public static void WriteOpeningBraceToTranslation(this ITranslationContext context)
        {
            if (context.TranslationQuery(q => !q.TranslationEndsWith('{')))
            {
                context.WriteNewLineToTranslation();
            }

            context.WriteToTranslation('{');
            context.WriteNewLineToTranslation();
            context.Indent();
        }

        public static void WriteClosingBraceToTranslation(this ITranslationContext context)
        {
            context.WriteNewLineToTranslation();
            context.Unindent();
            context.WriteToTranslation('}');
        }

        public static void WriteInParentheses(this ITranslation translation, ITranslationContext context)
        {
            context.WriteToTranslation('(');
            translation.WriteTo(context);
            context.WriteToTranslation(')');
        }

        public static void WriteInParenthesesIfRequired(this ITranslation translation, ITranslationContext context)
        {
            if (BinaryTranslation.IsBinary(translation.NodeType))
            {
                translation.WriteInParentheses(context);
                return;
            }

            context.WriteCodeBlockToTranslation(translation);
        }

        public static void WriteSpaceToTranslation(this ITranslationContext context)
        {
            context.WriteToTranslation(' ');
        }
    }
}