namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using Configuration;
    using Controls;
    using Theming;
    using static System.Windows.Forms.SystemInformation;

    public class VisualizerDialog : Form
    {
        private static readonly Size _dialogMinimumSize = new Size(530, 125);

        private readonly Func<object> _translationFactory;
        private readonly ExpressionDialogRenderer _renderer;
        private readonly Size _dialogMaximumSize;
        private readonly ToolStrip _menuStrip;
        private readonly WebBrowser _viewer;
        private readonly ToolStrip _toolbar;
        private readonly List<Control> _themeableControls;
        private readonly int _titleBarHeight;
        private bool _autoSize;
        private string _translation;

        public VisualizerDialog(Func<object> translationFactory)
        {
            _translationFactory = translationFactory;
            _renderer = new ExpressionDialogRenderer(this);
            _themeableControls = new List<Control>();

            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            _dialogMaximumSize = GetDialogMaximumSize();

            var screenRectangle = RectangleToScreen(ClientRectangle);
            _titleBarHeight = screenRectangle.Top - Top;

            using (var graphics = CreateGraphics())
            {
                WidthFactor = graphics.DpiX / 72;
                HeightFactor = graphics.DpiY / 72;
            }

            ToolTip = AddToolTip();
            _viewer = AddViewer();
            _menuStrip = AddMenuStrip();
            _toolbar = AddToolbar();

            SetViewerSizeLimits();
            SetTranslation();
        }

        internal VisualizerDialogSettings Settings => VisualizerDialogSettings.Instance;

        internal VisualizerDialogTheme Theme
        {
            get => Settings.Theme;
            private set => Settings.Theme = value;
        }

        internal float WidthFactor { get; }

        internal float HeightFactor { get; }

        internal ToolTip ToolTip { get; }

        internal bool ViewerUninitialised { get; private set; }

        private Size GetDialogMaximumSize()
        {
            var screenSize = Screen.FromControl(this).Bounds.Size;

            return new Size(
                Convert.ToInt32(screenSize.Width * .9),
                Convert.ToInt32(screenSize.Height * .8));
        }

        private ToolTip AddToolTip()
        {
            return new ToolTip();
        }

        private ToolStrip AddMenuStrip()
        {
            var optionsMenuItem = new VisualizerOptionsMenuItem(this);

            var menuStrip = new ToolStrip(optionsMenuItem)
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                AutoSize = true
            };

            RegisterThemeable(menuStrip);
            Controls.Add(menuStrip);

            return menuStrip;
        }

        private WebBrowser AddViewer()
        {
            var viewer = new WebBrowser
            {
                AllowNavigation = false
            };

            var viewerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            viewerPanel.Controls.Add(viewer);
            Controls.Add(viewerPanel);

            viewer.Resize += (sender, arg) => viewerPanel.Size = ((Control)sender).Size;

            RegisterThemeable(viewer);
            RegisterThemeable(viewerPanel);

            ViewerUninitialised = true;
            return viewer;
        }

        private ToolStrip AddToolbar()
        {
            var copyButton = new Button { Text = "Copy" };

            copyButton.Click += (sender, args) => Clipboard.SetText(
            // ReSharper disable PossibleNullReferenceException
                TranslationHtmlFormatter.Instance.GetRaw(_viewer.Document.Body.InnerHtml));
            // ReSharper restore PossibleNullReferenceException

            RegisterThemeable(copyButton);

            var buttonWrapper = new ToolStripControlHost(copyButton)
            {
                Alignment = ToolStripItemAlignment.Right
            };

            var toolbar = new ToolStrip(buttonWrapper)
            {
                Dock = DockStyle.Bottom,
                GripStyle = ToolStripGripStyle.Hidden,
                AutoSize = true
            };

            RegisterThemeable(toolbar);
            Controls.Add(toolbar);

            return toolbar;
        }

        private void SetViewerSizeLimits()
        {
            _viewer.MinimumSize = _dialogMinimumSize;
            _viewer.MaximumSize = GetViewerSizeBasedOn(_dialogMaximumSize);
        }

        private Size GetViewerSizeBasedOn(Size containerSize)
        {
            return new Size(
                containerSize.Width - VerticalScrollBarWidth,
                containerSize.Height - _titleBarHeight - _menuStrip.Height - _toolbar.Height);
        }

        public override bool AutoSize => _autoSize;

        public override Size MaximumSize => _dialogMaximumSize;

        public override string Text => string.Empty;

        private void RegisterThemeable(ToolStrip toolStrip)
        {
            toolStrip.Renderer = _renderer;
            RegisterThemeable((Control)toolStrip);
        }

        internal void RegisterThemeable(Control control)
        {
            Theme.ApplyTo(control);
            _themeableControls.Add(control);
        }

        internal void UpdateTranslation()
        {
            SetTranslation();
            SetViewerContent();
        }

        private void SetTranslation()
        {
            _translation = (string)_translationFactory.Invoke();
            
            var rawText = TranslationHtmlFormatter.Instance.GetRaw(_translation);
            var font = (Font)Settings.Font;
            var textSize = TextRenderer.MeasureText(rawText, font);

            var viewerSize = new Size(
                textSize.Width + _viewer.Padding.Left + _viewer.Padding.Right + VerticalScrollBarWidth + 10,
                textSize.Height + _viewer.Padding.Top + _viewer.Padding.Bottom + font.Height);

            SetViewerSize(viewerSize);
        }

        internal void OnThemeChanged(VisualizerDialogTheme newTheme)
        {
            Theme = newTheme;

            foreach (var control in _themeableControls)
            {
                newTheme.ApplyTo(control);
            }

            Settings.Save();

            SetViewerContent();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((ModifierKeys == Keys.None) && (keyData == Keys.Escape))
            {
                Close();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            SetViewerSize(GetViewerSizeBasedOn(Size));
        }

        protected override void OnShown(EventArgs e)
        {
            if (ViewerUninitialised)
            {
                _viewer.AllowWebBrowserDrop = false;
                _viewer.ScrollBarsEnabled = false;
                ViewerUninitialised = false;
            }

            SetViewerContent();

            base.OnShown(e);
        }

        private void SetViewerContent()
        {
            var content = $@"
<html>
<head>
<style type=""text/css"">
pre {{ 
    background: {Theme.Background};
    color: {Theme.Default}; 
    font-family: '{Settings.Font.Name}';
    font-size: {Settings.Font.Size}pt;
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
<body>
    <pre>{_translation}</pre>
</body>
</html>";

            if (string.IsNullOrEmpty(_viewer.DocumentText))
            {
                _viewer.DocumentText = content;
                return;
            }

            _viewer.Navigate("about:blank");
            _viewer.Document.OpenNew(false).Write(content);
        }

        private void SetViewerSize(Size newSize)
        {
            EnableAutoSize();

            var finalWidth = Math.Min(
                Math.Max(newSize.Width, _viewer.MinimumSize.Width),
                _viewer.MaximumSize.Width);

            var finalHeight = Math.Min(
                Math.Max(newSize.Height, _viewer.MinimumSize.Height),
                _viewer.MaximumSize.Height);

            _viewer.Size = new Size(finalWidth, finalHeight);

            DisableAutoSize();
        }

        private void EnableAutoSize() => _autoSize = true;

        private void DisableAutoSize() => _autoSize = false;

        protected override void Dispose(bool disposing)
        {
            _themeableControls.Clear();

            ToolTip.Dispose();

            base.Dispose(disposing);
        }
    }
}
