namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class FixedTerminatedValueTranslation : FixedValueTranslation, IPotentialSelfTerminatingTranslatable
    {
        public FixedTerminatedValueTranslation(
            ExpressionType expressionType, 
            string value, 
            Type type,
            TokenType tokenType)
            : base(expressionType, value, type, tokenType)
        {
        }

        public bool IsTerminated => true;
    }
}