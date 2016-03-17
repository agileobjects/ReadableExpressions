namespace AgileObjects.ReadableExpressions.Visualizers.Installer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Win32;

    public static class VisualizerFinder
    {
        public static IEnumerable<Visualizer> GetVisualizers()
        {
            var visualizers = typeof(VisualizerFinder)
                .Assembly
                .GetManifestResourceNames()
                .Select(visualizerResourceName => new Visualizer
                {
                    ResourceName = visualizerResourceName,
                    InstallPath = GetVisualizerInstallPath(visualizerResourceName)
                })
                .Where(v => v.InstallPath != null)
                .Where(v => !File.Exists(v.InstallPath))
                .ToArray();

            return visualizers;
        }

        private static readonly Regex _installPathMatcher = new Regex(
            @"[\/\\]Common7[\/\\]IDE[\/\\]?$",
            RegexOptions.IgnoreCase);

        private static string GetVisualizerInstallPath(string visualizerResourceName)
        {
            var vsVersionNumber = GetVsVersionNumber(visualizerResourceName);

            if (vsVersionNumber == null)
            {
                return null;
            }

            var registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\" + vsVersionNumber;
            var installPath = (string)Registry.GetValue(registryKey, "InstallDir", defaultValue: null);

            if ((installPath != null) &&
                Directory.Exists(installPath) &&
                _installPathMatcher.IsMatch(installPath))
            {
                var indexOfIde = installPath.IndexOf("IDE", StringComparison.OrdinalIgnoreCase);
                var pathToCommon7 = installPath.Substring(0, indexOfIde);
                var pathToVisualizers = Path.Combine(pathToCommon7, "Packages", "Debugger", "Visualizers");
                var visualizerAssemblyName = visualizerResourceName.Substring(typeof(Program).Namespace.Length + 1);
                var pathToVisualizer = Path.Combine(pathToVisualizers, visualizerAssemblyName);

                return pathToVisualizer;
            }

            return null;
        }

        private static readonly Regex _versionNumberMatcher =
            new Regex(@"Vs(?<VersionNumber>[\d]+)\.dll$", RegexOptions.IgnoreCase);

        private static string GetVsVersionNumber(string assemblyName)
        {
            var versionNumberMatch = _versionNumberMatcher.Match(assemblyName);

            return versionNumberMatch.Success ? versionNumberMatch.Groups["VersionNumber"].Value + ".0" : null;
        }

        public class Visualizer
        {
            public string ResourceName { get; set; }

            public string InstallPath { get; set; }
        }
    }
}