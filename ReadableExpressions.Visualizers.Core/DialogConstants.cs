﻿namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System.Drawing;
    using static System.Drawing.FontStyle;

    internal static class DialogConstants
    {
        public const int MenuWidth = 300;
        public const int MenuItemHeight = 16;
        public const int ThemeOptionWidth = 40;
        public const int SettingCheckBoxWidth = 30;

        public static readonly Font MenuItemFont = new Font(
            new FontFamily("Segoe UI"),
            10,
            Regular,
            GraphicsUnit.Point,
            1,
            gdiVerticalFont: false);
    }
}