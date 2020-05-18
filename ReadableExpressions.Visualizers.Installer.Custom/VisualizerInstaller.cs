namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Win32;
    using static System.StringComparison;

    internal class VisualizerInstaller
    {
        private readonly Action<string> _logger;
        private readonly VsixManifestInstaller _vsixManifestInstaller;
        private readonly VisualizerAssembly _visualizerAssembly;
        private readonly IList<string> _installDirectories;

        public VisualizerInstaller(
            Action<string> logger,
            VsixManifest vsixManifest,
            RegistryKey registryKey,
            string vsInstallDirectory,
            VisualizerAssembly visualizerAssembly)
        {
            _logger = logger;
            
            _vsixManifestInstaller = new VsixManifestInstaller(
                logger, 
                registryKey,
                vsInstallDirectory, 
                vsixManifest);
            
            _visualizerAssembly = visualizerAssembly;
            VsId = _visualizerAssembly.VsYear.ToString();

            var indexOfIde = vsInstallDirectory.IndexOf("IDE", OrdinalIgnoreCase);
            var pathToCommon7 = vsInstallDirectory.Substring(0, indexOfIde);

            var rootInstallDirectory = Path.Combine(pathToCommon7, "Packages", "Debugger", "Visualizers");

            if (visualizerAssembly.IsNetCore)
            {
                VsId += " (.NET Core)";
                rootInstallDirectory = Path.Combine(rootInstallDirectory, "netcoreapp");
            }

            _installDirectories = new List<string> { rootInstallDirectory };

            if (visualizerAssembly.IsNetStandard && Directory.Exists(rootInstallDirectory))
            {
                var netStandardSubDirectories = Directory
                    .EnumerateDirectories(rootInstallDirectory)
                    .Select(path => new DirectoryInfo(path))
                    .Where(dir => dir.Name.StartsWith("netstandard", OrdinalIgnoreCase));

                foreach (var netStandardSubDirectory in netStandardSubDirectories)
                {
                    _installDirectories.Add(netStandardSubDirectory.FullName);
                }
            }
        }

        public string ResourceName => _visualizerAssembly.ResourceName;

        public string VsId { get; }

        private IEnumerable<string> GetInstallPaths()
        {
            return _installDirectories.Select(dir => Path
                .Combine(dir, _visualizerAssembly.ResourceFileName));
        }

        public void Install()
        {
            foreach (var installPath in GetInstallPaths())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                if (!Directory.Exists(Path.GetDirectoryName(installPath)))
                {
                    Log("Skipping as directory does not exist: " + installPath);
                    return;
                }

                _visualizerAssembly.WriteTo(installPath);
            }

            _vsixManifestInstaller.Install();
        }

        public void Uninstall()
        {
            _vsixManifestInstaller.Uninstall();

            foreach (var installPath in GetInstallPaths())
            {
                if (File.Exists(installPath))
                {
                    File.Delete(installPath);
                }
            }
        }

        private void Log(string message) => _logger.Invoke(message);
    }
}