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
                        var singleArgumentTranslation = context.GetTranslationFor(init.Arguments[0]);
                        estimatedSize += singleArgumentTranslation.EstimatedSize;

                        return new[] { singleArgumentTranslation };
                    }

                    return init
                        .Arguments
                        .Project(arg =>
                        {
                            var argumentTranslation = context.GetTranslationFor(arg);

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

            context.WriteNewLineToTranslation();
            context.WriteToTranslation('{');
            context.WriteNewLineToTranslation();
            context.Indent();

            foreach (var initializerTranslationSet in _initializerTranslations)
            {
                var numberOfArguments = initializerTranslationSet.Length;
                var hasMultipleArguments = numberOfArguments != 0;

                if (hasMultipleArguments)
                {
                    context.WriteToTranslation("{ ");
                }

                for (var i = 0; ; ++i)
                {
                    initializerTranslationSet[i].WriteTo(context);

                    if (i == (numberOfArguments - 1))
                    {
                        break;
                    }

                    context.WriteToTranslation(", ");
                }

                if (hasMultipleArguments)
                {
                    context.WriteToTranslation(" }");
                }

                context.WriteNewLineToTranslation();
            }

            context.Unindent();
            context.WriteToTranslation('}');
        }
    }
}