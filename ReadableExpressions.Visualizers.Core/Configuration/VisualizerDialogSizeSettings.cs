namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    internal class VisualizerDialogSizeSettings
    {
        public VisualizerDialogSizeSettings()
        {
            ResizeToMatchCode = true;
        }

        public bool UseFixedSize => !ResizeToMatchCode;

        public bool ResizeToMatchCode { get; set; }

        public int? InitialWidth { get; set; }

        public int? InitialHeight { get; set; }

        public bool UpdateFrom(VisualizerDialog dialog)
        {
            var currentInitialWidth = InitialWidth;
            var currentInitialHeight = InitialHeight;

            if (ResizeToMatchCode)
            {
                InitialWidth = null;
                InitialHeight = null;
            }
            else
            {
                InitialWidth = dialog.Viewer.Width;
                InitialHeight = dialog.Viewer.Height;
            }

            return InitialWidth != currentInitialWidth ||
                   InitialHeight != currentInitialHeight;
        }
    }
}