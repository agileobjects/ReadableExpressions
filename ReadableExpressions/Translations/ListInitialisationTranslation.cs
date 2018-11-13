﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class ListInitialisationTranslation : InitialisationTranslationBase<ElementInit>
    {
        private ListInitialisationTranslation(ListInitExpression listInit, ITranslationContext context)
            : base(
                ExpressionType.ListInit,
                listInit.NewExpression,
                new ListInitializerSetTranslation(listInit, context),
                context)
        {
        }

        public static ITranslation For(ListInitExpression listInit, ITranslationContext context)
        {
            if (InitHasNoInitializers(listInit.NewExpression, listInit.Initializers, context, out var newingTranslation))
            {
                return newingTranslation.WithNodeType(ExpressionType.ListInit);
            }

            return new ListInitialisationTranslation(listInit, context);
        }

        protected override bool WriteLongTranslationsToMultipleLines => InitializerTranslations.WriteToMultipleLines;

        private class ListInitializerSetTranslation : InitializerSetTranslation
        {
            private bool _hasMultiArgumentInitializers;

            public ListInitializerSetTranslation(ListInitExpression listInit, ITranslationContext context)
                : base(listInit.Initializers, context)
            {
            }

            protected override ITranslatable GetTranslation(ElementInit init, ITranslationContext context)
            {
                if (init.Arguments.Count == 1)
                {
                    return context.GetCodeBlockTranslationFor(init.Arguments[0]);
                }

                _hasMultiArgumentInitializers = true;

                return new MultiArgumentInitializerTranslation(init, context);
            }

            public override bool WriteToMultipleLines => _hasMultiArgumentInitializers;
        }

        private class MultiArgumentInitializerTranslation : ITranslatable
        {
            private readonly IList<CodeBlockTranslation> _translations;

            public MultiArgumentInitializerTranslation(ElementInit init, ITranslationContext context)
            {
                var estimatedSize = 0;

                _translations = init
                    .Arguments
                    .ProjectToArray(arg =>
                    {
                        var translation = context.GetCodeBlockTranslationFor(arg);
                        estimatedSize += translation.EstimatedSize;

                        return translation;
                    });

                EstimatedSize = estimatedSize;
            }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation("{ ");

                var argumentCount = _translations.Count;

                for (var i = 0; ;)
                {
                    _translations[i].WriteTo(context);

                    if (++i == argumentCount)
                    {
                        break;
                    }

                    context.WriteToTranslation(", ");
                }

                context.WriteToTranslation(" }");
            }
        }
    }
}