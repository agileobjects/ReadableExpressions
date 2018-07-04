namespace AgileObjects.ReadableExpressions.Translators
{
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using UnaryExpression = Microsoft.Scripting.Ast.UnaryExpression;
#endif

    internal class ArrayLengthExpressionTranslator : ExpressionTranslatorBase
    {
        internal ArrayLengthExpressionTranslator()
            : base(ExpressionType.ArrayLength)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var arrayAccess = context.Translate(((UnaryExpression)expression).Operand);

            return arrayAccess + ".Length";
        }
    }
}