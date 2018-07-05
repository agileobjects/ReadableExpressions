namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using ListInitExpression = Microsoft.Scripting.Ast.ListInitExpression;
    using NewExpression = Microsoft.Scripting.Ast.NewExpression;
#endif

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

                        var additionArguments = initialisation
                            .Arguments
                            .Select(context.TranslateAsCodeBlock)
                            .Join(", ");

                        return "{ " + additionArguments + " }";
                    });
            }
        }
    }
}
