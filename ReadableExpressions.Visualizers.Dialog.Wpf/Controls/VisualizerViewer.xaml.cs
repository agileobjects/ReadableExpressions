namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Wpf.Controls
{
    using System.Windows;
    using Core.Theming;

    public partial class VisualizerViewer
    {
        private bool _initialised;

        public VisualizerViewer()
        {
            InitializeComponent();
        }

        public bool Uninitialised => !_initialised;

        public VisualizerDialogTheme Theme
        {
            get => (VisualizerDialogTheme)GetValue(ThemeProperty);
            set => SetValue(ThemeProperty, value);
        }

        public static readonly DependencyProperty ThemeProperty =
            DependencyProperty.RegisterAttached(
                nameof(Theme),
                typeof(VisualizerDialogTheme),
                typeof(VisualizerViewer),
                new PropertyMetadata((d, e) =>
                    ((VisualizerViewer)d).SetTheme((VisualizerDialogTheme)e.NewValue)));

        public string FontName
        {
            get => (string)GetValue(FontNameProperty);
            set => SetValue(FontNameProperty, value);
        }

        public static readonly DependencyProperty FontNameProperty =
            DependencyProperty.RegisterAttached(
                nameof(FontName),
                typeof(string),
                typeof(VisualizerViewer),
                new PropertyMetadata((d, e) =>
                    ((VisualizerViewer)d).SetFontName((string)e.NewValue)));

        public int FontSizeInPoints
        {
            get => (int)GetValue(FontSizeInPointsProperty);
            set => SetValue(FontSizeInPointsProperty, value);
        }

        public static readonly DependencyProperty FontSizeInPointsProperty =
            DependencyProperty.RegisterAttached(
                nameof(FontSizeInPoints),
                typeof(int),
                typeof(VisualizerViewer),
                new PropertyMetadata((d, e) =>
                    ((VisualizerViewer)d).SetFontSizeInPoints((int)e.NewValue)));

        public string Translation
        {
            get => (string)GetValue(TranslationProperty);
            set => SetValue(TranslationProperty, value);
        }

        public static readonly DependencyProperty TranslationProperty =
            DependencyProperty.RegisterAttached(
                nameof(Translation),
                typeof(string),
                typeof(VisualizerViewer),
                new PropertyMetadata((d, e) =>
                    ((VisualizerViewer)d).SetInitialContent((string)e.NewValue)));

        private void SetInitialContent(string translation)
        {
            if (_initialised)
            {
                return;
            }

            _initialised = true;

            var content = $@"
<html>
<head>
<style type=""text/css"">
body, pre {{ 
    background: {Theme.Background};
    color: {Theme.Default}; 
    font-family: '{FontName}';
    font-size: {FontSizeInPoints}pt;
    overflow: auto;
}}
.kw {{ color: {Theme.Keyword} }}
.vb {{ color: {Theme.Variable} }}
.tn {{ color: {Theme.TypeName} }}
.in {{ color: {Theme.InterfaceName} }}
.cs {{ color: {Theme.CommandStatement} }}
.tx {{ color: {Theme.Text} }}
.nm {{ color: {Theme.Numeric} }}
.mn {{ color: {Theme.MethodName} }}
.cm {{ color: {Theme.Comment} }}
</style>
</head>
<body scroll=""no"">
    <pre id=""translation"">{translation}</pre>
    <script type=""text/javascript"">
        function setFontFamily(ff) {{
            var cssRules = document.styleSheets[0].rules;

            for (var i = 0, l = cssRules.length; i < l; ++i) {{
                var cssRule = cssRules[i];
                var color;

                switch (cssRule.selectorText) {{
                    case 'BODY':
                    case 'PRE':
                        cssRule.style.fontFamily = ff;
                        continue;
                    default:
                        return;
                }}
            }}
        }}
        function setFontSize(fs) {{
            var cssRules = document.styleSheets[0].rules;

            for (var i = 0, l = cssRules.length; i < l; ++i) {{
                var cssRule = cssRules[i];
                var color;

                switch (cssRule.selectorText) {{
                    case 'BODY':
                    case 'PRE':
                        cssRule.style.fontSize = fs + 'pt';
                        continue;
                    default:
                        return;
                }}
            }}
        }}
        function setTheme(bg, df, kw, vb, tn, inf, cs, tx, nm, mn, cm) {{
            var cssRules = document.styleSheets[0].rules;

            for (var i = 0, l = cssRules.length; i < l; ++i) {{
                var cssRule = cssRules[i];
                var color;

                switch (cssRule.selectorText) {{
                    case 'BODY':
                    case 'PRE':
                        cssRule.style.background = bg;
                        color = df;
                        break;
                    case '.kw':
                        color = kw;
                        break;
                    case '.vb':
                        color = vb;
                        break;
                    case '.tn':
                        color = tn;
                        break;
                    case '.in':
                        color = inf;
                        break;
                    case '.cs':
                        color = cs;
                        break;
                    case '.tx':
                        color = tx;
                        break;
                    case '.nm':
                        color = nm;
                        break;
                    case '.mn':
                        color = mn;
                        break;
                    case '.cm':
                        color = cm;
                        break;
                }}

                if (color !== undefined) {{
                    cssRule.style.color = color;
                }}
            }}
        }}
    </script>
</body>
</html>".TrimStart();

            Browser.NavigateToString(content);
        }

        private void SetTheme(VisualizerDialogTheme theme)
        {
            if (Uninitialised)
            {
                return;
            }

            var args = new object[]
            {
                theme.Background,
                theme.Default,
                theme.Keyword,
                theme.Variable,
                theme.TypeName,
                theme.InterfaceName,
                theme.CommandStatement,
                theme.Text,
                theme.Numeric,
                theme.MethodName,
                theme.Comment
            };

            Browser.InvokeScript("setTheme", args);
        }

        private void SetFontName(string newFontName)
        {
            if (_initialised)
            {
                Browser.InvokeScript("setFontFamily", newFontName);
            }
        }

        private void SetFontSizeInPoints(int newSizeInPoints)
        {
            if (_initialised)
            {
                Browser.InvokeScript("setFontSize", newSizeInPoints);
            }
        }
    }
}
