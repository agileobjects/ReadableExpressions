namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using ElementInit = Microsoft.Scripting.Ast.ElementInit;
    using MemberAssignment = Microsoft.Scripting.Ast.MemberAssignment;
    using MemberBinding = Microsoft.Scripting.Ast.MemberBinding;
    using MemberListBinding = Microsoft.Scripting.Ast.MemberListBinding;
    using MemberMemberBinding = Microsoft.Scripting.Ast.MemberMemberBinding;
    using MemberBindingType = Microsoft.Scripting.Ast.MemberBindingType;
    using MemberInitExpression = Microsoft.Scripting.Ast.MemberInitExpression;
    using NewExpression = Microsoft.Scripting.Ast.NewExpression;
#endif
    using Extensions;

    internal partial struct InitialisationExpressionTranslator
    {
        private class MemberInitExpressionHelper : InitExpressionHelperBase<MemberInitExpression, NewExpression>
        {
            private readonly Dictionary<MemberBindingType, Func<MemberBinding, TranslationContext, string>> _bindingTranslatorsByType;

            public MemberInitExpressionHelper()
                : base(exp => exp.NewExpression, exp => !exp.Arguments.Any())
            {
                _bindingTranslatorsByType = new Dictionary<MemberBindingType, Func<MemberBinding, TranslationContext, string>>
                {
                    { MemberBindingType.Assignment, TranslateAssignmentBinding },
                    { MemberBindingType.ListBinding, TranslateListBinding },
                    { MemberBindingType.MemberBinding, TranslateMemberBinding }
                };
            }

            protected override IEnumerable<string> GetMemberInitialisations(MemberInitExpression initialisation, TranslationContext context)
                => GetInitialisations(initialisation.Bindings, context);

            private string[] GetInitialisations(IEnumerable<MemberBinding> memberBindings, TranslationContext context)
            {
                return memberBindings
                    .Project(b => _bindingTranslatorsByType[b.BindingType].Invoke(b, context))
                    .ToArray();
            }

            private static string TranslateAssignmentBinding(MemberBinding binding, TranslationContext context)
            {
                var assignment = (MemberAssignment)binding;
                var value = context.TranslateAsCodeBlock(assignment.Expression);

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
                    .Project(init => IsStandardAddMethod(init)
                        ? context.TranslateAsCodeBlock(init.Arguments.First())
                        : MethodCallExpressionTranslator.GetMethodCall(new BclMethodInfoWrapper(init.AddMethod), init.Arguments, context))
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
