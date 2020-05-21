namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using static System.StringComparison;

    internal class VisualizerAssembly
    {
        private static readonly Assembly _thisAssembly = typeof(VisualizerAssembly).Assembly;

        private static readonly Regex _versionNumberMatcher =
            new Regex(@"Vs(?<VersionNumber>[\d]+)\.dll$", RegexOptions.IgnoreCase);

        private static readonly Dictionary<string, int> _vsYearByVersionNumber = new Dictionary<string, int>(6)
        {
            ["10"] = 2010,
            ["11"] = 2012,
            ["12"] = 2013,
            ["14"] = 2015,
            ["15"] = 2017,
            ["16"] = 2019
        };

        private readonly Action<string> _logger;
        private readonly string _visualizerResourceFileName;
        private readonly string _objectSourceResourceName;
        private readonly string _objectSourceResourceFileName;

        public VisualizerAssembly(Action<string> logger, string visualizerResourceName)
        {
            _logger = logger;
            VisualizerResourceName = visualizerResourceName;

            var resourceAssemblyNameLength = (typeof(VisualizerAssembly).Namespace?.Length + 1).GetValueOrDefault();
            _visualizerResourceFileName = visualizerResourceName.Substring(resourceAssemblyNameLength);

            _objectSourceResourceName = visualizerResourceName.Replace(".dll", ".ObjectSource.dll");
            _objectSourceResourceFileName = _objectSourceResourceName.Substring(resourceAssemblyNameLength);

            var versionInfoMatch = _versionNumberMatcher.Match(visualizerResourceName);
            var vsVersionNumber = versionInfoMatch.Groups["VersionNumber"].Value;

            VsFullVersionNumber = vsVersionNumber + ".0";
            VsYear = _vsYearByVersionNumber[vsVersionNumber];
        }

        public string VisualizerResourceName { get; }

        public int VsYear { get; }

        public string VsFullVersionNumber { get; }

        public void Install(string visualizersDirectory)
        {
            var installPath = GetVisualizerInstallPath(visualizersDirectory);

            using (var resourceStream = GetResourceStream(VisualizerResourceName))
            using (var visualizerFileStream = File.OpenWrite(installPath))
            {
                Log("Writing visualizer to " + installPath);
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(visualizerFileStream);
            }

            var objectSourceInstallPaths = GetObjectSourceInstallPaths(visualizersDirectory);

            foreach (var objectSourceInstallPath in objectSourceInstallPaths)
            {
                using (var resourceStream = GetResourceStream(_objectSourceResourceName))
                using (var objectSourceFileStream = File.OpenWrite(objectSourceInstallPath))
                {
                    Log("Writing object source to " + objectSourceInstallPath);
                    // ReSharper disable once PossibleNullReferenceException
                    resourceStream.CopyTo(objectSourceFileStream);
                }
            }
        }

        private static Stream GetResourceStream(string resourceName)
            => _thisAssembly.GetManifestResourceStream(resourceName);

        private IEnumerable<string> GetObjectSourceInstallPaths(string visualizersDirectory)
        {
            return GetObjectSourceInstallDirectories(visualizersDirectory)
                .Select(GetObjectSourceInstallPath);
        }

        private IEnumerable<string> GetObjectSourceInstallDirectories(string visualizersDirectory)
        {
            if (InstallObjectSourceInVisualizersDirectory)
            {
                yield return visualizersDirectory;
                yield break;
            }

            var netStandardSubDirectories = Directory
                .EnumerateDirectories(visualizersDirectory)
                .Select(path => new DirectoryInfo(path))
                .Where(dir => 
                    dir.Name.Equals("net2.0", OrdinalIgnoreCase) ||
                    dir.Name.StartsWith("netstandard", OrdinalIgnoreCase));

            foreach (var netStandardSubDirectory in netStandardSubDirectories)
            {
                yield return netStandardSubDirectory.FullName;
            }
        }

        private bool InstallObjectSourceInVisualizersDirectory => VsYear < 2019;

        public void Uninstall(string visualizersDirectory)
        {
            var installPath = GetVisualizerInstallPath(visualizersDirectory);

            if (File.Exists(installPath))
            {
                File.Delete(installPath);
            }

            var objectSourceInstallPaths = GetObjectSourceInstallPaths(visualizersDirectory);

            foreach (var objectSourceInstallPath in objectSourceInstallPaths)
            {
                if (File.Exists(objectSourceInstallPath))
                {
                    File.Delete(objectSourceInstallPath);
                }
            }
        }

        private string GetVisualizerInstallPath(string visualizersDirectory)
            => Path.Combine(visualizersDirectory, _visualizerResourceFileName);

        private string GetObjectSourceInstallPath(string objectSourceDirectory)
            => Path.Combine(objectSourceDirectory, _objectSourceResourceFileName);

        private void Log(string message) => _logger.Invoke(message);
    }
}