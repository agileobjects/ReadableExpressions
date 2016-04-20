namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<ExpressionType, IInitExpressionHelper> _helpersByNodeType;

        internal InitialisationExpressionTranslator(
            MethodCallExpressionTranslator methodCallTranslator,
            Translator globalTranslator)
            : base(globalTranslator, ExpressionType.ListInit, ExpressionType.MemberInit, ExpressionType.NewArrayInit)
        {
            _helpersByNodeType = new Dictionary<ExpressionType, IInitExpressionHelper>
            {
                [ExpressionType.ListInit] = new ListInitExpressionHelper(methodCallTranslator, globalTranslator),
                [ExpressionType.MemberInit] = new MemberInitExpressionHelper(methodCallTranslator, globalTranslator),
                [ExpressionType.NewArrayInit] = new ArrayInitExpressionHelper(globalTranslator)
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            return _helpersByNodeType[expression.NodeType].Translate(expression, context);
        }
    }
}