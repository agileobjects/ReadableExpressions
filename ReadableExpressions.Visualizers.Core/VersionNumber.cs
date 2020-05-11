namespace AgileObjects.ReadableExpressions.Visualizers.Core
{
    using System;
    using System.Diagnostics;

    public static class VersionNumber
    {
        private static readonly FileVersionInfo _versionInfo = FileVersionInfo
            .GetVersionInfo(typeof(VersionNumber).Assembly.Location);

        public static Version Version = new Version(FileVersion);

        public static string CompanyName => _versionInfo.CompanyName;
        
        public static string ProductName => _versionInfo.ProductName;

        public static string FileVersion => _versionInfo.FileVersion;
    }
}
