namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.IO;
    using Microsoft.Win32;
    using static System.StringComparison;

    internal class VisualizerInstaller
    {
        private readonly Action<string> _logger;
        private readonly VsixManifestInstaller _vsixManifestInstaller;
        private readonly VisualizerAssembly _visualizerAssembly;
        private readonly string _visualizersDirectory;

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

            _visualizersDirectory = Path.Combine(pathToCommon7, "Packages", "Debugger", "Visualizers");
        }

        public string ResourceName => _visualizerAssembly.VisualizerResourceName;

        public string VsId { get; }

        public void Install()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(_visualizersDirectory))
            {
                Log("Skipping as directory does not exist: " + _visualizersDirectory);
                return;
            }

            _visualizerAssembly.Install(_visualizersDirectory);
            _vsixManifestInstaller.Install();
        }

        public void Uninstall()
        {
            _vsixManifestInstaller.Uninstall();
            _visualizerAssembly.Uninstall(_visualizersDirectory);
        }

        private void Log(string message) => _logger.Invoke(message);
    }
}