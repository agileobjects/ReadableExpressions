﻿namespace AgileObjects.ReadableExpressions.Translations.Initialisations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
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
            private readonly int _argumentCount;

            public MultiArgumentInitializerTranslation(ElementInit init, ITranslationContext context)
            {
                var translationSize = 0;
                var formattingSize = 0;

                var arguments = init.Arguments;
                _argumentCount = arguments.Count;

                _translations = new CodeBlockTranslation[_argumentCount];

                for (var i = 0; i < _argumentCount; ++i)
                {
                    var translation = context.GetCodeBlockTranslationFor(arguments[i]);
                    translationSize += translation.TranslationSize;
                    formattingSize += translation.FormattingSize;

                    _translations[i] = translation;
                }

                TranslationSize = translationSize;
                FormattingSize = formattingSize;
            }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetLineCount()
                => _translations.GetLineCount(_argumentCount);

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation("{ ");

                for (var i = 0; ;)
                {
                    _translations[i].WriteTo(buffer);

                    ++i;

                    if (i == _argumentCount)
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