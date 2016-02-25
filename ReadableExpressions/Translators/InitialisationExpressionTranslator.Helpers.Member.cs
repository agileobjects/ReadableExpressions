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
            private readonly MethodCallExpressionTranslator _methodCallTranslator;
            private readonly Dictionary<MemberBindingType, Func<MemberBinding, string>> _bindingTranslatorsByType;

            public MemberInitExpressionHelper(
                MethodCallExpressionTranslator methodCallTranslator,
                IExpressionTranslatorRegistry registry)
                : base(registry)
            {
                _methodCallTranslator = methodCallTranslator;

                _bindingTranslatorsByType = new Dictionary<MemberBindingType, Func<MemberBinding, string>>
                {
                    { MemberBindingType.Assignment, TranslateAssignmentBinding },
                    { MemberBindingType.ListBinding, TranslateListBinding },
                    { MemberBindingType.MemberBinding, TranslateMemberBinding }
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

            protected override IEnumerable<string> GetInitialisations(MemberInitExpression expression)
            {
                return GetInitialisations(expression.Bindings);
            }

            private string[] GetInitialisations(IEnumerable<MemberBinding> memberBindings)
            {
                return memberBindings
                    .Select(b => _bindingTranslatorsByType[b.BindingType].Invoke(b))
                    .ToArray();
            }

            private string TranslateAssignmentBinding(MemberBinding binding)
            {
                var assignment = (MemberAssignment)binding;
                var value = Registry.Translate(assignment.Expression);

                return assignment.Member.Name + " = " + value;
            }

            private string TranslateMemberBinding(MemberBinding binding)
            {
                var memberBinding = (MemberMemberBinding)binding;

                var subBindings = GetInitialisations(memberBinding.Bindings);

                return GetInitialisations(subBindings, memberBinding.Member.Name + " =");
            }

            private string TranslateListBinding(MemberBinding binding)
            {
                var listBinding = (MemberListBinding)binding;

                var listInitialisers = listBinding
                    .Initializers
                    .Select(init => IsStandardAddMethod(init)
                        ? Registry.Translate(init.Arguments.First())
                        : _methodCallTranslator.GetMethodCall(init.AddMethod, init.Arguments))
                    .ToArray();

                return GetInitialisations(listInitialisers, listBinding.Member.Name + " =");
            }

            private static bool IsStandardAddMethod(ElementInit init)
            {
                return (init.AddMethod.Name == "Add") && (init.Arguments.Count == 1);
            }
        }
    }
}
