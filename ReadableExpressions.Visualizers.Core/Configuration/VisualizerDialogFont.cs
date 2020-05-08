namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using System.Drawing;
    using System.Drawing.Text;

    internal class VisualizerDialogFont
    {
        private static readonly Font _monospace = new Font(
            new FontFamily(GenericFontFamilies.Monospace),
            13.5f,
            FontStyle.Regular,
            GraphicsUnit.Point);

        public static readonly VisualizerDialogFont Monospace = new VisualizerDialogFont
        {
            Name = _monospace.Name,
            Size = _monospace.SizeInPoints,
            _font = _monospace
        };

        private Font _font;

        public static implicit operator Font(VisualizerDialogFont font) => font.Font;

        public string Name { get; set; }

        public float Size { get; set; }

        private Font Font => _font ??= new Font(Name, Size);
    }
}