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

    internal class InitialisationTranslation : ITranslation
    {
        private readonly NewingTranslation _newingTranslation;
        private readonly IList<ITranslation[]> _initializerTranslations;

        public InitialisationTranslation(ListInitExpression listInit, ITranslationContext context)
        {
            _newingTranslation = new NewingTranslation(listInit.NewExpression, context);

            if (listInit.Initializers.Count == 0)
            {
                EstimatedSize = _newingTranslation.EstimatedSize;
                return;
            }

            _newingTranslation = _newingTranslation.WithoutParenthesesIfParameterless();

            var estimatedSize = _newingTranslation.EstimatedSize;

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
                            ITranslation argumentTranslation = context.GetCodeBlockTranslationFor(arg);

                            estimatedSize += argumentTranslation.EstimatedSize;

                            return argumentTranslation;
                        })
                        .ToArray();
                })
                .ToArray();

            NodeType = ExpressionType.ListInit;
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

            context.WriteClosingBraceToTranslation();
        }
    }
}