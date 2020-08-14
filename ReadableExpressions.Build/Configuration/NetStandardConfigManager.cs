#if NET_STANDARD
namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using static BuildConstants;

    internal class NetStandardConfigManager : IConfigManager
    {
        public string ConfigFileName => "appsettings.json";

        public Config GetConfigOrNull(string contentRoot, out FileInfo configFile)
        {
            configFile = new FileInfo(Path.Combine(contentRoot, "appsettings.json"));

            if (!configFile.Exists)
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

        public void SetDefaults(FileInfo configFile)
        {
            var configJsonString = File.ReadAllText(configFile.FullName);
            var configJson = JObject.Parse(configJsonString);

            if (!configJson.TryGetValue("appSettings", out var appSettingsJson) ||
                appSettingsJson == null)
            {
                appSettingsJson = new JObject();
            }

            appSettingsJson[InputFileKey] = DefaultInputFile;
            appSettingsJson[OutputFileKey] = DefaultOutputFile;

            configJsonString = JsonConvert.SerializeObject(configJson, Formatting.Indented);
            File.WriteAllText(configFile.FullName, configJsonString);
        }
    }
}
#endif