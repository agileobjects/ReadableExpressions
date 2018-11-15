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
        public FixedTerminatedValueTranslation(ExpressionType expressionType, string value, Type type)
            : base(expressionType, value, type)
        {
        }

        public bool IsTerminated => true;
    }
}