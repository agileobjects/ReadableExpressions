namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    public class VisualizerDialogFontSettings
    {
        public static readonly VisualizerDialogFontSettings Monospace = new VisualizerDialogFontSettings
        {
            Name = "Consolas",
            Size = 13
        };

        private int? _size;
        private string _name;

        public string Name
        {
            get => _name ??= Monospace.Name;
            set => _name = value;
        }

        public int Size
        {
            get => _size ??= Monospace.Size;
            set => _size = value;
        }
    }
}