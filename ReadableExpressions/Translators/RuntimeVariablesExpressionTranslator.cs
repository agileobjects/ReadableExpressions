namespace AgileObjects.ReadableExpressions.Translators
{
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using RuntimeVariablesExpression = Microsoft.Scripting.Ast.RuntimeVariablesExpression;

#endif

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