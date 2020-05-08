namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using System.Drawing;
    using System.Drawing.Text;

    internal class VisualizerDialogFont
    {
        private static readonly Font _monospace = new Font(
            new FontFamily(GenericFontFamilies.Monospace),
            13f,
            FontStyle.Regular,
            GraphicsUnit.Point);

        public static readonly VisualizerDialogFont Monospace = new VisualizerDialogFont
        {
            Name = _monospace.Name,
            Size = (int)_monospace.SizeInPoints,
            _font = _monospace
        };

        private int _size;
        private string _name;
        private Font _font;

        public static implicit operator Font(VisualizerDialogFont font) => font.Font;

        public string Name
        {
            get => _name;
            set
            {
                _font = null;
                _name = value;
            }
        }

        public int Size
        {
            get => _size;
            set
            {
                _font = null;
                _size = value;
            }
        }

        private Font Font => _font ??= new Font(Name, Size);
    }
}