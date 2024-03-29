﻿namespace AgileObjects.ReadableExpressions.Visualizers.Dialog.Controls;

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Core.Theming;

internal class VisualizerViewer : WebBrowser
{
    private readonly Panel _parent;
    private readonly VisualizerDialog _dialog;
    private bool _initialised;

    public VisualizerViewer(Panel parent, VisualizerDialog dialog)
    {
        _parent = parent;
        _dialog = dialog;
        AllowNavigation = false;

        var font = _dialog.Settings.Font;
        base.Font = new Font(font.Name, font.Size, GraphicsUnit.Point);

        Resize += (sender, _) =>
        {
            var viewer = (VisualizerViewer)sender;
            viewer._parent.Size = viewer.Size;
        };

        dialog.RegisterThemeable(this);
    }

    public bool Uninitialised => !_initialised;

    public void HandleShown(string translation)
    {
        if (_initialised)
        {
            return;
        }

        Retry(TrySetBrowserOptions);

        if (Retry(TrySetContent, translation))
        {
            _initialised = true;
        }
    }

    private static void Retry(Func<bool> action)
        => Retry(act => act.Invoke(), action);

    private static bool Retry<TArg>(
        Func<TArg, bool> action,
        TArg state = default)
    {
        var i = 0;

        while (i < 5)
        {
            if (action.Invoke(state))
            {
                return true;
            }

            ++i;
            Thread.Sleep(250);
        }

        return false;
    }

    private bool TrySetBrowserOptions()
    {
        try
        {
            AllowWebBrowserDrop = false;
            ScrollBarsEnabled = false;
            return true;
        }
        catch (COMException)
        {
            return false;
        }
    }

    private bool TrySetContent(string translation)
    {
        var theme = _dialog.Theme;

        var content = $@"
<html>
<head>
<style type=""text/css"">
body, pre {{ 
    background: {theme.Background};
    color: {theme.Default}; 
    font-family: '{Font.Name}';
    font-size: {Font.Size}pt;
    overflow: auto;
}}
.kw {{ color: {theme.Keyword} }}
.vb {{ color: {theme.Variable} }}
.tn {{ color: {theme.TypeName} }}
.in {{ color: {theme.InterfaceName} }}
.cs {{ color: {theme.CommandStatement} }}
.tx {{ color: {theme.Text} }}
.nm {{ color: {theme.Numeric} }}
.mn {{ color: {theme.MethodName} }}
.cm {{ color: {theme.Comment} }}
</style>
</head>
<body scroll=""no"">
    {GetTranslationElement(translation)}
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

        try
        {
            DocumentText = content;
            return true;
        }
        catch (COMException)
        {
            return false;
        }
    }

    public string GetContentRaw() => TranslationElement.InnerText;

    public void SetContent(string translation)
        => TranslationElement.OuterHtml = GetTranslationElement(translation);

    private static string GetTranslationElement(string translation)
        => $"<pre id=\"translation\">{translation}</pre>";

    private HtmlElement TranslationElement => Document!.GetElementById("translation");

    public void SetTheme(VisualizerDialogTheme theme)
    {
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

        Document!.InvokeScript("setTheme", args);
    }

    public void SetFontFamily(Font newFont)
    {
        Font = newFont;
        Document!.InvokeScript("setFontFamily", new object[] { newFont.Name });
    }

    public void SetFontSize(int newFontSize)
    {
        Font = new Font(Font.Name, newFontSize, GraphicsUnit.Point);
        Document!.InvokeScript("setFontSize", new object[] { newFontSize });
    }

    public void SetSize(Size newSize)
    {
        var finalWidth = Math.Min(
            Math.Max(newSize.Width, MinimumSize.Width),
            MaximumSize.Width);

        var finalHeight = Math.Min(
            Math.Max(newSize.Height, MinimumSize.Height),
            MaximumSize.Height);

        Size = new Size(finalWidth, finalHeight);
    }
}