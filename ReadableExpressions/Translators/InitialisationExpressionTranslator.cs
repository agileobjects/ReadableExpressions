namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<ExpressionType, IInitExpressionHelper> _helpersByNodeType;

        internal InitialisationExpressionTranslator(
            MethodCallExpressionTranslator methodCallTranslator,
            IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.ListInit, ExpressionType.MemberInit, ExpressionType.NewArrayInit)
        {
            _helpersByNodeType = new Dictionary<ExpressionType, IInitExpressionHelper>
            {
                [ExpressionType.ListInit] = new ListInitExpressionHelper(methodCallTranslator, registry),
                [ExpressionType.MemberInit] = new MemberInitExpressionHelper(methodCallTranslator, registry),
                [ExpressionType.NewArrayInit] = new ArrayInitExpressionHelper(registry)
            };
        }

        public override string Translate(Expression expression)
        {
            return _helpersByNodeType[expression.NodeType].Translate(expression);
        }
    }
}