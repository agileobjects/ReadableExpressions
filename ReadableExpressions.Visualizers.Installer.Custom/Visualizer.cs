namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Microsoft.Win32;

    internal class Visualizer : IDisposable
    {
        private static readonly Assembly _thisAssembly = typeof(Visualizer).Assembly;

        private readonly Action<string> _logger;
        private readonly Version _version;
        private readonly string _vsixManifest;

        public Visualizer(Action<string> logger, FileVersionInfo version, string vsixManifest)
            : this(logger, new Version(version.FileVersion), vsixManifest)
        {
        }

        private Visualizer(Action<string> logger, Version version, string vsixManifest)
        {
            _logger = logger;
            _version = version;
            _vsixManifest = vsixManifest;
        }

        public int VsVersionNumber { get; set; }

        public string VsFullVersionNumber => VsVersionNumber + ".0";

        public string ResourceName { get; set; }

        public RegistryKey RegistryKey { get; set; }

        public string VsInstallDirectory { get; set; }

        public string InstallPath { get; set; }

        public string VsixManifestPath { get; set; }

        public string VsExePath { get; set; }

        public string VsSetupArgument { get; set; }

        public void Install()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(Path.GetDirectoryName(InstallPath)))
            {
                Log("Skipping as directory does not exist: " + InstallPath);
                return;
            }

            using (var resourceStream = _thisAssembly.GetManifestResourceStream(ResourceName))
            using (var visualizerFileStream = File.OpenWrite(InstallPath))
            {
                Log("Writing visualizer to " + InstallPath);
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(visualizerFileStream);
            }

            var manifestDirectory = Path.GetDirectoryName(VsixManifestPath);

            Log("Writing manifest to " + VsixManifestPath);
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(manifestDirectory);
            File.WriteAllText(VsixManifestPath, _vsixManifest, Encoding.ASCII);

            ResetVsExtensions();
        }

        public DirectoryInfo GetVsixManifestDirectory()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return new DirectoryInfo(Path.GetDirectoryName(VsixManifestPath));
        }

        public void Uninstall()
        {
            DeletePreviousManifests();

            // ReSharper disable PossibleNullReferenceException
            if (File.Exists(VsixManifestPath))
            {
                var vsixManifestDirectory = GetVsixManifestDirectory();
                var productDirectory = vsixManifestDirectory.Parent;
                var companyDirectory = productDirectory.Parent;

                Log("Deleting previous manifest directory " + vsixManifestDirectory.FullName);
                vsixManifestDirectory.Delete(recursive: true);

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

                ResetVsExtensions();
            }
            // ReSharper restore PossibleNullReferenceException

            if (File.Exists(InstallPath))
            {
                File.Delete(InstallPath);
            }
        }

        private void DeletePreviousManifests()
        {
            var productDirectory = GetVsixManifestDirectory().Parent;

            if ((productDirectory == null) || !productDirectory.Exists)
            {
                return;
            }

            // ReSharper disable once PossibleNullReferenceException
            foreach (var versionDirectory in productDirectory.GetDirectories("*"))
            {
                if (Version.TryParse(versionDirectory.Name, out var directoryVersion))
                {
                    if (directoryVersion != _version)
                    {
                        Log("Deleting previous manifest version directory " + versionDirectory.FullName);
                        versionDirectory.Delete(recursive: true);
                    }
                }
            }
        }

        private void ResetVsExtensions()
        {
            if (VsExePath == null)
            {
                return;
            }

            Log("Updating VS extension records using " + VsExePath);
            using (Process.Start(VsExePath, VsSetupArgument)) { }
        }

        public void PopulateVsSetupData()
        {
            var pathToDevEnv = Path.Combine(VsInstallDirectory, "devenv.exe");

            if (!File.Exists(pathToDevEnv))
            {
                return;
            }

            VsExePath = pathToDevEnv;
            VsSetupArgument = RegistryKey?.GetValue("SetupCommandLine") as string ?? "/setup";
        }

        public Visualizer With(RegistryKey registryKey, string vsInstallPath)
        {
            return new Visualizer(_logger, _version, _vsixManifest)
            {
                VsVersionNumber = VsVersionNumber,
                ResourceName = ResourceName,
                RegistryKey = registryKey,
                VsInstallDirectory = vsInstallPath
            };
        }

        private void Log(string message) => _logger.Invoke(message);

        public void Dispose() => RegistryKey?.Dispose();
    }
}