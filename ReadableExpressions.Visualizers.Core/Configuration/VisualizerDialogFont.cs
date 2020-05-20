namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using System.Drawing;
    using System.Drawing.Text;

    public class VisualizerDialogFont
    {
        public static readonly VisualizerDialogFont Monospace;

        static VisualizerDialogFont()
        {
            var monospace = new Font(
                new FontFamily(GenericFontFamilies.Monospace),
                13f,
                FontStyle.Regular,
                GraphicsUnit.Point);

            Monospace = new VisualizerDialogFont
            {
                Name = monospace.Name,
                Size = 13,
                _font = monospace
            };
        }

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