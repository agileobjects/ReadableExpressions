#if NET_STANDARD
namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    using System.IO;
    using Microsoft.Extensions.Configuration;

    internal class NetStandardConfigManager : IConfigManager
    {
        public Config GetConfigOrNull(string contentRoot)
        {
            var appSettingsFile = new FileInfo(Path.Combine(contentRoot, "appsettings.json"));

            if (!appSettingsFile.Exists)
            {
                return null;
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", optional: false);

            var appSettings = builder.Build();

            return new Config
            {
                InputFile = appSettings["appSettings.reBuildInput"],
                OutputFile = appSettings["appSettings.reBuildOutput"]
            };
        }
    }
}
#endif