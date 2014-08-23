namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator
    {
        private class MemberInitExpressionHelper : InitExpressionHelperBase<MemberInitExpression, NewExpression>
        {
            private static readonly Dictionary<MemberBindingType, Func<MemberBinding, IExpressionTranslatorRegistry, string>> _bindingTranslatorsByType =
                new Dictionary<MemberBindingType, Func<MemberBinding, IExpressionTranslatorRegistry, string>>
                {
                    { MemberBindingType.Assignment, TranslateAssignmentBinding }
                };

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
                return expression.Bindings.Select(b => _bindingTranslatorsByType[b.BindingType].Invoke(b, translatorRegistry));
            }

            private static string TranslateAssignmentBinding(MemberBinding binding, IExpressionTranslatorRegistry translatorRegistry)
            {
                var assignment = (MemberAssignment)binding;
                var value = translatorRegistry.Translate(assignment.Expression);

                return assignment.Member.Name + " = " + value;
            }
        }
    }
}
