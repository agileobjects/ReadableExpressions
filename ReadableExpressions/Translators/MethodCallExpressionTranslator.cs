namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

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

            return methodCallSubject + "." + GetMethodCall(methodCall.Method, methodCall.Arguments, translatorRegistry);
        }

        private static string GetMethodCallSuject(
            MethodCallExpression methodCall,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            return (methodCall.Object != null)
                ? translatorRegistry.Translate(methodCall.Object)
                // ReSharper disable once PossibleNullReferenceException
                : methodCall.Method.DeclaringType.Name;
        }

        internal static string GetMethodCall(
            MethodInfo method,
            IEnumerable<Expression> parameters,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var parametersString = TranslationHelper.GetParameters(
                parameters,
                translatorRegistry,
                encloseSingleParameterInBrackets: true);

            return method.Name + parametersString;
        }
    }
}