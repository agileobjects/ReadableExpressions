namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class TranslationExtensions
    {
        public static IList<ITranslation> GetTranslationsFor(this ITranslationContext context, IList<Expression> expressions)
        {
            var translations = new ITranslation[expressions.Count];

            for (int i = 0, l = expressions.Count; i < l; i++)
            {
                translations[i] = context.GetTranslationFor(expressions[i]);
            }

            return translations;
        }

        public static void WriteInParentheses(this ITranslation translation, ITranslationContext context)
        {
            context.WriteToTranslation('(');
            translation.WriteTo(context);
            context.WriteToTranslation(')');
        }
    }
}