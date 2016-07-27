namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal partial class InitialisationExpressionTranslator
    {
        private class ArrayInitExpressionHelper : InitExpressionHelperBase<NewArrayExpression, NewArrayExpression>
        {
            public ArrayInitExpressionHelper()
                : base(null, null)
            {
            }

            protected override string GetNewExpressionString(NewArrayExpression initialisation, TranslationContext context)
            {
                var explicitType = GetExplicitArrayTypeIfRequired(initialisation);

                return "new" + explicitType + "[]";
            }

            private static string GetExplicitArrayTypeIfRequired(NewArrayExpression initialisation)
            {
                var expressionTypes = initialisation
                    .Expressions
                    .Select(exp => exp.Type)
                    .Distinct()
                    .ToArray();

                if (expressionTypes.Length == 1)
                {
                    return null;
                }

                return " " + initialisation.Type.GetElementType().GetFriendlyName();
            }

            protected override IEnumerable<string> GetMemberInitialisations(
                NewArrayExpression arrayInitialisation,
                TranslationContext context)
            {
                return arrayInitialisation.Expressions.Select(context.Translate);
            }
        }
    }
}
