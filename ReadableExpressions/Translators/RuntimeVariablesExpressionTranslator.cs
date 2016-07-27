namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class RuntimeVariablesExpressionTranslator : ExpressionTranslatorBase
    {
        public RuntimeVariablesExpressionTranslator()
            : base(ExpressionType.RuntimeVariables)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var runtimeVariables = (RuntimeVariablesExpression)expression;

            var translated = context
                .TranslateParameters(runtimeVariables.Variables)
                .WithParenthesesIfNecessary();

            return translated;
        }
    }
}