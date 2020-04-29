namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    internal class ExpressionTranslationTheme
    {
        public static readonly ExpressionTranslationTheme Dark = new ExpressionTranslationTheme
        {
            Background = "#1E1E1E",
            Default = "#DCDCDC",
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

        public string Background { get; set; }

        public string Default { get; set; }

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