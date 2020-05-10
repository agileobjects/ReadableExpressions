namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    using System.Drawing;

    internal static class GraphicsExtensions
    {
        public static bool IsDark(this Color color)
        {
            // See https://stackoverflow.com/questions/25426819/finding-out-if-a-hex-color-is-dark-or-light
            return color.R * 0.2126 + color.G * 0.7152 + color.B * 0.0722 < 255 / 2.0;
        }
    }
}