namespace AgileObjects.ReadableExpressions.Translations
{
    internal static class TranslationExtensions
    {
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

            translation.WriteTo(context);
        }

        public static void WriteSpaceToTranslation(this ITranslationContext context)
        {
            context.WriteToTranslation(' ');
        }
    }
}