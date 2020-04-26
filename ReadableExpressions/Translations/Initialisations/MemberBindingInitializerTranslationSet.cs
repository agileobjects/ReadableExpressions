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
                    return new MemberBindingTranslation((MemberMemberBinding)binding, this, context);

                case MemberBindingType.ListBinding:
                    return new ListBindingTranslation((MemberListBinding)binding, this, context);

                default:
                    return new AssignmentBindingTranslation((MemberAssignment)binding, context);
            }
        }

        public override bool ForceWriteToMultipleLines => false;

        private class MemberBindingTranslation : ITranslatable
        {
            private readonly string _memberName;
            private readonly MemberBindingInitializerTranslationSet _bindingTranslations;
            private readonly MemberBindingInitializerTranslationSet _parent;

            public MemberBindingTranslation(
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

            public void WriteTo(TranslationBuffer buffer)
            {
                _bindingTranslations.IsLongTranslation = _parent.IsLongTranslation;

                buffer.WriteToTranslation(_memberName);
                buffer.WriteToTranslation(" =");
                _bindingTranslations.WriteTo(buffer);
            }
        }

        private class ListBindingTranslation : ITranslatable
        {
            private readonly string _memberName;
            private readonly ListInitializerSetTranslation _initializerTranslations;
            private readonly MemberBindingInitializerTranslationSet _parent;

            public ListBindingTranslation(
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

            public void WriteTo(TranslationBuffer buffer)
            {
                _initializerTranslations.IsLongTranslation = _parent.IsLongTranslation;

                buffer.WriteToTranslation(_memberName);
                buffer.WriteToTranslation(" =");
                _initializerTranslations.WriteTo(buffer);
            }
        }

        private class AssignmentBindingTranslation : ITranslatable
        {
            private readonly string _memberName;
            private readonly ITranslation _valueTranslation;

            public AssignmentBindingTranslation(MemberAssignment assignment, ITranslationContext context)
            {
                _memberName = assignment.Member.Name;
                _valueTranslation = context.GetCodeBlockTranslationFor(assignment.Expression);
                EstimatedSize = _memberName.Length + 4 + _valueTranslation.EstimatedSize;
            }

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation(_memberName);
                buffer.WriteToTranslation(" = ");
                _valueTranslation.WriteTo(buffer);
            }
        }
    }
}