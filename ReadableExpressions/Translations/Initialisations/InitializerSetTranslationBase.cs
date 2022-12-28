namespace AgileObjects.ReadableExpressions.Translations.Initialisations;

using System.Collections.Generic;
using Extensions;
using static TranslationConstants;

internal abstract class InitializerSetTranslationBase<TInitializer> :
    IInitializerSetTranslation
{
    private readonly IList<ITranslation> _initializerTranslatables;

    protected InitializerSetTranslationBase(
        IList<TInitializer> initializers,
        ITranslationContext context)
    {
        var initializersCount = initializers.Count;
        Count = initializersCount;
        _initializerTranslatables = new ITranslation[initializersCount];

        for (var i = 0; ;)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            var initializerTranslation = GetTranslationFor(initializers[i], context);
            _initializerTranslatables[i] = initializerTranslation;


            ++i;

            if (i == initializersCount)
            {
                break;
            }
        }
    }

    protected abstract ITranslation GetTranslationFor(
        TInitializer initializer,
        ITranslationContext context);

    public int TranslationLength =>
        "{ ".Length +
        _initializerTranslatables.TotalTranslationLength(separator: ", ") +
        " }".Length;

    ITranslation IInitializerSetTranslation.Parent { set => Parent = value; }

    protected ITranslation Parent { get; set; }

    public int Count { get; }

    public void WriteTo(TranslationWriter writer)
    {
        var writeToMultipleLines = WriteToMultipleLines;

        if (writeToMultipleLines)
        {
            writer.WriteOpeningBraceToTranslation();
        }
        else
        {
            if (!writer.TranslationQuery(q => q.TranslationEndsWith(' ')))
            {
                writer.WriteSpaceToTranslation();
            }

            writer.WriteToTranslation("{ ");
        }

        for (var i = 0; ;)
        {
            _initializerTranslatables[i].WriteTo(writer);

            ++i;

            if (i == Count)
            {
                break;
            }

            if (writeToMultipleLines)
            {
                writer.WriteToTranslation(',');
                writer.WriteNewLineToTranslation();
                continue;
            }

            writer.WriteToTranslation(", ");
        }

        if (writeToMultipleLines)
        {
            writer.WriteClosingBraceToTranslation();
        }
        else
        {
            writer.WriteToTranslation(" }");
        }
    }

    protected virtual bool WriteToMultipleLines
        => Parent.TranslationLength > ExpressionWrapThreshold;
}