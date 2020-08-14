namespace AgileObjects.ReadableExpressions.Build.Configuration
{
    internal interface IConfigManager
    {
        Config GetConfigOrNull(string contentRoot);
    }
}
