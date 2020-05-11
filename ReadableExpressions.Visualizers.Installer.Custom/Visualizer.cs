namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Core;
    using Microsoft.Win32;

    internal class Visualizer : IDisposable
    {
        private static readonly Assembly _thisAssembly = typeof(Visualizer).Assembly;

        private readonly Action<string> _logger;
        private readonly string _vsixManifest;
        private RegistryKey _registryKey;
        private string _vsExePath;
        private string _vsSetupArgument;
        private string _installPath;
        private string _vsixManifestPath;

        public Visualizer(
            Action<string> logger,
            string vsixManifest,
            string resourceName)
            : this(logger, vsixManifest, resourceName, GetVsVersionNumber(resourceName))
        {
        }

        #region Setup

        private static readonly Regex _versionNumberMatcher =
            new Regex(@"Vs(?<VersionNumber>[\d]+)\.dll$", RegexOptions.IgnoreCase);

        private static int GetVsVersionNumber(string resourceName)
        {
            var matchValue = _versionNumberMatcher
                .Match(resourceName)
                .Groups["VersionNumber"]
                .Value;

            return int.Parse(matchValue);
        }

        #endregion

        private Visualizer(
            Action<string> logger,
            string vsixManifest,
            string resourceName,
            int vsVersionNumber)
        {
            _logger = logger;
            _vsixManifest = vsixManifest;
            ResourceName = resourceName;
            VsVersionNumber = vsVersionNumber;
        }

        public int VsVersionNumber { get; }

        public string VsFullVersionNumber => VsVersionNumber + ".0";

        public string ResourceName { get; }

        public string GetResourceFileName()
        {
            var resourceAssemblyNameLength = (typeof(Visualizer).Namespace?.Length + 1).GetValueOrDefault();
            var resourceFileName = ResourceName.Substring(resourceAssemblyNameLength);

            return resourceFileName;
        }

        public string VsInstallDirectory { get; private set; }

        public void SetInstallPath(string pathToVisualizers)
            => _installPath = Path.Combine(pathToVisualizers, GetResourceFileName());

        public void SetVsixManifestPath(string pathToExtensions)
            => _vsixManifestPath = Path.Combine(pathToExtensions, "extension.vsixmanifest");

        public void Install()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(Path.GetDirectoryName(_installPath)))
            {
                Log("Skipping as directory does not exist: " + _installPath);
                return;
            }

            using (var resourceStream = _thisAssembly.GetManifestResourceStream(ResourceName))
            using (var visualizerFileStream = File.OpenWrite(_installPath))
            {
                Log("Writing visualizer to " + _installPath);
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(visualizerFileStream);
            }

            var manifestDirectory = Path.GetDirectoryName(_vsixManifestPath);

            Log("Writing manifest to " + _vsixManifestPath);
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(manifestDirectory);
            File.WriteAllText(_vsixManifestPath, _vsixManifest, Encoding.ASCII);

            ResetVsExtensions();
        }

        public DirectoryInfo GetVsixManifestDirectory()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return new DirectoryInfo(Path.GetDirectoryName(_vsixManifestPath));
        }

        public void Uninstall()
        {
            DeletePreviousManifests();

            // ReSharper disable PossibleNullReferenceException
            if (File.Exists(_vsixManifestPath))
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

            if (File.Exists(_installPath))
            {
                File.Delete(_installPath);
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
                    if (directoryVersion != VersionNumber.Version)
                    {
                        Log("Deleting previous manifest version directory " + versionDirectory.FullName);
                        versionDirectory.Delete(recursive: true);
                    }
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

        public void PopulateVsSetupData()
        {
            var pathToDevEnv = Path.Combine(VsInstallDirectory, "devenv.exe");

            if (!File.Exists(pathToDevEnv))
            {
                return;
            }

            _vsExePath = pathToDevEnv;
            _vsSetupArgument = _registryKey?.GetValue("SetupCommandLine") as string ?? "/setup";
        }

        public Visualizer WithInstallPath(string pathToVisualizers)
        {
            var visualizer = With(_registryKey, VsInstallDirectory);

            visualizer.SetInstallPath(pathToVisualizers);

            return visualizer;
        }

        public Visualizer With(RegistryKey registryKey, string vsInstallPath)
        {
            return new Visualizer(_logger, _vsixManifest, ResourceName, VsVersionNumber)
            {
                _registryKey = registryKey,
                VsInstallDirectory = vsInstallPath,
                _vsExePath = _vsExePath,
                _vsSetupArgument = _vsSetupArgument,
                _installPath = _installPath,
                _vsixManifestPath = _vsixManifestPath
            };
        }

        private void Log(string message) => _logger.Invoke(message);

        public void Dispose() => _registryKey?.Dispose();
    }
}