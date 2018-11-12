﻿namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MemberInitialisationTranslation : InitialisationTranslationBase<MemberBinding>
    {
        private MemberInitialisationTranslation(MemberInitExpression memberInit, ITranslationContext context)
            : base(
                ExpressionType.MemberInit,
                memberInit.NewExpression,
                memberInit.Bindings,
                GetMemberBindingTranslation,
                context)
        {
        }

        private static ITranslatable GetMemberBindingTranslation(MemberBinding binding, ITranslationContext context)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.MemberBinding:

                    break;

                case MemberBindingType.ListBinding:

                    break;

                default:
                    return new AssignmentBindingTranslatable((MemberAssignment)binding, context);
            }

            return null;
        }

        public static ITranslation For(MemberInitExpression memberInit, ITranslationContext context)
        {
            if (InitHasNoInitializers(memberInit.NewExpression, memberInit.Bindings, context, out var newingTranslation))
            {
                return newingTranslation.WithNodeType(ExpressionType.MemberInit);
            }

            return new MemberInitialisationTranslation(memberInit, context);
        }

        protected override bool WriteLongTranslationsToMultipleLines => true;

        private class AssignmentBindingTranslatable : ITranslatable
        {
            private readonly string _memberName;
            private readonly ITranslation _valueTranslation;

            public AssignmentBindingTranslatable(MemberAssignment assignment, ITranslationContext context)
            {
                _memberName = assignment.Member.Name;
                _valueTranslation = context.GetCodeBlockTranslationFor(assignment.Expression);
                EstimatedSize = _memberName.Length + 4 + _valueTranslation.EstimatedSize;
            }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation(_memberName);
                context.WriteToTranslation(" = ");
                _valueTranslation.WriteTo(context);
            }
        }
    }
}