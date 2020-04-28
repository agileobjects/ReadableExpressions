namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class ListInitializerSetTranslation : InitializerSetTranslationBase<ElementInit>
    {
        private bool _hasMultiArgumentInitializers;

        public ListInitializerSetTranslation(IList<ElementInit> initializers, ITranslationContext context)
            : base(initializers, context)
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

        public override bool ForceWriteToMultipleLines => _hasMultiArgumentInitializers;

        private class MultiArgumentInitializerTranslation : ITranslatable
        {
            private readonly IList<CodeBlockTranslation> _translations;

            public MultiArgumentInitializerTranslation(ElementInit init, ITranslationContext context)
            {
                var translationSize = 0;
                var formattingSize = 0;

                _translations = init
                    .Arguments
                    .ProjectToArray(arg =>
                    {
                        var translation = context.GetCodeBlockTranslationFor(arg);
                        translationSize += translation.TranslationSize;
                        formattingSize += translation.FormattingSize;

                        return translation;
                    });

                TranslationSize = translationSize;
                FormattingSize = formattingSize;
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation("{ ");

                var argumentCount = _translations.Count;

                for (var i = 0; ;)
                {
                    _translations[i].WriteTo(buffer);

                    ++i;

                    if (i == argumentCount)
                    {
                        break;
                    }

                    buffer.WriteToTranslation(", ");
                }

                buffer.WriteToTranslation(" }");
            }
        }
    }
}