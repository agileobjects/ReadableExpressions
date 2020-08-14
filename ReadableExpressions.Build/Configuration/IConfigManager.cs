namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    using System.IO;

    internal interface IConfigManager
    {
        string ConfigFileName { get; }

        Config GetConfigOrNull(string contentRoot, out FileInfo configFile);

        void SetDefaults(FileInfo configFile);
    }
}
