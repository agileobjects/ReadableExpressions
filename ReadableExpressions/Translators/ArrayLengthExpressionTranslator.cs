namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using UnaryExpression = Microsoft.Scripting.Ast.UnaryExpression;
#endif

    internal struct ArrayLengthExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.ArrayLength; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var arrayAccess = context.Translate(((UnaryExpression)expression).Operand);

            return arrayAccess + ".Length";
        }
    }
}