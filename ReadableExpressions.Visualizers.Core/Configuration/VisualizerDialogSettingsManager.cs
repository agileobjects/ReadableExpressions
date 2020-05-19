namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Theming;
    using static VisualizerDialogSettingsConstants;

    internal static class VisualizerDialogSettingsManager
    {
        private static readonly string _settingsFilePath;

        private static readonly string[] _newLines = { Environment.NewLine };
        private static readonly char[] _colons = { ':' };

        private static readonly BackgroundWorker _saveWorker;

        static VisualizerDialogSettingsManager()
        {
            var settingsFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft", "VisualStudio");

            if (!Directory.Exists(settingsFolderPath))
            {
                Directory.CreateDirectory(settingsFolderPath);
            }

            _settingsFilePath = Path.Combine(settingsFolderPath, "ReadableExpressions.yml");

            _saveWorker = new BackgroundWorker();
            _saveWorker.DoWork += Save;
        }

        public static bool TryLoad(out VisualizerDialogSettings settings)
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    settings = null;
                    return false;
                }

                var settingsByName = File
                    .ReadAllText(_settingsFilePath)
                    .Split(_newLines, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Split(_colons, count: 2))
                    .ToDictionary(
                        keyValue => keyValue.FirstOrDefault()?.Trim(),
                        keyValue => keyValue.LastOrDefault()?.Trim());

                settings = new VisualizerDialogSettings
                {
                    Theme = new VisualizerDialogTheme(),
                    Font = new VisualizerDialogFont(),
                    Size = new VisualizerDialogSizeSettings()
                };

                SetValues(settings, settingsByName);
                return true;
            }
            catch
            {
                settings = null;
                return false;
            }
        }

        private static void SetValues(
            VisualizerDialogSettings settings,
            IDictionary<string, string> settingsByName)
        {
            SetThemeValues(settings, settingsByName);
            SetFontValues(settings, settingsByName);
            SetSizeValues(settings, settingsByName);

            if (settingsByName.TryGetValue(nameof(settings.UseFullyQualifiedTypeNames), out var value))
            {
                settings.UseFullyQualifiedTypeNames = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(settings.UseExplicitTypeNames), out value))
            {
                settings.UseExplicitTypeNames = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(settings.UseExplicitGenericParameters), out value))
            {
                settings.UseExplicitGenericParameters = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(settings.DeclareOutputParametersInline), out value))
            {
                settings.DeclareOutputParametersInline = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(settings.ShowImplicitArrayTypes), out value))
            {
                settings.ShowImplicitArrayTypes = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(settings.ShowLambdaParameterTypeNames), out value))
            {
                settings.ShowLambdaParameterTypeNames = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(settings.ShowQuotedLambdaComments), out value))
            {
                settings.ShowQuotedLambdaComments = IsTrue(value);
            }
        }

        private static bool IsTrue(string value)
            => bool.TryParse(value, out var result) && result;

        private static void SetThemeValues(
            VisualizerDialogSettings settings,
            IDictionary<string, string> settingsByName)
        {
            var defaultTheme = settings.Theme;

            if (settingsByName.TryGetValue(ThemeName, out var value))
            {
                settings.Theme.Name = value;

                switch (value)
                {
                    case "Light":
                        defaultTheme = VisualizerDialogTheme.Light;
                        break;

                    case "Dark":
                        defaultTheme = VisualizerDialogTheme.Dark;
                        break;
                }
            }

            settings.Theme.Background = settingsByName.TryGetValue(ThemeBackground, out value)
                ? value : defaultTheme.Background;

            settings.Theme.Default = settingsByName.TryGetValue(ThemeDefault, out value)
                ? value : defaultTheme.Default;

            settings.Theme.Toolbar = settingsByName.TryGetValue(ThemeToolbar, out value)
                ? value : defaultTheme.Toolbar;

            settings.Theme.Menu = settingsByName.TryGetValue(ThemeMenu, out value)
                ? value : defaultTheme.Menu;

            settings.Theme.MenuHighlight = settingsByName.TryGetValue(ThemeMenuHighlight, out value) ? value : defaultTheme.MenuHighlight;

            settings.Theme.Keyword = settingsByName.TryGetValue(ThemeKeyword, out value)
                ? value : defaultTheme.Keyword;

            settings.Theme.Variable = settingsByName.TryGetValue(ThemeVariable, out value)
                ? value : defaultTheme.Variable;

            settings.Theme.TypeName = settingsByName.TryGetValue(ThemeTypeName, out value)
                ? value : defaultTheme.TypeName;

            settings.Theme.InterfaceName = settingsByName.TryGetValue(ThemeInterfaceName, out value)
                ? value : defaultTheme.InterfaceName;

            settings.Theme.CommandStatement = settingsByName.TryGetValue(ThemeCommandStatement, out value)
                ? value : defaultTheme.CommandStatement;

            settings.Theme.Text = settingsByName.TryGetValue(ThemeText, out value)
                ? value : defaultTheme.Text;

            settings.Theme.Numeric = settingsByName.TryGetValue(ThemeNumeric, out value)
                ? value : defaultTheme.Numeric;

            settings.Theme.MethodName = settingsByName.TryGetValue(ThemeMethodName, out value)
                ? value : defaultTheme.MethodName;

            settings.Theme.Comment = settingsByName.TryGetValue(ThemeComment, out value)
                ? value : defaultTheme.Comment;
        }

        private static void SetFontValues(
            VisualizerDialogSettings settings,
            IDictionary<string, string> settingsByName)
        {
            if (settingsByName.TryGetValue(FontName, out var value))
            {
                settings.Font.Name = value;
            }

            if (settingsByName.TryGetValue(FontSize, out value) &&
                int.TryParse(value, out var fontSize))
            {
                settings.Font.Size = fontSize;
            }
        }

        private static void SetSizeValues(
            VisualizerDialogSettings settings,
            IDictionary<string, string> settingsByName)
        {
            if (settingsByName.TryGetValue(SizeResizeToCode, out var value))
            {
                settings.Size.ResizeToMatchCode = IsTrue(value);
            }

            if (settingsByName.TryGetValue(SizeInitialWidth, out value) &&
                int.TryParse(value, out var width))
            {
                settings.Size.InitialWidth = width;
            }

            if (settingsByName.TryGetValue(SizeInitialHeight, out value) &&
                int.TryParse(value, out var height))
            {
                settings.Size.InitialHeight = height;
            }
        }

        public static void Save(VisualizerDialogSettings settings)
            => _saveWorker.RunWorkerAsync(settings);

        private static void Save(object sender, DoWorkEventArgs args)
        {
            var settings = (VisualizerDialogSettings)args.Argument;

            var serialized = Serialize(settings);

            File.WriteAllText(_settingsFilePath, serialized);
        }

        private static string Serialize(VisualizerDialogSettings settings)
        {
            var theme = settings.Theme;

            return $@"
{ThemeName}: {theme.Name}
{ThemeBackground}: {theme.Background}
{ThemeDefault}: {theme.Default}
{ThemeToolbar}: {theme.Toolbar}
{ThemeMenu}: {theme.Menu}
{ThemeMenuHighlight}: {theme.MenuHighlight}
{ThemeKeyword}: {theme.Keyword}
{ThemeVariable}: {theme.Variable}
{ThemeTypeName}: {theme.TypeName}
{ThemeInterfaceName}: {theme.InterfaceName}
{ThemeCommandStatement}: {theme.CommandStatement}
{ThemeText}: {theme.Text}
{ThemeNumeric}: {theme.Numeric}
{ThemeMethodName}: {theme.MethodName}
{ThemeComment}: {theme.Comment}
{FontName}: {settings.Font.Name}
{FontSize}: {settings.Font.Size}
{SizeResizeToCode}: {settings.Size.ResizeToMatchCode}
{SizeInitialWidth}: {settings.Size.InitialWidth}
{SizeInitialHeight}: {settings.Size.InitialHeight}
{nameof(settings.UseFullyQualifiedTypeNames)}: {settings.UseFullyQualifiedTypeNames}
{nameof(settings.UseExplicitTypeNames)}: {settings.UseExplicitTypeNames}
{nameof(settings.UseExplicitGenericParameters)}: {settings.UseExplicitGenericParameters}
{nameof(settings.DeclareOutputParametersInline)}: {settings.DeclareOutputParametersInline}
{nameof(settings.ShowImplicitArrayTypes)}: {settings.ShowImplicitArrayTypes}
{nameof(settings.ShowLambdaParameterTypeNames)}: {settings.ShowLambdaParameterTypeNames}
{nameof(settings.ShowQuotedLambdaComments)}: {settings.ShowQuotedLambdaComments}".TrimStart();
        }
    }
}