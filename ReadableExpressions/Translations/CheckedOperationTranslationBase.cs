namespace AgileObjects.ReadableExpressions.Translations
{
    internal abstract class CheckedOperationTranslationBase
    {
        private readonly string _openingSymbol;
        private readonly string _closingSymbol;

        protected CheckedOperationTranslationBase(bool isCheckedOperation, string openingSymbol, string closingSymbol)
        {
            _openingSymbol = openingSymbol;
            _closingSymbol = closingSymbol;
            IsCheckedOperation = isCheckedOperation;
        }

        protected bool IsCheckedOperation { get; }

        protected void WriteOpeningCheckedIfNecessary(TranslationBuffer buffer, out bool isMultiStatementChecked)
        {
            if (IsCheckedOperation == false)
            {
                isMultiStatementChecked = false;
                return;
            }

            buffer.WriteKeywordToTranslation("checked");

            isMultiStatementChecked = IsMultiStatement();

            if (isMultiStatementChecked)
            {
                buffer.WriteOpeningBraceToTranslation();
                return;
            }

            buffer.WriteToTranslation(_openingSymbol);
        }

        protected abstract bool IsMultiStatement();

        protected void WriteClosingCheckedIfNecessary(TranslationBuffer buffer, bool isMultiStatementChecked)
        {
            if (IsCheckedOperation == false)
            {
                return;
            }

            if (isMultiStatementChecked)
            {
                buffer.WriteClosingBraceToTranslation();
                return;
            }

            buffer.WriteToTranslation(_closingSymbol);
        }
    }
}