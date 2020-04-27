namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class ExpressionDialog : Form
    {
        public static readonly ExpressionDialog Instance = new ExpressionDialog();

        private readonly WebBrowser _viewer;
        private readonly Size _dialogMaximumSize;
        private readonly ToolStrip _toolbar;
        private readonly int _titleBarHeight;
        private bool _autoSize;

        private ExpressionDialog()
        {
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            _dialogMaximumSize = GetDialogMaximumSize();

            var screenRectangle = RectangleToScreen(ClientRectangle);
            _titleBarHeight = screenRectangle.Top - Top;

            _viewer = AddExpressionViewer();
            _toolbar = AddToolbar();

            SetViewerMaximumSize();
        }

        private Size GetDialogMaximumSize()
        {
            var screenSize = Screen.FromControl(this).Bounds.Size;

            return new Size(
                Convert.ToInt32(screenSize.Width * .9),
                Convert.ToInt32(screenSize.Height * .8));
        }

        private WebBrowser AddExpressionViewer()
        {
            var viewer = new WebBrowser
            {
                AllowNavigation = false,
                AllowWebBrowserDrop = false,
                Font = new Font(new FontFamily("Consolas"), 13f)
            };

            var viewerPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            viewerPanel.Controls.Add(viewer);
            Controls.Add(viewerPanel);

            viewer.Resize += (sender, arg) => viewerPanel.Size = ((Control)sender).Size;

            return viewer;
        }

        private ToolStrip AddToolbar()
        {
            var copyButton = new ToolStripButton
            {
                Alignment = ToolStripItemAlignment.Right,
                Text = "Copy",
            };

            copyButton.Click += (sender, args) => Clipboard.SetText(_viewer.Text);

            var toolbar = new ToolStrip(copyButton)
            {
                Dock = DockStyle.Bottom,
                GripStyle = ToolStripGripStyle.Hidden
            };

            Controls.Add(toolbar);

            return toolbar;
        }

        private void SetViewerMaximumSize()
        {
            _viewer.MaximumSize = GetViewerSizeBasedOn(_dialogMaximumSize);
        }

        private Size GetViewerSizeBasedOn(Size containerSize)
        {
            return new Size(
                containerSize.Width - SystemInformation.VerticalScrollBarWidth,
                containerSize.Height - _titleBarHeight - _toolbar.Height);
        }

        public override bool AutoSize => _autoSize;

        public override Size MaximumSize => _dialogMaximumSize;

        public override string Text => string.Empty;

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

        public ExpressionDialog WithText(string translation)
        {
            var rawText = TranslationHtmlFormatter.RemoveFormatting(translation);
            var textSize = TextRenderer.MeasureText(rawText, _viewer.Font);

            var viewerSize = new Size(
                textSize.Width + _viewer.Padding.Left + _viewer.Padding.Right + SystemInformation.VerticalScrollBarWidth + 10,
                textSize.Height + _viewer.Padding.Top + _viewer.Padding.Bottom + _viewer.Font.Height);

            SetViewerSize(viewerSize);

            _viewer.DocumentText = $@"
<html>
<head>
<style type=""text/css"">
body {{ 
    background: #1E1E1E;
    color: #DCDCDC; 
    font-family: Consolas, Monaco, 'Andale Mono', 'Ubuntu Mono', monospace;
    font-size: 11.5pt;
}}
.kw {{ color: #569CD6 }}
.pm {{ color: #9CDCFE }}
.tn {{ color: #4EC9B0 }}
.in {{ color: #B8D7A3 }}
.cs {{ color: #D8A0DF }}
.tx {{ color: #D69D85 }}
.nm {{ color: #B5CEA8 }}
.mn {{ color: #DCDCAA }}
.cm {{ color: #57A64A }}
</style>
</head>
<body>
    <pre>{translation}</pre>
</body>
</html>";

            return this;
        }

        private void SetViewerSize(Size newSize)
        {
            EnableAutoSize();

            var finalSize = new Size(
                Math.Max(newSize.Width, _viewer.Parent.Width),
                Math.Max(newSize.Height, _viewer.Parent.Height));

            _viewer.Size = finalSize;

            DisableAutoSize();
        }

        private void EnableAutoSize()
        {
            _autoSize =
            _viewer.Parent.AutoSize = true;
        }

        private void DisableAutoSize()
        {
            _autoSize =
            _viewer.Parent.AutoSize = false;
        }
    }
}
