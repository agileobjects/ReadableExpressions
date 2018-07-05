namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using RuntimeVariablesExpression = Microsoft.Scripting.Ast.RuntimeVariablesExpression;
#endif

    internal struct RuntimeVariablesExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.RuntimeVariables; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var runtimeVariables = (RuntimeVariablesExpression)expression;

            var translated = context
                .TranslateParameters(runtimeVariables.Variables)
                .WithParenthesesIfNecessary();

            return translated;
        }
    }
}