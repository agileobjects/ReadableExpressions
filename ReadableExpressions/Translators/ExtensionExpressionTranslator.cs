namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class ExtensionExpressionTranslator : ExpressionTranslatorBase
    {
        public ExtensionExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Extension)
        {
        }

        public override string Translate(Expression expression)
        {
            return expression.ToString();
        }
    }
}