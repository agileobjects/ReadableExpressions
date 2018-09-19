namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using NewArrayExpression = Microsoft.Scripting.Ast.NewArrayExpression;
#endif
    using Extensions;

    internal struct NewArrayExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.NewArrayBounds; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var newArray = (NewArrayExpression)expression;

            var arrayTypeName = expression.Type.GetElementType().GetFriendlyName(context.Settings);

            var bounds = newArray.Expressions.Project(context.Translate).Join("[]");

            return $"new {arrayTypeName}[{bounds}]";
        }
    }
}