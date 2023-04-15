namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;

internal abstract class CheckedOperationTranslationBase
{
    private readonly string _openingSymbol;
    private readonly string _closingSymbol;

    protected CheckedOperationTranslationBase(
        bool isCheckedOperation,
        string openingSymbol,
        string closingSymbol)
    {
        _openingSymbol = openingSymbol;
        _closingSymbol = closingSymbol;
        IsCheckedOperation = isCheckedOperation;
    }

    protected bool IsCheckedOperation { get; }

    protected void WriteOpeningCheckedIfNecessary(
        TranslationWriter writer,
        out bool isMultiStatementChecked)
    {
        if (IsCheckedOperation == false)
        {
            isMultiStatementChecked = false;
            return;
        }

        writer.WriteKeywordToTranslation("checked");

        isMultiStatementChecked = IsMultiStatement();

        if (isMultiStatementChecked)
        {
            writer.WriteOpeningBraceToTranslation();
            return;
        }

        writer.WriteToTranslation(_openingSymbol);
    }

    protected abstract bool IsMultiStatement();

    protected void WriteClosingCheckedIfNecessary(
        TranslationWriter writer,
        bool isMultiStatementChecked)
    {
        if (IsCheckedOperation == false)
        {
            return;
        }

        if (isMultiStatementChecked)
        {
            writer.WriteClosingBraceToTranslation();
            return;
        }

        writer.WriteToTranslation(_closingSymbol);
    }
}