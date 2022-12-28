namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class MemberBindingInitializerTranslationSet :
    InitializerSetTranslationBase<MemberBinding>
{
    public MemberBindingInitializerTranslationSet(
        IList<MemberBinding> bindings,
        ITranslationContext context) :
        base(bindings, context)
    {
    }

    protected override ITranslation GetTranslationFor(
        MemberBinding binding,
        ITranslationContext context)
    {
        switch (binding.BindingType)
        {
            case MemberBindingType.MemberBinding:
                var memberBinding = (MemberMemberBinding)binding;

                return new BindingsTranslation(
                    memberBinding.Member.Name,
                    new MemberBindingInitializerTranslationSet(
                        memberBinding.Bindings,
                        context));

            case MemberBindingType.ListBinding:
                var listBinding = (MemberListBinding)binding;

                return new BindingsTranslation(
                    listBinding.Member.Name,
                    new ListInitializerSetTranslation(
                        listBinding.Initializers,
                        context));

            default:
                return new AssignmentBindingTranslation(
                    (MemberAssignment)binding, 
                     context);
        }
    }

    public class BindingsTranslation : ITranslation
    {
        private readonly string _subjectName;
        private readonly IInitializerSetTranslation _initializerSetTranslation;

        public BindingsTranslation(
            string subjectName,
            IInitializerSetTranslation initializerSetTranslation)
        {
            initializerSetTranslation.Parent = this;

            _subjectName = subjectName;
            _initializerSetTranslation = initializerSetTranslation;
        }

        public int TranslationLength
            => _subjectName.Length + " =".Length + _initializerSetTranslation.TranslationLength;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteToTranslation(_subjectName);
            writer.WriteToTranslation(" =");
            _initializerSetTranslation.WriteTo(writer);
        }
    }

    private class AssignmentBindingTranslation : ITranslation
    {
        private readonly string _memberName;
        private readonly INodeTranslation _valueTranslation;

        public AssignmentBindingTranslation(
            MemberAssignment assignment,
            ITranslationContext context)
        {
            _memberName = assignment.Member.Name;
            _valueTranslation = context.GetCodeBlockTranslationFor(assignment.Expression);
        }

        public int TranslationLength
            => _memberName.Length + " = ".Length + _valueTranslation.TranslationLength;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteToTranslation(_memberName);
            writer.WriteToTranslation(" = ");
            _valueTranslation.WriteTo(writer);
        }
    }
}