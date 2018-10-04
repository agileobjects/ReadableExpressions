namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface ITranslationContext
    {
        TranslationSettings Settings { get; }

        int? GetUnnamedVariableNumber(ParameterExpression variable);

        ITranslation GetTranslationFor(Type type);

        ITranslation GetTranslationFor(Expression expression);

        void WriteToTranslation(char character);

        void WriteToTranslation(string stringValue);

        void WriteToTranslation(int intValue);

        void WriteToTranslation(object value);
    }

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