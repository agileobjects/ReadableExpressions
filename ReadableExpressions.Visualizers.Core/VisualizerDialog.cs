﻿namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using Configuration;
    using Controls;
    using Theming;
    using Translations.Formatting;
    using static System.Windows.Forms.SystemInformation;
    using static DialogConstants;

    public class VisualizerDialog : Form
    {
        private static readonly Size _dialogMinimumSize = new Size(530, 125);

        private readonly Func<object> _translationFactory;
        private readonly VisualizerDialogRenderer _renderer;
        private readonly ToolStrip _menuStrip;
        private readonly ToolStrip _toolbar;
        private readonly List<Control> _themeableControls;
        private readonly int _titleBarHeight;
        private bool _autoSize;
        private string _translation;

        public VisualizerDialog(Func<object> translationFactory)
        {
            _translationFactory = translationFactory;
            _renderer = new VisualizerDialogRenderer(this);
            _themeableControls = new List<Control>();

            base.Text = "ReadableExpressions v" + VersionNumber.FileVersion;
            StartPosition = FormStartPosition.CenterScreen;
            MinimizeBox = false;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var screenRectangle = RectangleToScreen(ClientRectangle);
            _titleBarHeight = screenRectangle.Top - Top;

            using (var graphics = CreateGraphics())
            {
                WidthFactor = graphics.DpiX / 72;
                HeightFactor = graphics.DpiY / 72;
            }

            ToolTip = AddToolTip();
            Viewer = AddViewer();
            _menuStrip = AddMenuStrip();
            _toolbar = AddToolbar();

            SetViewerSizeLimits();
            SetTranslation();

            Resize += (sender, args) => ((VisualizerDialog)sender).HandleResize();
        }

        internal VisualizerDialogSettings Settings => VisualizerDialogSettings.Instance;

        internal VisualizerDialogTheme Theme
        {
            get => Settings.Theme;
            private set => Settings.Theme = value;
        }

        internal ITranslationFormatter Formatter => TranslationHtmlFormatter.Instance;

        internal float WidthFactor { get; }

        internal float HeightFactor { get; }

        internal WebBrowser Viewer { get; }

        internal ToolTip ToolTip { get; }

        internal bool ViewerUninitialised { get; private set; }

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
            var feedbackButton = new ToolStripControlHost(new FeedbackButton(this))
            {
                Alignment = ToolStripItemAlignment.Left
            };

            var copyButton = new ToolStripControlHost(new CopyButton(this))
            {
                Alignment = ToolStripItemAlignment.Right
            };

            var toolbar = new ToolStrip(feedbackButton, copyButton)
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
            Viewer.MinimumSize = _dialogMinimumSize;
            Viewer.MaximumSize = GetViewerSizeBasedOn(MaximumSize);
        }

        private Size GetViewerSizeBasedOn(Size containerSize)
        {
            return new Size(
                containerSize.Width - VerticalScrollBarWidth,
                containerSize.Height - _titleBarHeight - _menuStrip.Height - _toolbar.Height);
        }

        public override bool AutoSize => _autoSize;

        public override Size MaximumSize => Screen.PrimaryScreen.WorkingArea.Size;

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

            var rawText = Formatter.GetRaw(_translation);
            var font = (Font)Settings.Font;
            var textSize = TextRenderer.MeasureText(rawText, font);

            var width = textSize.Width + Viewer.Padding.Left + Viewer.Padding.Right + VerticalScrollBarWidth + 10;
            int height;

            var saveNewSize = false;

            if (Settings.Size.UseFixedSize &&
                Settings.Size.InitialWidth.HasValue &&
                Settings.Size.InitialHeight.HasValue)
            {
                if (Settings.Size.InitialWidth.Value > width)
                {
                    width = Settings.Size.InitialWidth.Value;
                }
                else
                {
                    saveNewSize = true;
                }

                height = Settings.Size.InitialHeight.Value;
            }
            else
            {
                height = textSize.Height + Viewer.Padding.Top + Viewer.Padding.Bottom + font.Height;
            }

            SetViewerSize(new Size(width, height));

            if (saveNewSize)
            {
                SaveNewSize();
            }
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

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // See https://stackoverflow.com/questions/1295999/event-when-a-window-gets-maximized-un-maximized
            if (m.Msg == SystemCommand)
            {
                var eventId = m.WParam.ToInt32() & 0xFFF0;

                switch (eventId)
                {
                    case WindowMaximise:
                    case WindowMinimise:
                    case WindowToggle:
                        HandleResize();
                        break;
                }
            }
        }

        private void HandleResize()
        {
            SetViewerSize(GetViewerSizeBasedOn(Size));
            SaveNewSize();
        }

        private void SaveNewSize()
        {
            Settings.Size.UpdateFrom(this);
            Settings.Save();
        }

        protected override void OnShown(EventArgs e)
        {
            if (ViewerUninitialised)
            {
                Viewer.AllowWebBrowserDrop = false;
                Viewer.ScrollBarsEnabled = false;
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
body, pre {{ 
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

            if (string.IsNullOrEmpty(Viewer.DocumentText))
            {
                Viewer.DocumentText = content;
                return;
            }

            Viewer.Navigate("about:blank");
            Viewer.Document.OpenNew(false).Write(content);
        }

        private void SetViewerSize(Size newSize)
        {
            EnableAutoSize();

            var finalWidth = Math.Min(
                Math.Max(newSize.Width, Viewer.MinimumSize.Width),
                Viewer.MaximumSize.Width);

            var finalHeight = Math.Min(
                Math.Max(newSize.Height, Viewer.MinimumSize.Height),
                Viewer.MaximumSize.Height);

            Viewer.Size = new Size(finalWidth, finalHeight);

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
