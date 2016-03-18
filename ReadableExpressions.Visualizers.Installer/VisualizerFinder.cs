namespace AgileObjects.ReadableExpressions.Visualizers.Installer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class VisualizerFinder
    {
        public static Visualizer GetRelevantVisualizer(string currentDomainBaseDirectory)
        {
            var executingVsVersion = GetExecutingVsVersion(currentDomainBaseDirectory);

            if (executingVsVersion == null)
            {
                return null;
            }

            var visualizer = typeof(VisualizerFinder)
                .Assembly
                .GetManifestResourceNames()
                .Select(visualizerResourceName =>
                    GetVisualizer(currentDomainBaseDirectory, visualizerResourceName))
                .Where(v => v.InstallPath != null)
                .OrderBy(v => v.VsVersionNumber)
                .FirstOrDefault(v => v.VsVersionNumber >= executingVsVersion);

            return visualizer;
        }

        private static int? GetExecutingVsVersion(string currentDomainBaseDirectory)
        {
            if (currentDomainBaseDirectory == null)
            {
                return null;
            }

            var path = Path.Combine(currentDomainBaseDirectory, "msenv.dll");

            if (File.Exists(path))
            {
                return FileVersionInfo.GetVersionInfo(path).ProductMajorPart;
            }

            return null;
        }

        private static Visualizer GetVisualizer(
            string currentDomainBaseDirectory,
            string visualizerResourceName)
        {
            var visualizer = new Visualizer { ResourceName = visualizerResourceName };

            if (TryPopulateVsVersionNumber(visualizer))
            {
                PopulateVisualizerInstallPath(currentDomainBaseDirectory, visualizer);
            }

            return visualizer;
        }

        private static readonly Regex _versionNumberMatcher =
            new Regex(@"Vs(?<VersionNumber>[\d]+)\.dll$", RegexOptions.IgnoreCase);

        private static bool TryPopulateVsVersionNumber(Visualizer visualizer)
        {
            var versionNumberMatch = _versionNumberMatcher.Match(visualizer.ResourceName);

            if (versionNumberMatch.Success)
            {
                visualizer.VsVersionNumber = int.Parse(versionNumberMatch.Groups["VersionNumber"].Value);
                return true;
            }

            return false;
        }

        private static readonly Regex _installPathMatcher =
            new Regex(@"[\/\\]Common7[\/\\]IDE[\/\\]?$", RegexOptions.IgnoreCase);

        private static void PopulateVisualizerInstallPath(
            string currentDomainBaseDirectory,
            Visualizer visualizer)
        {
            var installPath = currentDomainBaseDirectory;

            if (!_installPathMatcher.IsMatch(currentDomainBaseDirectory))
            {
                return;
            }

            var indexOfIde = installPath.IndexOf("IDE", StringComparison.OrdinalIgnoreCase);
            var pathToCommon7 = installPath.Substring(0, indexOfIde);
            var pathToVisualizers = Path.Combine(pathToCommon7, "Packages", "Debugger", "Visualizers");
            var visualizerAssemblyName = visualizer.ResourceName.Substring(typeof(Visualizer).Namespace.Length + 1);

            visualizer.InstallPath = Path.Combine(pathToVisualizers, visualizerAssemblyName);
        }

        public class Visualizer
        {
            public int VsVersionNumber { get; set; }

            public string ResourceName { get; set; }

            public string InstallPath { get; set; }
        }
    }
}