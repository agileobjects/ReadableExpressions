namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms;
    using Configuration;
    using Theming;
    using static System.Windows.Forms.SystemInformation;

    public class ExpressionDialog : Form
    {
        public static readonly ExpressionDialog Instance = new ExpressionDialog();

        private readonly ToolStripProfessionalRenderer _renderer;
        private readonly Size _dialogMaximumSize;
        private readonly ToolStrip _menuStrip;
        private readonly WebBrowser _viewer;
        private readonly ToolStrip _toolbar;
        private readonly int _titleBarHeight;
        private ExpressionTranslationTheme _theme;
        private bool _autoSize;
        private bool _viewerUninitialised;
        private string _translation;

        private ExpressionDialog()
        {
            var settings = ExpressionDialogSettings.LoadOrGetDefault();
            _theme = settings.Theme;
            _renderer = new ExpressionDialogRenderer(_theme);

            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;

            _dialogMaximumSize = GetDialogMaximumSize();

            var screenRectangle = RectangleToScreen(ClientRectangle);
            _titleBarHeight = screenRectangle.Top - Top;

            _viewer = AddViewer();
            _menuStrip = AddMenuStrip();
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

        private ToolStrip AddMenuStrip()
        {
            const int MENU_WIDTH = 400;
            const int THEME_SELECTOR_WIDTH = 220;
            const int MENU_ITEM_HEIGHT = 44;

            var themeSelectorLabel = new Label
            {
                BackColor = _theme.MenuColour,
                ForeColor = _theme.ForeColour,
                Size = new Size(MENU_WIDTH - THEME_SELECTOR_WIDTH, MENU_ITEM_HEIGHT),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Theme"
            };

            var lightThemeRadio = new RadioButton
            {
                BackColor = _theme.MenuColour,
                ForeColor = _theme.ForeColour,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Light",
                Width = THEME_SELECTOR_WIDTH / 2,
                Height = MENU_ITEM_HEIGHT,
                Checked = _theme == ExpressionTranslationTheme.Light
            };

            lightThemeRadio.CheckedChanged += (sender, args) =>
            {
                if (_viewerUninitialised)
                {
                    return;
                }

                if (sender is RadioButton themeButton && themeButton.Checked)
                {
                    _theme = themeButton.Text == "Light"
                        ? ExpressionTranslationTheme.Light
                        : ExpressionTranslationTheme.Dark;

                    SetViewerContent();
                }
            };

            var darkThemeRadio = new RadioButton
            {
                BackColor = _theme.MenuColour,
                ForeColor = _theme.ForeColour,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Dark",
                Width = THEME_SELECTOR_WIDTH / 2,
                Height = MENU_ITEM_HEIGHT,
                Checked = _theme == ExpressionTranslationTheme.Dark
            };

            darkThemeRadio.CheckedChanged += (sender, args) =>
            {
                if (_viewerUninitialised)
                {
                    return;
                }

                if (sender is RadioButton themeButton && themeButton.Checked)
                {
                    _theme = themeButton.Text == "Light"
                        ? ExpressionTranslationTheme.Light
                        : ExpressionTranslationTheme.Dark;

                    SetViewerContent();
                }
            };

            var themeMenuItemPanel = new FlowLayoutPanel
            {
                BackColor = _theme.MenuColour,
                FlowDirection = FlowDirection.LeftToRight,
                Width = MENU_WIDTH,
                Controls = { themeSelectorLabel, lightThemeRadio, darkThemeRadio }
            };

            var themeMenuItem = new ToolStripControlHost(themeMenuItemPanel);

            var optionsMenuItem = new ToolStripMenuItem("Options")
            {
                DropDown = new ToolStripDropDown
                {
                    BackColor = _theme.MenuColour,
                    ForeColor = _theme.ForeColour,
                    Width = MENU_WIDTH,
                    Items =
                    {
                        themeMenuItem
                    }
                }
            };

            var menuStrip = new ToolStrip(optionsMenuItem)
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = _theme.ToolbarColour,
                ForeColor = _theme.ForeColour,
                Renderer = _renderer
            };

            Controls.Add(menuStrip);

            return menuStrip;
        }

        private WebBrowser AddViewer()
        {
            var viewer = new WebBrowser
            {
                AllowNavigation = false,
                Font = new Font(new FontFamily(GenericFontFamilies.Monospace), 13.5f)
            };

            var viewerPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            viewerPanel.Controls.Add(viewer);
            Controls.Add(viewerPanel);

            viewer.Resize += (sender, arg) => viewerPanel.Size = ((Control)sender).Size;

            _viewerUninitialised = true;
            return viewer;
        }

        private ToolStrip AddToolbar()
        {
            var copyButton = new ToolStripButton
            {
                Alignment = ToolStripItemAlignment.Right,
                Text = "Copy",
                BackColor = _theme.MenuColour,
                ForeColor = _theme.ForeColour
            };

            copyButton.Click += (sender, args) => Clipboard.SetText(
            // ReSharper disable PossibleNullReferenceException
                TranslationHtmlFormatter.Instance.GetRaw(_viewer.Document.Body.InnerHtml));
            // ReSharper restore PossibleNullReferenceException

            var toolbar = new ToolStrip(copyButton)
            {
                Dock = DockStyle.Bottom,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = _theme.ToolbarColour,
                ForeColor = _theme.ForeColour,
                Renderer = _renderer
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
                containerSize.Width - VerticalScrollBarWidth,
                containerSize.Height - _titleBarHeight - _menuStrip.Height - _toolbar.Height);
        }

        public override bool AutoSize => _autoSize;

        public override Size MaximumSize => _dialogMaximumSize;

        public override string Text => string.Empty;

        public ExpressionDialog WithText(string translation)
        {
            _translation = translation;
            var rawText = TranslationHtmlFormatter.Instance.GetRaw(translation);
            var textSize = TextRenderer.MeasureText(rawText, _viewer.Font);

            var viewerSize = new Size(
                textSize.Width + _viewer.Padding.Left + _viewer.Padding.Right + VerticalScrollBarWidth + 10,
                textSize.Height + _viewer.Padding.Top + _viewer.Padding.Bottom + _viewer.Font.Height);

            SetViewerSize(viewerSize);
            return this;
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
            if (_viewerUninitialised)
            {
                _viewer.AllowWebBrowserDrop =
                _viewer.ScrollBarsEnabled = false;
                _viewerUninitialised = false;
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
body {{ 
    background: {_theme.Background};
    color: {_theme.Default}; 
    font-family: '{_viewer.Font.FontFamily.Name}';
    font-size: 13pt;
    overflow: auto;
}}
.kw {{ color: {_theme.Keyword} }}
.vb {{ color: {_theme.Variable} }}
.tn {{ color: {_theme.TypeName} }}
.in {{ color: {_theme.InterfaceName} }}
.cs {{ color: {_theme.CommandStatement} }}
.tx {{ color: {_theme.Text} }}
.nm {{ color: {_theme.Numeric} }}
.mn {{ color: {_theme.MethodName} }}
.cm {{ color: {_theme.Comment} }}
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
