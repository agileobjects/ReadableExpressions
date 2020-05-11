namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using System.Drawing;
    using System.Drawing.Text;

    internal class VisualizerDialogFont
    {
        private static readonly Font _default = new Font(
            new FontFamily(GenericFontFamilies.Monospace),
            13f,
            FontStyle.Regular,
            GraphicsUnit.Point);

        public static readonly VisualizerDialogFont Monospace = new VisualizerDialogFont
        {
            Name = _default.Name,
            Size = (int)_default.SizeInPoints,
            _font = _default
        };

        private int? _size;
        private string _name;
        private Font _font;

        public static implicit operator Font(VisualizerDialogFont font) => font.Font;

        public string Name
        {
            get => _name ??= Monospace.Name;
            set
            {
                _font = null;
                _name = value;
            }
        }

        public int Size
        {
            get => _size ??= Monospace.Size;
            set
            {
                _font = null;
                _size = value;
            }
        }

        private Font Font => _font ??= new Font(Name, Size);
    }
}