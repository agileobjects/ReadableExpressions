namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface ITranslationContext
    {
        TranslationSettings Settings { get; }

        int? GetUnnamedVariableNumber(ParameterExpression variable);

        ITranslation GetTranslationFor(Expression expression);

        void WriteToTranslation(char character);

        void WriteToTranslation(string stringValue);
    }
}