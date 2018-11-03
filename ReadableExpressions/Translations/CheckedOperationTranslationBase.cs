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

        protected void WriteOpeningCheckedIfNecessary(ITranslationContext context, out bool isMultiStatementChecked)
        {
            if (IsCheckedOperation == false)
            {
                isMultiStatementChecked = false;
                return;
            }

            context.WriteToTranslation("checked");

            isMultiStatementChecked = IsMultiStatement();

            if (isMultiStatementChecked)
            {
                context.WriteOpeningBraceToTranslation();
                return;
            }

            context.WriteToTranslation(_openingSymbol);
        }

        protected abstract bool IsMultiStatement();

        protected void WriteClosingCheckedIfNecessary(ITranslationContext context, bool isMultiStatementChecked)
        {
            if (IsCheckedOperation == false)
            {
                return;
            }

            if (isMultiStatementChecked)
            {
                context.WriteClosingBraceToTranslation();
                return;
            }

            context.WriteToTranslation(_closingSymbol);
        }
    }
}