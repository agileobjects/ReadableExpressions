namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls;

internal class StringConcatCallsOption : VisualizerDialogOptionBase
{
    public StringConcatCallsOption(VisualizerDialog dialog)
        : base(
            "Show string.Concat() calls",
            "Show string.Concat() method calls with all-string parameters as method calls instead of using the concatenation operator (+).",
            dialog.Settings.ShowStringConcatCalls,
            (dlg, isChecked) => dlg.Settings.ShowStringConcatCalls = isChecked,
            dialog)
    {
    }
}