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

        Allocation Allocate(int estimatedSize);

        ITranslation GetTranslationFor(Expression expression);
    }

    internal struct Allocation
    {
        public int StartIndex { get; set; }

        public int EndIndex { get; set; }
    }
}