#if NETFRAMEWORK
namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    using System.Configuration;
    using System.IO;
    using Io;
    using static BuildConstants;

    internal class NetFrameworkConfigManager : IConfigManager
    {
        private readonly IFileManager _fileManager;

        public NetFrameworkConfigManager(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public string ConfigFileName => "web.config/app.config";

        public Config GetConfigOrNull(string contentRoot)
        {
            var configFilePath = Path.Combine(contentRoot, "web.config");

            if (_fileManager.Exists(configFilePath))
            {
                return GetConfig(configFilePath);
            }

            configFilePath = Path.Combine(contentRoot, "app.config");

            if (_fileManager.Exists(configFilePath))
            {
                return GetConfig(configFilePath);
            }

            return null;
        }

        private static Config GetConfig(string configFilePath)
        {
            var exeConfig = ConfigurationManager.OpenExeConfiguration(configFilePath);

            return new Config
            {
                InputFile = exeConfig.AppSettings.Settings[InputFileKey]?.Value,
                OutputFile = exeConfig.AppSettings.Settings[OutputFileKey]?.Value
            };
        }
    }
}
#endif