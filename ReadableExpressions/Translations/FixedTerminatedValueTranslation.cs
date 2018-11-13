namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class FixedTerminatedValueTranslation : FixedValueTranslation, IPotentialSelfTerminatingTranslatable
    {
        public FixedTerminatedValueTranslation(ExpressionType expressionType, string value)
            : base(expressionType, value)
        {
        }

        public bool IsTerminated => true;
    }
}