namespace AgileObjects.ReadableExpressions.Visualizers.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Theming;
    using static System.StringSplitOptions;

    internal class ExpressionDialogSettings
    {
        private static readonly ExpressionDialogSettings _default = new ExpressionDialogSettings
        {
            Theme = ExpressionTranslationTheme.Dark
        };

        private static readonly string[] _newLines = { Environment.NewLine };
        private static readonly char[] _colons = { ':' };

        public static readonly ExpressionDialogSettings Instance = LoadOrGetDefault();

        private static ExpressionDialogSettings LoadOrGetDefault()
        {
            try
            {
                var settingsFilePath = Path.Combine(
                    Application.LocalUserAppDataPath,
                    "ReadableExpressions.yml");

                if (!File.Exists(settingsFilePath))
                {
                    return _default;
                }

                var settingsByName = File
                    .ReadAllText(settingsFilePath)
                    .Split(_newLines, RemoveEmptyEntries)
                    .Select(line => line.Split(_colons, count: 2))
                    .ToDictionary(
                        keyValue => keyValue.FirstOrDefault(),
                        keyValue => keyValue.LastOrDefault());

                var settings = new ExpressionDialogSettings
                {
                    Theme = new ExpressionTranslationTheme()
                };

                SetValues(settings, settingsByName);

                return settings;
            }
            catch
            {
                return _default;
            }
        }

        #region Theme Keys

        private const string _themeBackground = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.Background);
        private const string _themeDefault = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.Default);
        private const string _themeKeyword = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.Keyword);
        private const string _themeVariable = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.Variable);
        private const string _themeTypeName = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.TypeName);
        private const string _themeInterfaceName = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.InterfaceName);
        private const string _themeCommandStatement = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.CommandStatement);
        private const string _themeText = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.Text);
        private const string _themeNumeric = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.Numeric);
        private const string _themeMethodName = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.MethodName);
        private const string _themeComment = nameof(Theme) + "." + nameof(ExpressionTranslationTheme.Comment);

        #endregion

        private static void SetValues(
            ExpressionDialogSettings settings,
            IDictionary<string, string> settingsByName)
        {
            SetThemeValues(settings, settingsByName);

            if (settingsByName.TryGetValue(nameof(UseFullyQualifiedTypeNames), out var value))
            {
                settings.UseFullyQualifiedTypeNames = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(UseExplicitGenericParameters), out value))
            {
                settings.UseExplicitGenericParameters = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(DeclareOutputParametersInline), out value))
            {
                settings.DeclareOutputParametersInline = IsTrue(value);
            }

            if (settingsByName.TryGetValue(nameof(ShowQuotedLambdaComments), out value))
            {
                settings.ShowQuotedLambdaComments = IsTrue(value);
            }
        }

        private static bool IsTrue(string value)
            => bool.TryParse(value, out var result) && result;

        private static void SetThemeValues(
            ExpressionDialogSettings settings,
            IDictionary<string, string> settingsByName)
        {
            if (settingsByName.TryGetValue(nameof(_themeBackground), out var value))
            {
                settings.Theme.Background = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeDefault), out value))
            {
                settings.Theme.Default = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeKeyword), out value))
            {
                settings.Theme.Keyword = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeVariable), out value))
            {
                settings.Theme.Variable = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeTypeName), out value))
            {
                settings.Theme.TypeName = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeInterfaceName), out value))
            {
                settings.Theme.InterfaceName = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeCommandStatement), out value))
            {
                settings.Theme.CommandStatement = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeText), out value))
            {
                settings.Theme.Text = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeNumeric), out value))
            {
                settings.Theme.Numeric = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeMethodName), out value))
            {
                settings.Theme.MethodName = value;
            }

            if (settingsByName.TryGetValue(nameof(_themeComment), out value))
            {
                settings.Theme.Comment = value;
            }
        }

        public ExpressionTranslationTheme Theme { get; set; }

        public bool UseFullyQualifiedTypeNames { get; set; }

        public bool UseExplicitGenericParameters { get; set; }

        public bool DeclareOutputParametersInline { get; set; }

        public bool ShowQuotedLambdaComments { get; set; }

        public TranslationSettings Update(TranslationSettings settings)
        {
            if (UseFullyQualifiedTypeNames)
            {
                settings = settings.UseFullyQualifiedTypeNames;
            }

            if (UseExplicitGenericParameters)
            {
                settings = settings.UseExplicitGenericParameters;
            }

            if (DeclareOutputParametersInline)
            {
                settings = settings.DeclareOutputParametersInline;
            }

            if (ShowQuotedLambdaComments)
            {
                settings = settings.ShowQuotedLambdaComments;
            }

            return settings;
        }

        public string SerializeToString()
        {
            return $@"
{_themeBackground}: {Theme.Background}
{_themeDefault}: {Theme.Default}
{_themeKeyword}: {Theme.Keyword}
{_themeVariable}: {Theme.Variable}
{_themeTypeName}: {Theme.TypeName}
{_themeInterfaceName}: {Theme.InterfaceName}
{_themeCommandStatement}: {Theme.CommandStatement}
{_themeText}: {Theme.Text}
{_themeNumeric}: {Theme.Numeric}
{_themeMethodName}: {Theme.MethodName}
{_themeComment}: {Theme.Comment}
{nameof(UseFullyQualifiedTypeNames)}: {UseFullyQualifiedTypeNames}
{nameof(UseExplicitGenericParameters)}: {UseExplicitGenericParameters}
{nameof(DeclareOutputParametersInline)}: {DeclareOutputParametersInline}
{nameof(ShowQuotedLambdaComments)}: {ShowQuotedLambdaComments}".TrimStart();
        }
    }
}