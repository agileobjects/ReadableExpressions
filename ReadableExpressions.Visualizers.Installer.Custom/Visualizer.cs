namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using Microsoft.Win32;

    internal class Visualizer : IDisposable
    {
        public int VsVersionNumber { get; set; }

        public string VsFullVersionNumber => VsVersionNumber + ".0";

        public string ResourceName { get; set; }

        public RegistryKey RegistryKey { get; set; }

        public string VsInstallDirectory { get; set; }

        public string InstallPath { get; set; }

        public string VsixManifestPath { get; set; }

        public string VsExePath { get; set; }

        public string VsSetupArgument { get; set; }

        public Visualizer With(RegistryKey registryKey, string vsInstallPath)
        {
            return new Visualizer
            {
                VsVersionNumber = VsVersionNumber,
                ResourceName = ResourceName,
                RegistryKey = registryKey,
                VsInstallDirectory = vsInstallPath
            };
        }

        public void Dispose()
        {
            RegistryKey?.Dispose();
        }
    }
}