namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;

    internal class RuntimeVariablesExpressionTranslator : ExpressionTranslatorBase
    {
        public RuntimeVariablesExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.RuntimeVariables)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var runtimeVariables = (RuntimeVariablesExpression)expression;

            var translated = GetTranslatedParameters(runtimeVariables.Variables, context)
                .WithParenthesesIfNecessary();

            return translated;
        }
    }
}