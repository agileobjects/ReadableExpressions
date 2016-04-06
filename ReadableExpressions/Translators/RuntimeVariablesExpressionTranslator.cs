namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class RuntimeVariablesExpressionTranslator : ExpressionTranslatorBase
    {
        public RuntimeVariablesExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.RuntimeVariables)
        {
        }

        public override string Translate(Expression expression)
        {
            var runtimeVariables = (RuntimeVariablesExpression)expression;

            var translated = GetTranslatedParameters(runtimeVariables.Variables)
                .WithBracketsIfNecessary();

            return translated;
        }
    }
}