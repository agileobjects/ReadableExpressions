namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal class VisualizerAssembly
    {
        private static readonly Assembly _thisAssembly = typeof(VisualizerAssembly).Assembly;

        private static readonly Regex _versionNumberMatcher =
            new Regex(@"Vs(?<VersionNumber>[\d]+)\.(?<NetCore>NetCore\.)?dll$", RegexOptions.IgnoreCase);

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

        public VisualizerAssembly(Action<string> logger, string resourceName)
        {
            _logger = logger;
            ResourceName = resourceName;

            var resourceAssemblyNameLength = (typeof(VisualizerAssembly).Namespace?.Length + 1).GetValueOrDefault();
            ResourceFileName = resourceName.Substring(resourceAssemblyNameLength);

            var versionInfoMatch = _versionNumberMatcher.Match(resourceName);
            var vsVersionNumber = versionInfoMatch.Groups["VersionNumber"].Value;

            VsFullVersionNumber = vsVersionNumber + ".0";
            VsYear = _vsYearByVersionNumber[vsVersionNumber];
            IsNetCore = versionInfoMatch.Groups["NetCore"].Success;
        }

        public string ResourceName { get; }

        public string ResourceFileName { get; }

        public int VsYear { get; }

        public string VsFullVersionNumber { get; }

        public bool IsNetCore { get; }

        public bool IsNetStandard => !IsNetCore && VsYear >= 2017;

        public void WriteTo(string path)
        {
            using (var resourceStream = _thisAssembly.GetManifestResourceStream(ResourceName))
            using (var visualizerFileStream = File.OpenWrite(path))
            {
                Log("Writing visualizer to " + path);
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(visualizerFileStream);
            }
        }

        private void Log(string message) => _logger.Invoke(message);
    }
}