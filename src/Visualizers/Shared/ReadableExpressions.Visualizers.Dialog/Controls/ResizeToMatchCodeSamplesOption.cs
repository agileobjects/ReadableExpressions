namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls
{
    using Configuration;

    internal class ResizeToMatchCodeSamplesOption : VisualizerDialogOptionBase
    {
        public ResizeToMatchCodeSamplesOption(VisualizerDialog dialog)
            : base(
                "Resize viewer to match code",
                "Resize the debugger visualizer window to match the code samples it contains.",
                dialog.Settings.Size.ResizeToMatchCode,
                UpdateSizeSettings,
                dialog)
        {
        }

        private static void UpdateSizeSettings(VisualizerDialog dialog, bool resizeToCode)
        {
            var sizeSettings = dialog.Settings.Size;

            sizeSettings.ResizeToMatchCode = resizeToCode;
            sizeSettings.UpdateFrom(dialog);
        }
    }
}