using System;

namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal abstract class InitialisationTranslationBase<TInitializer> : ITranslation
    {
        private readonly NewingTranslation _newingTranslation;
        private readonly IList<ITranslation[]> _initializerTranslations;

        protected InitialisationTranslationBase(
            ExpressionType initType,
            NewExpression newing,
            ICollection<TInitializer> initializers,
            Func<TInitializer, ITranslationContext, ITranslation[]> initializerTranslationFactory,
            ITranslationContext context)
        {
            NodeType = initType;
            _newingTranslation = new NewingTranslation(newing, context);

            if (initializers.Count == 0)
            {
                EstimatedSize = _newingTranslation.EstimatedSize;
                return;
            }

            _newingTranslation = _newingTranslation.WithoutParenthesesIfParameterless();

            var estimatedSize = _newingTranslation.EstimatedSize;

            _initializerTranslations = initializers
                .Project(init => initializerTranslationFactory.Invoke(init, context))
                .ToArray();

            for (int i = 0, l = _initializerTranslations.Count - 1; ; ++i)
            {
                var initializerTranslationSet = _initializerTranslations[i];

                foreach (var initializerTranslation in initializerTranslationSet)
                {
                    estimatedSize += initializerTranslation.EstimatedSize;
                }

                if (i == l)
                {
                    break;
                }
            }

            EstimatedSize = estimatedSize;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _newingTranslation.WriteTo(context);

            if (_initializerTranslations.Count == 0)
            {
                return;
            }

            context.WriteOpeningBraceToTranslation();

            for (int i = 0, l = _initializerTranslations.Count - 1; ; ++i)
            {
                WriteInitializerTranslationSet(_initializerTranslations[i], context);

                if (i == l)
                {
                    break;
                }

                context.WriteNewLineToTranslation();
            }

            context.WriteClosingBraceToTranslation();
        }

        protected abstract void WriteInitializerTranslationSet(
            ITranslation[] initializerTranslationSet,
            ITranslationContext context);
    }

    internal class ListInitialisationTranslation : InitialisationTranslationBase<ElementInit>
    {
        public ListInitialisationTranslation(ListInitExpression listInit, ITranslationContext context)
            : base(
                ExpressionType.ListInit,
                listInit.NewExpression,
                listInit.Initializers,
                GetElementInitializerTranslation,
                context)
        {
        }

        private static ITranslation[] GetElementInitializerTranslation(ElementInit init, ITranslationContext context)
        {
            if (init.Arguments.Count == 1)
            {
                ITranslation singleArgumentTranslation = context.GetCodeBlockTranslationFor(init.Arguments[0]);

                return new[] { singleArgumentTranslation };
            }

            return init
                .Arguments
                .Project(arg => (ITranslation)context.GetCodeBlockTranslationFor(arg))
                .ToArray();
        }

        protected override void WriteInitializerTranslationSet(
            ITranslation[] initializerTranslationSet,
            ITranslationContext context)
        {
            var numberOfArguments = initializerTranslationSet.Length;
            var hasMultipleArguments = numberOfArguments != 0;

            if (hasMultipleArguments)
            {
                context.WriteToTranslation("{ ");
            }

            for (int j = 0, m = numberOfArguments - 1; ; ++j)
            {
                initializerTranslationSet[j].WriteTo(context);

                if (j == m)
                {
                    break;
                }

                context.WriteToTranslation(", ");
            }

            if (hasMultipleArguments)
            {
                context.WriteToTranslation(" }");
            }
        }
    }

    internal class MemberInitialisationTranslation : InitialisationTranslationBase<MemberBinding>
    {
        public MemberInitialisationTranslation(MemberInitExpression memberInit, ITranslationContext context)
            : base(
                ExpressionType.MemberInit,
                memberInit.NewExpression,
                memberInit.Bindings,
                GetMemberBindingTranslation,
                context)
        {

        }

        private static ITranslation[] GetMemberBindingTranslation(MemberBinding binding, ITranslationContext context)
        {
            throw new NotImplementedException();
        }

        protected override void WriteInitializerTranslationSet(
            ITranslation[] initializerTranslationSet,
            ITranslationContext context)
        {
            throw new NotImplementedException();
        }
    }
}