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

    internal class ListInitialisationTranslation : InitialisationTranslationBase<ElementInit>
    {
        private readonly IList<ITranslation[]> _initializerTranslations;

        public ListInitialisationTranslation(ListInitExpression listInit, ITranslationContext context)
            : base(
                ExpressionType.ListInit,
                listInit.NewExpression,
                listInit.Initializers,
                context)
        {
            if (HasNoInitializers)
            {
                return;
            }

            var estimatedSize = NewingTranslation.EstimatedSize;

            _initializerTranslations = listInit.Initializers
                .Project(init =>
                {
                    if (init.Arguments.Count == 1)
                    {
                        ITranslation singleArgumentTranslation = context.GetCodeBlockTranslationFor(init.Arguments[0]);

                        estimatedSize += singleArgumentTranslation.EstimatedSize;

                        return new[] { singleArgumentTranslation };
                    }

                    return init
                        .Arguments
                        .Project(arg =>
                        {
                            var codeBlockTranslation = context.GetCodeBlockTranslationFor(arg);

                            estimatedSize += codeBlockTranslation.EstimatedSize;

                            return (ITranslation)codeBlockTranslation;
                        })
                        .ToArray();
                })
                .ToArray();

            EstimatedSize = estimatedSize;
        }

        protected override void WriteInitializers(ITranslationContext context)
        {
            for (int i = 0, l = _initializerTranslations.Count - 1; ; ++i)
            {
                var initializerTranslationSet = _initializerTranslations[i];
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

                if (i == l)
                {
                    break;
                }

                context.WriteNewLineToTranslation();
            }
        }
    }
}