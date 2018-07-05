namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using LambdaExpression = Microsoft.Scripting.Ast.LambdaExpression;
#endif

    internal struct LambdaExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.Lambda; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var lambda = (LambdaExpression)expression;

            var parameters = context.TranslateParameters(lambda.Parameters).WithParenthesesIfNecessary();
            var bodyBlock = context.TranslateCodeBlock(lambda.Body);

            var body = bodyBlock.IsASingleStatement
                ? bodyBlock.AsExpressionBody()
                : bodyBlock.WithCurlyBraces();

            return parameters + " =>" + body;
        }
    }
}