namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    internal interface IConfigManager
    {
        string ConfigFileName { get; }

        Config GetConfigOrNull(string contentRoot);
    }
}
