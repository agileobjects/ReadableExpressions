namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    public class VisualizerDialogSizeSettings
    {
        public bool UseFixedSize => !ResizeToMatchCode;

        public bool ResizeToMatchCode { get; set; } = true;

        public int? InitialWidth { get; set; }

        public int? InitialHeight { get; set; }
    }
}