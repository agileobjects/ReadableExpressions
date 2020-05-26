namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

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
                    var memberBinding = (MemberMemberBinding)binding;

                    return new BindingsTranslation(
                        this,
                        memberBinding.Member.Name,
                        new MemberBindingInitializerTranslationSet(memberBinding.Bindings, context));

                case MemberBindingType.ListBinding:
                    var listBinding = (MemberListBinding)binding;

                    return new BindingsTranslation(
                        this,
                        listBinding.Member.Name,
                        new ListInitializerSetTranslation(listBinding.Initializers, context));

                default:
                    return new AssignmentBindingTranslation((MemberAssignment)binding, context);
            }
        }

        public override bool ForceWriteToMultipleLines => false;

        public class BindingsTranslation : ITranslatable
        {
            private readonly IInitializerSetTranslation _parent;
            private readonly string _subjectName;
            private readonly IInitializerSetTranslation _initializerSetTranslation;

            public BindingsTranslation(
                IInitializerSetTranslation parent,
                string subjectName,
                IInitializerSetTranslation initializerSetTranslation)
            {
                _parent = parent;
                _subjectName = subjectName;
                _initializerSetTranslation = initializerSetTranslation;
                TranslationSize = subjectName.Length + 2 + initializerSetTranslation.TranslationSize;
            }

            public int TranslationSize { get; }

            public int FormattingSize => _initializerSetTranslation.FormattingSize;

            public int GetIndentSize()
            {
                _initializerSetTranslation.IsLongTranslation = _parent.IsLongTranslation;

                return _initializerSetTranslation.GetIndentSize();
            }

            public int GetLineCount()
            {
                var isLongBindingsSet = _parent.IsLongTranslation;

                _initializerSetTranslation.IsLongTranslation = isLongBindingsSet;

                var bindingsLineCount = _initializerSetTranslation.GetLineCount();

                if (isLongBindingsSet)
                {
                    ++bindingsLineCount;
                }

                return bindingsLineCount;
            }

            public void WriteTo(TranslationWriter writer)
            {
                _initializerSetTranslation.IsLongTranslation = _parent.IsLongTranslation;

                writer.WriteToTranslation(_subjectName);
                writer.WriteToTranslation(" =");
                _initializerSetTranslation.WriteTo(writer);
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
                TranslationSize = _memberName.Length + 4 + _valueTranslation.TranslationSize;
            }

            public int TranslationSize { get; }

            public int FormattingSize => _valueTranslation.FormattingSize;

            public int GetIndentSize() => _valueTranslation.GetIndentSize();

            public int GetLineCount() => _valueTranslation.GetLineCount();

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteToTranslation(_memberName);
                writer.WriteToTranslation(" = ");
                _valueTranslation.WriteTo(writer);
            }
        }
    }
}