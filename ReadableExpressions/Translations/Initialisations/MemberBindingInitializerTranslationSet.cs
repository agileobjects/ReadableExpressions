namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
    using Interfaces;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MemberBindingInitializerTranslationSet : InitializerSetTranslationBase<MemberBinding>
    {
        public MemberBindingInitializerTranslationSet(IList<MemberBinding> bindings, ITranslationContext context)
            : base(bindings, context)
        {
        }

        protected override ITranslatable GetTranslation(MemberBinding binding, ITranslationContext context)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.MemberBinding:
                    return new MemberBindingTranslatable((MemberMemberBinding)binding, this, context);

                case MemberBindingType.ListBinding:
                    return new ListBindingTranslatable((MemberListBinding)binding, this, context);

                default:
                    return new AssignmentBindingTranslatable((MemberAssignment)binding, context);
            }
        }

        public override bool ForceWriteToMultipleLines => false;

        private class MemberBindingTranslatable : ITranslatable
        {
            private readonly string _memberName;
            private readonly MemberBindingInitializerTranslationSet _bindingTranslations;
            private readonly MemberBindingInitializerTranslationSet _parent;

            public MemberBindingTranslatable(
                MemberMemberBinding memberBinding,
                MemberBindingInitializerTranslationSet parent,
                ITranslationContext context)
            {
                _memberName = memberBinding.Member.Name;
                _bindingTranslations = new MemberBindingInitializerTranslationSet(memberBinding.Bindings, context);
                _parent = parent;
                EstimatedSize = _memberName.Length + 2 + _bindingTranslations.EstimatedSize;
            }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _bindingTranslations.IsLongTranslation = _parent.IsLongTranslation;

                context.WriteToTranslation(_memberName);
                context.WriteToTranslation(" =");
                _bindingTranslations.WriteTo(context);
            }
        }

        private class ListBindingTranslatable : ITranslatable
        {
            private readonly string _memberName;
            private readonly ListInitializerSetTranslation _initializerTranslations;
            private readonly MemberBindingInitializerTranslationSet _parent;

            public ListBindingTranslatable(
                MemberListBinding listBinding, 
                MemberBindingInitializerTranslationSet parent,
                ITranslationContext context)
            {
                _memberName = listBinding.Member.Name;
                _initializerTranslations = new ListInitializerSetTranslation(listBinding.Initializers, context);
                _parent = parent;
                EstimatedSize = _memberName.Length + 2 + _initializerTranslations.EstimatedSize;
            }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _initializerTranslations.IsLongTranslation = _parent.IsLongTranslation;

                context.WriteToTranslation(_memberName);
                context.WriteToTranslation(" =");
                _initializerTranslations.WriteTo(context);
            }
        }

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