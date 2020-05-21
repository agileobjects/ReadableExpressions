namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    public class VisualizerDialogSizeSettings
    {
        public VisualizerDialogSizeSettings()
        {
            ResizeToMatchCode = true;
        }

        public bool UseFixedSize => !ResizeToMatchCode;

        public bool ResizeToMatchCode { get; set; }

        public int? InitialWidth { get; set; }

        public int? InitialHeight { get; set; }
    }
}