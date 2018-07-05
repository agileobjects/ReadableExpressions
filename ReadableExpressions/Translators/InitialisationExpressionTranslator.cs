namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;

#endif

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
            => _helpersByNodeType[expression.NodeType].Translate(expression, context);
    }
}