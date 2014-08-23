namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    internal class MethodCallExpressionTranslator : ExpressionTranslatorBase
    {
        internal MethodCallExpressionTranslator()
            : base(ExpressionType.Call)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var methodCall = (MethodCallExpression)expression;
            var methodCallSubject = GetMethodCallSuject(methodCall, translatorRegistry);
            
            var parameters = TranslationHelper.GetParameters(
                methodCall.Arguments, 
                translatorRegistry,
                encloseSingleParameterInBrackets: true);

            return methodCallSubject + "." + methodCall.Method.Name + parameters;
        }

        private static string GetMethodCallSuject(
            MethodCallExpression methodCall,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            if (methodCall.Object != null)
            {
                return translatorRegistry.Translate(methodCall.Object);
            }

            return methodCall.Method.DeclaringType.Name;
        }
    }
}