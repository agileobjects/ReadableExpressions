namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using Microsoft.Win32;
    using System;
    using System.IO;

    internal sealed class VisualizerInstaller
    {
        private readonly Action<string> _logger;
        private readonly VsixManifestInstaller _vsixManifestInstaller;
        private readonly Visualizer _visualizer;
        private readonly string _visualizersDirectory;

        public VisualizerInstaller(
            Action<string> logger,
            VsixManifest vsixManifest,
            RegistryKey registryKey,
            string vsInstallDirectory,
            Visualizer visualizer)
        {
            _logger = logger;

            _vsixManifestInstaller = new VsixManifestInstaller(
                logger, 
                registryKey,
                vsInstallDirectory, 
                vsixManifest);

            _visualizer = visualizer;
            VsId = _visualizer.VsYear.ToString();

            var indexOfIde = vsInstallDirectory.StartIndexOfIde();
            var pathToCommon7 = vsInstallDirectory.Substring(0, indexOfIde);

            _visualizersDirectory = Path.Combine(pathToCommon7, "Packages", "Debugger", "Visualizers");
        }

        private string ResourceName => _visualizer.VisualizerResourceName;

        public string VsId { get; }

        public bool Install()
        {
            Log("Installing visualizer " + ResourceName + "...");

            Uninstall();

            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(_visualizersDirectory))
            {
                Log("Skipping as directory does not exist: " + _visualizersDirectory);
                return false;
            }

            _visualizer.Install(_visualizersDirectory);
            _vsixManifestInstaller.Install();
            return true;
        }

        public void Uninstall()
        {
            _vsixManifestInstaller.Uninstall();
            _visualizer.Uninstall(_visualizersDirectory);
        }

        private void Log(string message) => _logger.Invoke(message);
    }
}