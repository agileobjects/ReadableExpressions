#if NETFRAMEWORK
namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    using System.Configuration;
    using System.IO;
    using static BuildConstants;

    internal class NetFrameworkConfigManager : IConfigManager
    {
        public string ConfigFileName => "web.config/app.config";

        public Config GetConfigOrNull(string contentRoot, out FileInfo configFile)
        {
            var webConfigFile = new FileInfo(Path.Combine(contentRoot, "web.config"));

            if (webConfigFile.Exists)
            {
                return GetConfig(configFile = webConfigFile);
            }

            var appConfigFile = new FileInfo(Path.Combine(contentRoot, "app.config"));

            if (appConfigFile.Exists)
            {
                return GetConfig(configFile = appConfigFile);
            }

            configFile = null;
            return null;
        }

        private static Config GetConfig(FileSystemInfo configFile)
        {
            var exeConfig = GetConfiguration(configFile);

            return new Config
            {
                InputFile = exeConfig.AppSettings.Settings[InputFileKey]?.Value,
                OutputFile = exeConfig.AppSettings.Settings[OutputFileKey]?.Value
            };
        }

        private static Configuration GetConfiguration(FileSystemInfo configFile)
            => ConfigurationManager.OpenExeConfiguration(configFile.FullName);

        public void SetDefaults(FileInfo configFile)
        {
            var exeConfig = GetConfiguration(configFile);
            exeConfig.AppSettings.Settings.Add(InputFileKey, DefaultInputFile);
            exeConfig.AppSettings.Settings.Add(OutputFileKey, DefaultOutputFile);
            exeConfig.Save(ConfigurationSaveMode.Modified);
        }
    }
}
#endif