namespace AgileObjects.ReadableExpressions.Visualizers.Dialog;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Configuration;
using Controls;
using Core;
using Core.Configuration;
using Core.Theming;
using Theming;
using static DialogConstants;
using static System.Windows.Forms.ScreenOrientation;
using static System.Windows.Forms.SystemInformation;

public class VisualizerDialog : Form
{
    private static readonly Size _dialogMinimumSize = new(530, 125);

    private readonly Func<object> _translationPathFactory;
    private readonly VisualizerDialogRenderer _renderer;
    private readonly ToolStrip _menuStrip;
    private readonly ToolStrip _toolbar;
    private readonly List<Control> _themeableControls;
    private readonly int _titleBarHeight;
    private string _translationPath;
    private bool _autoSize;

    public VisualizerDialog(Func<object> translationPathFactory)
    {
        _translationPathFactory = translationPathFactory;
        ColourTable = new(this);
        _renderer = new(this);
        _themeableControls = new();

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

        ViewModel = new();

        ToolTip = new();
        Viewer = AddViewer();
        _menuStrip = AddMenuStrip();
        _toolbar = AddToolbar();

        SetViewerSizeLimits();
        SetTranslation();

        Application.Idle += LazyLoadMenus;

        Shown += HandleShown;
        Resize += HandleResize;
    }

    internal TranslationViewModel ViewModel { get; }

    internal VisualizerDialogColourTable ColourTable { get; }

    internal VisualizerDialogSettings Settings => ViewModel.Settings;

    internal VisualizerDialogTheme Theme
    {
        get => Settings.Theme;
        private set => Settings.Theme = value;
    }

    internal float WidthFactor { get; }

    internal float HeightFactor { get; }

    internal VisualizerViewer Viewer { get; }

    internal ToolTip ToolTip { get; }

    private static void HandleShown(object sender, EventArgs _)
    {
        var dialog = (VisualizerDialog)sender;
        dialog.Viewer.HandleShown(dialog.ViewModel.Translation);
    }

    private static void HandleResize(object sender, EventArgs _)
        => ((VisualizerDialog)sender).HandleResize();

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

    private VisualizerViewer AddViewer()
    {
        var viewerPanel = new VisualizerViewerPanel(this);

        Controls.Add(viewerPanel);

        return viewerPanel.Viewer;
    }

    private ToolStrip AddToolbar()
    {
        var feedbackButton = new ToolStripControlHost(new FeedbackButton(this))
        {
            Alignment = ToolStripItemAlignment.Left

        };

        var closeButton = new ToolStripControlHost(new CloseButton(this))
        {
            Alignment = ToolStripItemAlignment.Right,
            Padding = new(8, 0, 0, 0)
        };

        var copyButton = new ToolStripControlHost(new CopyButton(this))
        {
            Alignment = ToolStripItemAlignment.Right
        };

        var toolbar = new ToolStrip(feedbackButton, closeButton, copyButton)
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
        return new(
            containerSize.Width - VerticalScrollBarWidth,
            containerSize.Height - _titleBarHeight - _menuStrip.Height - _toolbar.Height);
    }

    public override bool AutoSize => _autoSize;

    public override Size MaximumSize
    {
        get
        {
            var screenSize = Screen.GetWorkingArea(this).Size;

            switch (SystemInformation.ScreenOrientation)
            {
                case Angle90:
                case Angle270:
                    return new(screenSize.Height, screenSize.Width);

                default:
                    return screenSize;
            }
        }
    }

    public override string Text => string.Empty;

    private void RegisterThemeable(ToolStrip toolStrip)
    {
        toolStrip.Renderer = _renderer;
        RegisterThemeable((Control)toolStrip);
    }

    internal void RegisterThemeable(Control control)
    {
        ColourTable.ApplyTo(control);
        _themeableControls.Add(control);
    }

    internal void UpdateTranslation()
    {
        var currentTranslation = ViewModel.Translation;

        SetTranslation();

        if (ViewModel.Translation != currentTranslation)
        {
            Viewer.SetContent(ViewModel.Translation);
        }
    }

    private void SetTranslation()
    {
        _translationPath = (string)_translationPathFactory.Invoke();

        var translation = File.Exists(_translationPath)
            ? File.ReadAllText(_translationPath)
            : _translationPath;

        ViewModel.Translation = translation;

        var rawText = ViewModel.TranslationRaw;
        var font = Viewer.Font;
        var textSize = TextRenderer.MeasureText(rawText, font);

        var width = textSize.Width + Viewer.Padding.Left + Viewer.Padding.Right + VerticalScrollBarWidth + 10;
        int height;

        var sizeSettings = Settings.Size;
        var saveNewSize = false;

        if (sizeSettings.UseFixedSize &&
            sizeSettings.InitialWidth.HasValue &&
            sizeSettings.InitialHeight.HasValue)
        {
            if (sizeSettings.InitialWidth.Value > width)
            {
                width = sizeSettings.InitialWidth.Value;
            }
            else
            {
                saveNewSize = true;
            }

            height = sizeSettings.InitialHeight.Value;
        }
        else
        {
            height = textSize.Height + Viewer.Padding.Top + Viewer.Padding.Bottom + font.Height;
        }

        SetViewerSize(new(width, height));

        if (saveNewSize)
        {
            SaveNewSize();
        }
    }

    internal void HandleThemeChanged(VisualizerDialogTheme newTheme)
    {
        Theme = newTheme;

        ColourTable.HandleThemeChanged();

        foreach (var control in _themeableControls)
        {
            ColourTable.ApplyTo(control);
        }

        Viewer.SetTheme(newTheme);

        Settings.Save();
    }

    protected override bool ProcessDialogKey(Keys keyData)
    {
        if (ModifierKeys == Keys.None && keyData == Keys.Escape)
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
        if (Settings.Size.UpdateFrom(this))
        {
            Settings.Save();
        }
    }

    private void LazyLoadMenus(object sender, EventArgs args)
    {
        var lazyLoadMenus = _menuStrip.Items.OfType<ILazyMenuItem>();

        foreach (var menu in lazyLoadMenus)
        {
            menu.Initialize();
        }


    }

    private void SetViewerSize(Size newSize)
    {
        _autoSize = true;

        Viewer.SetSize(newSize);

        _autoSize = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (_themeableControls?.Any() == true)
        {
            _themeableControls.Clear();
            ToolTip.Dispose();
        }

        if (_translationPath != null)
        {
            try
            {
                if (File.Exists(_translationPath))
                {
                    File.Delete(_translationPath);
                }
            }
            catch
            {
                // Can't do anything about it...
            }
        }

        RemoveEventHandlers();
        base.Dispose(disposing);
    }

    private void RemoveEventHandlers()
    {
        Shown -= HandleShown;
        Resize -= HandleResize;
        Application.Idle -= LazyLoadMenus;
    }
}