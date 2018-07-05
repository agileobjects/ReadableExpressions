namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;

#endif

    internal partial struct InitialisationExpressionTranslator : IExpressionTranslator
    {
        private static readonly Dictionary<ExpressionType, IInitExpressionHelper> _helpersByNodeType =
            new Dictionary<ExpressionType, IInitExpressionHelper>
            {
                [ExpressionType.ListInit] = new ListInitExpressionHelper(),
                [ExpressionType.MemberInit] = new MemberInitExpressionHelper(),
                [ExpressionType.NewArrayInit] = new ArrayInitExpressionHelper()
            };

        public IEnumerable<ExpressionType> NodeTypes
        {
            get
            {
                yield return ExpressionType.ListInit;
                yield return ExpressionType.MemberInit;
                yield return ExpressionType.NewArrayInit;
            }
        }

        public string Translate(Expression expression, TranslationContext context)
            => _helpersByNodeType[expression.NodeType].Translate(expression, context);
    }
}