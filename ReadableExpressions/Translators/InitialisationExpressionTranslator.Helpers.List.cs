namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator
    {
        private class ListInitExpressionHelper : InitExpressionHelperBase<ListInitExpression, NewExpression>
        {
            public ListInitExpressionHelper()
                : base(exp => exp.NewExpression, exp => !exp.Arguments.Any())
            {
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
                            return context.TranslateAsCodeBlock(initialisation.Arguments.First());
                        }

                        var additionArguments = string.Join(
                            ", ",
                            initialisation.Arguments.Select(context.TranslateAsCodeBlock));

                        return "{ " + additionArguments + " }";
                    });
            }
        }
    }
}
