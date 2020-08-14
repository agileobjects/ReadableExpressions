#if NETFRAMEWORK
namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    using System.Configuration;
    using System.IO;

    internal class NetFrameworkConfigManager : IConfigManager
    {
        public Config GetConfigOrNull(string contentRoot)
        {
            var webConfigFile = new FileInfo(Path.Combine(contentRoot, "web.config"));

            if (webConfigFile.Exists)
            {
                return GetConfig(webConfigFile);
            }

            var appConfigFile = new FileInfo(Path.Combine(contentRoot, "app.config"));

            if (appConfigFile.Exists)
            {
                return GetConfig(appConfigFile);
            }

            return null;
        }

        private static Config GetConfig(FileSystemInfo configFile)
        {
            var exeConfig = ConfigurationManager.OpenExeConfiguration(configFile.FullName);
            
            return new Config
            {
                InputFile = exeConfig.AppSettings.Settings["ReBuildInput"]?.Value,
                OutputFile = exeConfig.AppSettings.Settings["ReBuildOutput"]?.Value
            };
        }
    }
}
#endif