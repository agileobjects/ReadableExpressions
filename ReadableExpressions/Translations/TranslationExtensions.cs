namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

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
            bool writeParentheses;

            switch (translation.NodeType)
            {
                case Assign:
                case Equal:
                case NotEqual:
                case TypeEqual:
                    writeParentheses = true;
                    break;

                default:
                    writeParentheses = false;
                    break;
            }

            if (writeParentheses)
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