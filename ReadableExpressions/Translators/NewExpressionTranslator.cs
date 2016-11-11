namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal class NewExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewExpressionTranslator()
            : base(ExpressionType.New)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var newExpression = (NewExpression)expression;

            if (newExpression.Type.IsAnonymous())
            {
                return GetAnonymousTypeCreation(newExpression, context);
            }

            var typeName = (newExpression.Type == typeof(object)) ? "Object" : newExpression.Type.GetFriendlyName();
            var parameters = context.TranslateParameters(newExpression.Arguments).WithParentheses();

            return "new " + typeName + parameters;
        }
        private string GetAnonymousTypeCreation(NewExpression newExpression, TranslationContext context)
        {
            var constructorParameters = newExpression.Constructor.GetParameters();

            var arguments = newExpression
                .Arguments
                .Select((arg, i) => constructorParameters[i].Name + " = " + context.Translate(arg));

            var argumentsString = string.Join(", ", arguments);

            return "new { " + argumentsString + " }";
        }
    }
}