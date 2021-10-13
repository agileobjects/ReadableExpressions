namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Win32;

    internal class VsPost2015Data : IDisposable
    {
        private static readonly Dictionary<int, string> _vsVersionsByYear = Visualizer
            .VsYearByVersionNumber
            .Select(kvp => new { Version = kvp.Key, Year = kvp.Value })
            .Where(_ => _.Year > 2015)
            .ToDictionary(_ => _.Year, _ => _.Version + ".0");

        public VsPost2015Data(RegistryKey post2015Key)
        {
            RegistryKey = post2015Key;

            using (var capabilitiesKey = post2015Key.OpenSubKey("Capabilities"))
            {
                VsFullVersionNumber = GetVsFullVersion(capabilitiesKey);
                InstallDirectory = GetInstallPath(capabilitiesKey);
            }

            IsValid = (VsFullVersionNumber != null) && (InstallDirectory != null);
        }

        #region Setup

        private static string GetVsFullVersion(RegistryKey capabilitiesKey)
        {
            var appName = capabilitiesKey.GetValue("ApplicationName") as string;

            if (string.IsNullOrWhiteSpace(appName))
            {
                return null;
            }

            var vsYearNumber = appName
                .TrimEnd()
                .Split(' ')
                .Reverse()
                .Select(segment => int.TryParse(segment, out var yearNumber) ? yearNumber : default(int?))
                .FirstOrDefault(yearNumber => yearNumber != default(int?));

            return vsYearNumber.HasValue && _vsVersionsByYear.TryGetValue(vsYearNumber.Value, out var vsVersionNumber)
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

        public string InstallDirectory { get; }

        public bool IsValid { get; }

        public void Dispose() => RegistryKey.Dispose();
    }
}