namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator
    {
        private class MemberInitExpressionHelper : InitExpressionHelperBase<MemberInitExpression, NewExpression>
        {
            protected override NewExpression GetNewExpression(MemberInitExpression expression)
            {
                return expression.NewExpression;
            }

            protected override bool ConstructorIsParameterless(NewExpression newExpression)
            {
                return !newExpression.Arguments.Any();
            }

            protected override IEnumerable<string> GetInitialisations(
                MemberInitExpression expression,
                IExpressionTranslatorRegistry translatorRegistry)
            {
                return expression.Bindings.Select(b => GetMemberBinding(b, translatorRegistry));
            }

            private static string GetMemberBinding(MemberBinding binding, IExpressionTranslatorRegistry translatorRegistry)
            {
                if (binding.BindingType == MemberBindingType.Assignment)
                {
                    var assignment = (MemberAssignment)binding;
                    var value = translatorRegistry.Translate(assignment.Expression);

                    return assignment.Member.Name + " = " + value;
                }

                return null;
            }
        }
    }
}
