#if NET_STANDARD
namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    using System.IO;
    using Io;
    using Microsoft.Extensions.Configuration;
    using static BuildConstants;

    internal class NetStandardConfigManager : IConfigManager
    {
        private readonly IFileManager _fileManager;

        public NetStandardConfigManager(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public string ConfigFileName => "appsettings.json";

        public Config GetConfigOrNull(string contentRoot)
        {
            var configFilePath = Path.Combine(contentRoot, ConfigFileName);

            if (!_fileManager.Exists(configFilePath))
            {
                return null;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", optional: false);

            var appSettings = builder.Build();

            return new Config
            {
                InputFile = appSettings[$"appSettings:{InputFileKey}"],
                OutputFile = appSettings[$"appSettings:{OutputFileKey}"]
            };
        }
    }
}
#endif