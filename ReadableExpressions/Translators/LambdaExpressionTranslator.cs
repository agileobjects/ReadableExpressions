namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class LambdaExpressionTranslator : ExpressionTranslatorBase
    {
        internal LambdaExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Lambda)
        {
        }

        public override string Translate(Expression expression)
        {
            var lambda = (LambdaExpression)expression;

            var parameters = GetTranslatedParameters(lambda.Parameters).WithBracketsIfNecessary();

            var bodyBlock = GetTranslatedExpressionBody(lambda.Body);

            var body = bodyBlock.IsASingleStatement
                ? bodyBlock.AsExpressionBody()
                : bodyBlock.WithBrackets();

            return parameters + " =>" + body;
        }
    }
}