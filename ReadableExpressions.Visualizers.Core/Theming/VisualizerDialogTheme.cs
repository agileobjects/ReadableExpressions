﻿namespace AgileObjects.ReadableExpressions.Visualizers.Core.Theming
{
    public class VisualizerDialogTheme
    {
        public static readonly VisualizerDialogTheme Dark = new VisualizerDialogTheme
        {
            Name = "Dark",
            Background = "#1E1E1E",
            Default = "#DCDCDC",
            Toolbar = "#2D2D30",
            Menu = "#1B1B1C",
            MenuHighlight = "#333334",
            Keyword = "#569CD6",
            Variable = "#9CDCFE",
            TypeName = "#4EC9B0",
            InterfaceName = "#B8D7A3",
            CommandStatement = "#D8A0DF",
            Text = "#D69D85",
            Numeric = "#B5CEA8",
            MethodName = "#DCDCAA",
            Comment = "#57A64A"
        };

        public static readonly VisualizerDialogTheme Light = new VisualizerDialogTheme
        {
            Name = "Light",
            Background = "#FFF",
            Default = "#000",
            Toolbar = "#EEEEF2",
            Menu = "#F6F6F6",
            MenuHighlight = "#C9DEF5",
            Keyword = "#0000FF",
            Variable = "#1F377F",
            TypeName = "#2B91AF",
            InterfaceName = "#2B91AF",
            CommandStatement = "#8F08C4",
            Text = "#A31515",
            Numeric = "#000",
            MethodName = "#74531F",
            Comment = "#008000"
        };

        public string Name { get; set; }

        public string Background { get; set; }

        public string Default { get; set; }

        public string Toolbar { get; set; }

        public string Menu { get; set; }

        public string MenuHighlight { get; set; }

        public string Keyword { get; set; }

        public string Variable { get; set; }

        public string TypeName { get; set; }

        public string InterfaceName { get; set; }

        public string CommandStatement { get; set; }

        public string Text { get; set; }

        public string Numeric { get; set; }

        public string MethodName { get; set; }

        public string Comment { get; set; }
    }
}