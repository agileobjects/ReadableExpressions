namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, IInitExpressionHelper> _helpersByNodeType =
            new Dictionary<ExpressionType, IInitExpressionHelper>
            {
                { ExpressionType.ListInit, new ListInitExpressionHelper() },
                { ExpressionType.MemberInit, new MemberInitExpressionHelper() },
                { ExpressionType.NewArrayInit, new ArrayInitExpressionHelper() }
            };

        internal InitialisationExpressionTranslator()
            : base(_helpersByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            return _helpersByNodeType[expression.NodeType].Translate(expression, translatorRegistry);
        }
    }
}