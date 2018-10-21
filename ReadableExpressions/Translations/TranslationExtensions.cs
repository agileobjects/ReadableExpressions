namespace AgileObjects.ReadableExpressions.Translations
{
    internal static class TranslationExtensions
    {
        public static bool IsMultiStatement(this ITranslatable translation)
        {
            return (translation is IPotentialMultiStatementTranslatable multiStatementTranslatable) &&
                    multiStatementTranslatable.IsMultiStatement;
        }

        public static bool IsTerminated(this ITranslatable translation)
        {
            return (translation is IPotentialSelfTerminatingTranslatable selfTerminatedTranslatable) &&
                   selfTerminatedTranslatable.IsTerminated;
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

            if (AssignmentTranslation.IsAssignment(translation.NodeType))
            {
                translation.WriteInParentheses(context);
                return;
            }

            new CodeBlockTranslation(translation).WriteTo(context);
        }

        public static void WriteSpaceToTranslation(this ITranslationContext context)
        {
            context.WriteToTranslation(' ');
        }
    }
}