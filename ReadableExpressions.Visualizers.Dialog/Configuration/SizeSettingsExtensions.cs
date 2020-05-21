namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Configuration
{
    using Core.Configuration;

    internal static class SizeSettingsExtensions
    {
        public static bool UpdateFrom(this VisualizerDialogSizeSettings settings, VisualizerDialog dialog)
        {
            var currentInitialWidth = settings.InitialWidth;
            var currentInitialHeight = settings.InitialHeight;

            if (settings.ResizeToMatchCode)
            {
                settings.InitialWidth = null;
                settings.InitialHeight = null;
            }
            else
            {
                settings.InitialWidth = dialog.Viewer.Width;
                settings.InitialHeight = dialog.Viewer.Height;
            }

            return settings.InitialWidth != currentInitialWidth ||
                   settings.InitialHeight != currentInitialHeight;
        }
    }
}
