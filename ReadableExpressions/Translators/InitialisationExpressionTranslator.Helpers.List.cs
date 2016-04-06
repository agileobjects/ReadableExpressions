namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator
    {
        private class ListInitExpressionHelper : InitExpressionHelperBase<ListInitExpression, NewExpression>
        {
            private readonly MethodCallExpressionTranslator _methodCallTranslator;

            public ListInitExpressionHelper(
                MethodCallExpressionTranslator methodCallTranslator,
                Func<Expression, string> globalTranslator)
                : base(globalTranslator)
            {
                _methodCallTranslator = methodCallTranslator;
            }

            protected override NewExpression GetNewExpression(ListInitExpression expression)
            {
                return expression.NewExpression;
            }

            protected override bool ConstructorIsParameterless(NewExpression newExpression)
            {
                return !newExpression.Arguments.Any();
            }

            protected override IEnumerable<string> GetInitialisations(ListInitExpression expression)
            {
                return expression.Initializers
                    .Select(initialisation =>
                    {
                        var listAddCall = _methodCallTranslator
                            .GetMethodCall(initialisation.AddMethod, initialisation.Arguments);

                        return listAddCall;
                    });
            }
        }
    }
}
