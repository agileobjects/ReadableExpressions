namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;
    using Formatting;

    internal class RuntimeVariablesExpressionTranslator : ExpressionTranslatorBase
    {
        public RuntimeVariablesExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.RuntimeVariables)
        {
        }

        public override string Translate(Expression expression)
        {
            var runtimeVariables = (RuntimeVariablesExpression)expression;

            var translated = Registry
                .TranslateParameters(runtimeVariables.Variables)
                .WithBracketsIfNecessary();

            return translated;
        }
    }
}