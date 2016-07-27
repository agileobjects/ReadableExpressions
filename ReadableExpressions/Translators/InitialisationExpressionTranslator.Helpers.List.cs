namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator
    {
        private class ListInitExpressionHelper : InitExpressionHelperBase<ListInitExpression, NewExpression>
        {
            private readonly MethodCallExpressionTranslator _methodCallTranslator;

            public ListInitExpressionHelper(MethodCallExpressionTranslator methodCallTranslator)
                : base(exp => exp.NewExpression, exp => !exp.Arguments.Any())
            {
                _methodCallTranslator = methodCallTranslator;
            }

            protected override IEnumerable<string> GetMemberInitialisations(
                ListInitExpression listInitialisation,
                TranslationContext context)
            {
                return listInitialisation.Initializers
                    .Select(initialisation =>
                    {
                        if (initialisation.Arguments.Count == 1)
                        {
                            return context.Translate(initialisation.Arguments.First());
                        }

                        var additionArguments = string.Join(", ", initialisation.Arguments.Select(context.Translate));

                        return "{ " + additionArguments + " }";
                    });
            }
        }
    }
}
