namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class TranslationExtensions
    {
        public static CodeBlockTranslation GetCodeBlockFor(this ITranslationContext context, Expression expression)
        {
            return new CodeBlockTranslation(context.GetTranslationFor(expression));
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

            translation.WriteTo(context);
        }

        public static void WriteSpaceToTranslation(this ITranslationContext context)
        {
            context.WriteToTranslation(' ');
        }
    }
}