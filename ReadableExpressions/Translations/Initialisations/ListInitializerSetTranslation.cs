namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

using System.Collections.Generic;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions;

internal class ListInitializerSetTranslation :
    InitializerSetTranslationBase<ElementInit>
{
    private bool _hasMultiArgumentInitializers;

    public ListInitializerSetTranslation(
        IList<ElementInit> initializers,
        ITranslationContext context) :
        base(initializers, context)
    {
    }

    protected override ITranslation GetTranslationFor(
        ElementInit init,
        ITranslationContext context)
    {
        if (init.Arguments.Count == 1)
        {
            return context.GetCodeBlockTranslationFor(init.Arguments[0]);
        }

        _hasMultiArgumentInitializers = true;
        return new MultiArgumentInitializerTranslation(init, context);
    }

    protected override bool WriteToMultipleLines
        => _hasMultiArgumentInitializers || base.WriteToMultipleLines;

    private class MultiArgumentInitializerTranslation : ITranslation
    {
        private readonly IList<CodeBlockTranslation> _translations;
        private readonly int _argumentCount;

        public MultiArgumentInitializerTranslation(
            ElementInit init,
            ITranslationContext context)
        {
            var arguments = init.Arguments;
            _argumentCount = arguments.Count;

            _translations = new CodeBlockTranslation[_argumentCount];

            for (var i = 0; i < _argumentCount; ++i)
            {
                _translations[i] = context.GetCodeBlockTranslationFor(arguments[i]);
            }
        }

        public int TranslationLength =>
            _translations.TotalTranslationLength(separator: ", ") +
            "{ ".Length + " }".Length;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteToTranslation("{ ");

            for (var i = 0; ;)
            {
                _translations[i].WriteTo(writer);

                ++i;

                if (i == _argumentCount)
                {
                    break;
                }

                writer.WriteToTranslation(", ");
            }

            writer.WriteToTranslation(" }");
        }
    }
}