namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System.Drawing;
    using static System.Drawing.FontStyle;
    using static System.Drawing.GraphicsUnit;

    internal static class DialogConstants
    {
        public const int MenuWidth = 500;
        public const int MenuItemHeight = 26;
        public const int ThemeOptionWidth = 75;
        public const int SettingCheckBoxWidth = 50;

        public static readonly Font MenuItemFont =
            new Font(new FontFamily("Arial"), 16, Regular, Pixel);
    }
}