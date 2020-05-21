namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Theming
{
    using System.Drawing;

    internal static class GraphicsExtensions
    {
        public static bool IsDark(this Color color)
        {
            // See https://stackoverflow.com/questions/25426819/finding-out-if-a-hex-color-is-dark-or-light
            return color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722 < 255 / 2.0;
        }

        public static Color ChangeBrightness(this Color color, float changeBy)
        {
            var red = (float)color.R;
            var green = (float)color.G;
            var blue = (float)color.B;

            if (changeBy < 0)
            {
                changeBy = 1 + changeBy;
                red *= changeBy;
                green *= changeBy;
                blue *= changeBy;
            }
            else
            {
                red = (255 - red) * changeBy + red;
                green = (255 - green) * changeBy + green;
                blue = (255 - blue) * changeBy + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
    }
}