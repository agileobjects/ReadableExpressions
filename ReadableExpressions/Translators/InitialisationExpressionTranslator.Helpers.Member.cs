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
            private readonly Dictionary<MemberBindingType, Func<MemberBinding, string>> _bindingTranslatorsByType;

            public MemberInitExpressionHelper(IExpressionTranslatorRegistry registry)
                : base(registry)
            {
                _bindingTranslatorsByType = new Dictionary<MemberBindingType, Func<MemberBinding, string>>
                {
                    { MemberBindingType.Assignment, TranslateAssignmentBinding }
                };
            }

            protected override NewExpression GetNewExpression(MemberInitExpression expression)
            {
                return expression.NewExpression;
            }

            protected override bool ConstructorIsParameterless(NewExpression newExpression)
            {
                return !newExpression.Arguments.Any();
            }

            protected override IEnumerable<string> GetInitialisations(
                MemberInitExpression expression)
            {
                return expression.Bindings.Select(b => _bindingTranslatorsByType[b.BindingType].Invoke(b));
            }

            private string TranslateAssignmentBinding(MemberBinding binding)
            {
                var assignment = (MemberAssignment)binding;
                var value = Registry.Translate(assignment.Expression);

                return assignment.Member.Name + " = " + value;
            }
        }
    }
}
