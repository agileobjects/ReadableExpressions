namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Msi.Custom
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Win32;

    [RunInstaller(true)]
    public partial class VisualizerInstaller : Installer
    {
        private static readonly Assembly _thisAssembly = typeof(VisualizerInstaller).Assembly;
        private static readonly FileVersionInfo _thisAssemblyVersion = FileVersionInfo.GetVersionInfo(_thisAssembly.Location);

        private readonly Lazy<string> _vsixManifestResourceNameLoader;
        private readonly Lazy<string> _vsixManifestLoader;

        public VisualizerInstaller()
        {
            InitializeComponent();

            _vsixManifestResourceNameLoader = new Lazy<string>(GetVsixManifestResourceName);
            _vsixManifestLoader = new Lazy<string>(GetVsixManifest);
        }

        private static string GetVsixManifestResourceName()
        {
            return _thisAssembly
                .GetManifestResourceNames()
                .WithExtension("vsixmanifest")
                .First();
        }

        private string GetVsixManifest()
        {
            using (var resourceStream = _thisAssembly.GetManifestResourceStream(VsixManifestResourceName))
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        private string VsixManifestResourceName => _vsixManifestResourceNameLoader.Value;

        private string VsixManifest => _vsixManifestLoader.Value;

        [SecurityPermission(SecurityAction.Demand)]
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            foreach (var visualizer in GetRelevantVisualizers())
            {
                Delete(visualizer);
                Write(visualizer);
            }
        }

        private IEnumerable<Visualizer> GetRelevantVisualizers()
        {
            return _thisAssembly
                .GetManifestResourceNames()
                .WithExtension("dll")
                .Select(visualizerResourceName => new Visualizer
                {
                    ResourceName = visualizerResourceName,
                    VsVersionNumber = GetVsVersionNumber(visualizerResourceName)
                })
                .Where(TryPopulateInstallPaths);
        }

        private static readonly Regex _versionNumberMatcher =
            new Regex(@"Vs(?<VersionNumber>[\d]+)\.dll$", RegexOptions.IgnoreCase);

        private static int GetVsVersionNumber(string visualizerResourceName)
        {
            var matchValue = _versionNumberMatcher
                .Match(visualizerResourceName)
                .Groups["VersionNumber"]
                .Value;

            return int.Parse(matchValue);
        }

        private static bool TryPopulateInstallPaths(Visualizer visualizer)
        {
            var registryKey = $@"SOFTWARE\Microsoft\VisualStudio\{visualizer.VsVersionNumber}.0";

            using (var localMachineKey = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                var vsInstallPath = localMachineKey?.GetValue("InstallDir") as string;

                if (vsInstallPath == null)
                {
                    return false;
                }

                var indexOfIde = vsInstallPath.IndexOf("IDE", StringComparison.OrdinalIgnoreCase);
                var pathToCommon7 = vsInstallPath.Substring(0, indexOfIde);
                var pathToVisualizers = Path.Combine(pathToCommon7, "Packages", "Debugger", "Visualizers");
                var visualizerAssemblyName = GetResourceFileName(visualizer.ResourceName);
                var pathToExtensions = GetPathToExtensions(vsInstallPath);

                visualizer.InstallPath = Path.Combine(pathToVisualizers, visualizerAssemblyName);
                visualizer.VsixManifestPath = Path.Combine(pathToExtensions, "extension.vsixmanifest");

                return true;
            }
        }

        private static string GetPathToExtensions(string vsInstallPath)
        {
            return Path.Combine(
                vsInstallPath,
                "Extensions",
                _thisAssemblyVersion.CompanyName,
                _thisAssemblyVersion.ProductName,
                _thisAssemblyVersion.FileVersion);
        }

        private static string GetResourceFileName(string resourceName)
        {
            var resourceAssemblyNameLength = (typeof(Visualizer).Namespace?.Length + 1).GetValueOrDefault();
            var resourceFileName = resourceName.Substring(resourceAssemblyNameLength);

            return resourceFileName;
        }

        private void Write(Visualizer visualizer)
        {
            using (var resourceStream = _thisAssembly.GetManifestResourceStream(visualizer.ResourceName))
            using (var visualizerFileStream = File.OpenWrite(visualizer.InstallPath))
            {
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(visualizerFileStream);
            }

            var manifestDirectory = Path.GetDirectoryName(visualizer.VsixManifestPath);

            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(manifestDirectory);
            File.WriteAllText(visualizer.VsixManifestPath, VsixManifest, Encoding.ASCII);
        }

        [SecurityPermission(SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            foreach (var visualizer in GetRelevantVisualizers())
            {
                Delete(visualizer);
            }
        }

        private static void Delete(Visualizer visualizer)
        {
            if (File.Exists(visualizer.VsixManifestPath))
            {
                var vsixManifestPath = Path.GetDirectoryName(visualizer.VsixManifestPath);

                // ReSharper disable once PossibleNullReferenceException
                var productNameIndex = vsixManifestPath.IndexOf(_thisAssemblyVersion.ProductName, StringComparison.Ordinal);
                var endOfProductNameIndex = productNameIndex + _thisAssemblyVersion.ProductName.Length;
                var productDirectory = vsixManifestPath.Substring(0, endOfProductNameIndex);

                Directory.Delete(productDirectory, recursive: true);
            }

            File.Delete(visualizer.InstallPath);
        }
    }
}
