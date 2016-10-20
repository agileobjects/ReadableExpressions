namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<ExpressionType, IInitExpressionHelper> _helpersByNodeType;

        internal InitialisationExpressionTranslator(MethodCallExpressionTranslator methodCallTranslator)
            : base(ExpressionType.ListInit, ExpressionType.MemberInit, ExpressionType.NewArrayInit)
        {
            _helpersByNodeType = new Dictionary<ExpressionType, IInitExpressionHelper>
            {
                [ExpressionType.ListInit] = new ListInitExpressionHelper(),
                [ExpressionType.MemberInit] = new MemberInitExpressionHelper(methodCallTranslator),
                [ExpressionType.NewArrayInit] = new ArrayInitExpressionHelper()
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            return _helpersByNodeType[expression.NodeType].Translate(expression, context);
        }
    }
}