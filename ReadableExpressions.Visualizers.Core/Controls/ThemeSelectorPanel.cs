﻿namespace AgileObjects.ReadableExpressions.Visualizers.Core.Controls
{
    using System.Windows.Forms;
    using static Theming.ExpressionTranslationTheme;

    internal class ThemeSelectorPanel : FlowLayoutPanel
    {
        public ThemeSelectorPanel(VisualizerDialog dialog)
        {
            FlowDirection = FlowDirection.LeftToRight;
            Width = DialogConstants.MenuWidth;

            dialog.RegisterThemeable(this);

            Controls.Add(new ThemeSelectorLabel(dialog));
            Controls.Add(new ThemeOption(Light, dialog));
            Controls.Add(new ThemeOption(Dark, dialog));
        }
    }
}