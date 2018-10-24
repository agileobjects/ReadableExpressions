namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MemberInitialisationTranslation : InitialisationTranslationBase<MemberBinding>
    {
        private readonly IList<ITranslation> _initializerTranslations;

        public MemberInitialisationTranslation(MemberInitExpression memberInit, ITranslationContext context)
            : base(
                ExpressionType.MemberInit,
                memberInit.NewExpression,
                memberInit.Bindings,
                context)
        {
            if (HasNoInitializers)
            {
                return;
            }

            _initializerTranslations = new ITranslation[memberInit.Bindings.Count];

            for (int i = 0, l = _initializerTranslations.Count; ; ++i)
            {
                var binding = memberInit.Bindings[i];

                switch (binding.BindingType)
                {
                    case MemberBindingType.MemberBinding:
                        _initializerTranslations[i] = null;
                        break;

                    case MemberBindingType.ListBinding:
                        _initializerTranslations[i] = null;
                        break;

                    default:
                        _initializerTranslations[i] = null;
                        break;
                }

                if (i == l)
                {
                    break;
                }
            }
        }

        protected override void WriteInitializers(ITranslationContext context)
        {
            for (int i = 0, l = _initializerTranslations.Count - 1; ; ++i)
            {
                _initializerTranslations[i].WriteTo(context);

                if (i == l)
                {
                    break;
                }
            }
        }

        private void WriteMemberBinding(ITranslationContext context)
        {

        }

        private void WriteListBinding(ITranslationContext context)
        {

        }

        private void WriteAssignmentBinding(ITranslationContext context)
        {
            /*
                var value = context.TranslateAsCodeBlock(assignment.Expression);

                return assignment.Member.Name + " = " + value;
             *
             */
        }
    }
}