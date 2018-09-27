namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Win32;

    internal class VsPost2017Data : IDisposable
    {
        private static readonly Dictionary<string, string> _vsVersionsByYear = new Dictionary<string, string>
        {
            ["2017"] = "15.0"
        };

        public VsPost2017Data(RegistryKey post2015Key)
        {
            RegistryKey = post2015Key;

            using (var capabilitiesKey = post2015Key.OpenSubKey("Capabilities"))
            {
                VsFullVersionNumber = GetVsFullVersion(capabilitiesKey);
                InstallPath = GetInstallPath(capabilitiesKey);
            }

            IsValid = (VsFullVersionNumber != null) && (InstallPath != null);
        }

        #region Setup

        private static string GetVsFullVersion(RegistryKey capabilitiesKey)
        {
            var appName = capabilitiesKey.GetValue("ApplicationName") as string;

            if (string.IsNullOrWhiteSpace(appName))
            {
                return null;
            }
            
            var vsYearNumber = appName.TrimEnd().Split(' ').LastOrDefault();

            if (vsYearNumber == null)
            {
                return null;
            }

            return _vsVersionsByYear.TryGetValue(vsYearNumber, out var vsVersionNumber)
                ? vsVersionNumber : null;
        }

        private static string GetInstallPath(RegistryKey capabilitiesKey)
        {
            var installPath = capabilitiesKey.GetValue("ApplicationDescription") as string;

            if (string.IsNullOrWhiteSpace(installPath))
            {
                return null;
            }

            var indexOfIde = installPath.IndexOf("IDE", StringComparison.OrdinalIgnoreCase);

            if (indexOfIde == -1)
            {
                return null;
            }

            installPath = installPath.Substring(0, indexOfIde + "IDE".Length);

            if (installPath.StartsWith("@", StringComparison.Ordinal))
            {
                installPath = installPath.Substring(1);
            }

            return Directory.Exists(installPath) ? installPath : null;
        }

        #endregion

        public RegistryKey RegistryKey { get; }

        public string VsFullVersionNumber { get; }

        public string InstallPath { get; }

        public bool IsValid { get; }

        public void Dispose() => RegistryKey.Dispose();
    }
}