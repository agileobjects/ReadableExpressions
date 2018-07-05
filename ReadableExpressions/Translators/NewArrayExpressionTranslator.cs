namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using NewArrayExpression = Microsoft.Scripting.Ast.NewArrayExpression;
#endif
    using Extensions;

    internal class NewArrayExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewArrayExpressionTranslator()
            : base(ExpressionType.NewArrayBounds)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var newArray = (NewArrayExpression)expression;

            var arrayTypeName = expression.Type.GetElementType().GetFriendlyName();

            var bounds = newArray.Expressions.Select(context.Translate).Join("[]");

            return $"new {arrayTypeName}[{bounds}]";
        }
    }
}