namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class NewExpressionTranslator : ExpressionTranslatorBase
    {
        internal NewExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.New)
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
            var parameters = GetTranslatedParameters(newExpression.Arguments, context).WithParentheses();

            return "new " + typeName + parameters;
        }
        private string GetAnonymousTypeCreation(NewExpression newExpression, TranslationContext context)
        {
            var constructorParameters = newExpression.Constructor.GetParameters();

            var arguments = newExpression
                .Arguments
                .Select((arg, i) => constructorParameters[i].Name + " = " + GetTranslation(arg, context));

            var argumentsString = string.Join(", ", arguments);

            return "new { " + argumentsString + " }";
        }
    }
}