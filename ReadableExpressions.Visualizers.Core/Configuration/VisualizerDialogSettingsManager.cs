namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Theming;
    using static VisualizerDialogSettingsConstants;

    internal static class VisualizerDialogSettingsManager
    {
        private static readonly string _settingsFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "Local", "Microsoft", "VisualStudio");

        private static readonly string _settingsFilePath = Path.Combine(
            _settingsFolderPath,
            "ReadableExpressions.yml");

        private static readonly string[] _newLines = { Environment.NewLine };
        private static readonly char[] _colons = { ':' };

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
            if (settingsByName.TryGetValue(ThemeName, out var value))
            {
                settings.Theme.Name = value;
            }

            if (settingsByName.TryGetValue(ThemeBackground, out value))
            {
                settings.Theme.Background = value;
            }

            if (settingsByName.TryGetValue(ThemeDefault, out value))
            {
                settings.Theme.Default = value;
            }

            if (settingsByName.TryGetValue(ThemeToolbar, out value))
            {
                settings.Theme.Toolbar = value;
            }

            if (settingsByName.TryGetValue(ThemeMenu, out value))
            {
                settings.Theme.Menu = value;
            }

            if (settingsByName.TryGetValue(ThemeKeyword, out value))
            {
                settings.Theme.Keyword = value;
            }

            if (settingsByName.TryGetValue(ThemeVariable, out value))
            {
                settings.Theme.Variable = value;
            }

            if (settingsByName.TryGetValue(ThemeTypeName, out value))
            {
                settings.Theme.TypeName = value;
            }

            if (settingsByName.TryGetValue(ThemeInterfaceName, out value))
            {
                settings.Theme.InterfaceName = value;
            }

            if (settingsByName.TryGetValue(ThemeCommandStatement, out value))
            {
                settings.Theme.CommandStatement = value;
            }

            if (settingsByName.TryGetValue(ThemeText, out value))
            {
                settings.Theme.Text = value;
            }

            if (settingsByName.TryGetValue(ThemeNumeric, out value))
            {
                settings.Theme.Numeric = value;
            }

            if (settingsByName.TryGetValue(ThemeMethodName, out value))
            {
                settings.Theme.MethodName = value;
            }

            if (settingsByName.TryGetValue(ThemeComment, out value))
            {
                settings.Theme.Comment = value;
            }
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
        {
            var serialized = Serialize(settings);

            if (!Directory.Exists(_settingsFolderPath))
            {
                Directory.CreateDirectory(_settingsFolderPath);
            }

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