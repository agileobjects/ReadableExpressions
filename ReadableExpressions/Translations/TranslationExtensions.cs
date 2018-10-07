﻿namespace AgileObjects.ReadableExpressions.Translations
{
    internal static class TranslationExtensions
    {
        public static void WriteInParentheses(this ITranslation translation, ITranslationContext context)
        {
            context.WriteToTranslation('(');
            translation.WriteTo(context);
            context.WriteToTranslation(')');
        }
    }
}