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

        public void UpdateFrom(VisualizerDialog dialog)
        {
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
        }
    }
}