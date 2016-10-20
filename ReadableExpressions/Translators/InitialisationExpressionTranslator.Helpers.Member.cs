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
            private readonly Dictionary<MemberBindingType, Func<MemberBinding, TranslationContext, string>> _bindingTranslatorsByType;

            public MemberInitExpressionHelper(MethodCallExpressionTranslator methodCallTranslator)
                : base(exp => exp.NewExpression, exp => !exp.Arguments.Any())
            {
                _methodCallTranslator = methodCallTranslator;

                _bindingTranslatorsByType = new Dictionary<MemberBindingType, Func<MemberBinding, TranslationContext, string>>
                {
                    { MemberBindingType.Assignment, TranslateAssignmentBinding },
                    { MemberBindingType.ListBinding, TranslateListBinding },
                    { MemberBindingType.MemberBinding, TranslateMemberBinding }
                };
            }

            protected override IEnumerable<string> GetMemberInitialisations(MemberInitExpression initialisation, TranslationContext context)
            {
                return GetInitialisations(initialisation.Bindings, context);
            }

            private string[] GetInitialisations(IEnumerable<MemberBinding> memberBindings, TranslationContext context)
            {
                return memberBindings
                    .Select(b => _bindingTranslatorsByType[b.BindingType].Invoke(b, context))
                    .ToArray();
            }

            private static string TranslateAssignmentBinding(MemberBinding binding, TranslationContext context)
            {
                var assignment = (MemberAssignment)binding;
                var value = context.Translate(assignment.Expression);

                return assignment.Member.Name + " = " + value;
            }

            private string TranslateMemberBinding(MemberBinding binding, TranslationContext context)
            {
                var memberBinding = (MemberMemberBinding)binding;

                var subBindings = GetInitialisations(memberBinding.Bindings, context);

                return GetInitialisation(memberBinding.Member.Name + " =", subBindings);
            }

            private string TranslateListBinding(MemberBinding binding, TranslationContext context)
            {
                var listBinding = (MemberListBinding)binding;

                var listInitialisers = listBinding
                    .Initializers
                    .Select(init => IsStandardAddMethod(init)
                        ? context.TranslateAsCodeBlock(init.Arguments.First())
                        : _methodCallTranslator.GetMethodCall(new BclMethodInfoWrapper(init.AddMethod), init.Arguments, context))
                    .ToArray();

                return GetInitialisation(listBinding.Member.Name + " =", listInitialisers);
            }

            private static bool IsStandardAddMethod(ElementInit init)
            {
                return (init.AddMethod.Name == "Add") && (init.Arguments.Count == 1);
            }
        }
    }
}
