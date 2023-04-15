namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Core;
    using Microsoft.Win32;

    internal class VsixManifestInstaller
    {
        private readonly Action<string> _logger;
        private readonly VsixManifest _vsixManifest;
        private readonly string _installDirectory;
        private readonly DirectoryInfo _installDirectoryInfo;
        private readonly string _installPath;
        private readonly string _vsExePath;
        private readonly string _vsSetupArgument;

        public VsixManifestInstaller(
            Action<string> logger,
            RegistryKey registryKey,
            string vsInstallDirectory,
            VsixManifest vsixManifest)
        {
            _logger = logger;
            _vsixManifest = vsixManifest;

            _installDirectory = Path.Combine(
                vsInstallDirectory,
                "Extensions",
                VersionNumber.CompanyName,
                VersionNumber.ProductName,
                VersionNumber.FileVersion);

            _installDirectoryInfo = new DirectoryInfo(_installDirectory);
            _installPath = Path.Combine(_installDirectory, "extension.vsixmanifest");

            var pathToDevEnv = Path.Combine(vsInstallDirectory, "devenv.exe");

            if (!File.Exists(pathToDevEnv))
            {
                return;
            }

            _vsExePath = pathToDevEnv;
            _vsSetupArgument = registryKey?.GetValue("SetupCommandLine") as string ?? "/setup";
        }

        public void Install()
        {
            Log("Writing manifest to " + _installDirectory);
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(_installDirectory);
            File.WriteAllText(_installPath, _vsixManifest.Content, Encoding.ASCII);

            ResetVsExtensions();
        }

        public void Uninstall()
        {
            DeletePreviousVersionManifests();

            if (!File.Exists(_installPath))
            {
                return;
            }

            // ReSharper disable PossibleNullReferenceException
            var productDirectory = _installDirectoryInfo.Parent;
            var companyDirectory = productDirectory.Parent;

            Log("Deleting previous manifest directory " + _installDirectoryInfo.FullName);
            _installDirectoryInfo.Delete(recursive: true);

            if (!productDirectory.GetDirectories().Any())
            {
                Log("Deleting previous manifest product directory " + productDirectory.FullName);
                productDirectory.Delete(recursive: true);

                if (!companyDirectory.GetDirectories().Any())
                {
                    Log("Deleting previous manifest company directory " + companyDirectory.FullName);
                    companyDirectory.Delete(recursive: true);
                }
            }
            // ReSharper restore PossibleNullReferenceException

            ResetVsExtensions();
        }

        private void DeletePreviousVersionManifests()
        {
            var productDirectory = _installDirectoryInfo.Parent;

            if ((productDirectory == null) || !productDirectory.Exists)
            {
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            foreach (var versionDirectory in productDirectory.GetDirectories("*"))
            {
                if (Version.TryParse(versionDirectory.Name, out var directoryVersion) &&
                   (directoryVersion != VersionNumber.Version))
                {
                    Log("Deleting previous manifest version directory " + versionDirectory.FullName);
                    versionDirectory.Delete(recursive: true);
                }
            }
        }

        private void ResetVsExtensions()
        {
            if (_vsExePath == null)
            {
                return;
            }

            Log("Updating VS extension records using " + _vsExePath);
            using (Process.Start(_vsExePath, _vsSetupArgument)) { }
        }

        private void Log(string message) => _logger.Invoke(message);
    }
}