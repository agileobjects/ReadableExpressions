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
                return streamReader
                    .ReadToEnd()
                    .Replace("$version$", _thisAssemblyVersion.FileVersion)
                    .Replace("$author$", _thisAssemblyVersion.CompanyName);
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

        private static IEnumerable<Visualizer> GetRelevantVisualizers()
        {
            const string REGISTRY_KEY = @"SOFTWARE\Microsoft\VisualStudio";

            using (var vsMachineKey = Registry.LocalMachine.OpenSubKey(REGISTRY_KEY))
            {
                if (vsMachineKey == null)
                {
                    return Enumerable.Empty<Visualizer>();
                }

                var vsSubKeyNames = vsMachineKey.GetSubKeyNames();

                if (!vsSubKeyNames.Any())
                {
                    return Enumerable.Empty<Visualizer>();
                }

                return _thisAssembly
                    .GetManifestResourceNames()
                    .WithExtension("dll")
                    .Select(visualizerResourceName => new Visualizer
                    {
                        ResourceName = visualizerResourceName,
                        VsVersionNumber = GetVsVersionNumber(visualizerResourceName)
                    })
                    .SelectMany(visualizer => PopulateInstallPaths(visualizer, vsMachineKey, vsSubKeyNames))
                    .ToArray();
            }
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

        private static IEnumerable<Visualizer> PopulateInstallPaths(
            Visualizer visualizer,
            RegistryKey vsMachineKey,
            IEnumerable<string> vsSubKeyNames)
        {
            ICollection<Visualizer> targetVisualizers;

            if (!TryPopulateInstallPaths(vsMachineKey, vsSubKeyNames, visualizer, out targetVisualizers))
            {
                yield break;
            }

            foreach (var targetVisualizer in targetVisualizers)
            {
                using (targetVisualizer)
                {
                    var vsInstallPath = targetVisualizer.VsInstallDirectory;
                    var indexOfIde = vsInstallPath.IndexOf("IDE", StringComparison.OrdinalIgnoreCase);
                    var pathToCommon7 = vsInstallPath.Substring(0, indexOfIde);
                    var pathToVisualizers = Path.Combine(pathToCommon7, "Packages", "Debugger", "Visualizers");
                    var visualizerAssemblyName = GetResourceFileName(targetVisualizer.ResourceName);
                    var pathToExtensions = GetPathToExtensions(vsInstallPath);

                    targetVisualizer.InstallPath = Path.Combine(pathToVisualizers, visualizerAssemblyName);
                    targetVisualizer.VsixManifestPath = Path.Combine(pathToExtensions, "extension.vsixmanifest");

                    PopulateVsSetupData(targetVisualizer);

                    yield return targetVisualizer;
                }
            }
        }

        private static bool TryPopulateInstallPaths(
            RegistryKey vsMachineKey,
            IEnumerable<string> vsSubKeyNames,
            Visualizer visualizer,
            out ICollection<Visualizer> targetVisualizers)
        {
            targetVisualizers = new List<Visualizer>();

            Visualizer targetVisualizer;

            if (TryGetPreVersion15InstallPath(vsMachineKey, vsSubKeyNames, visualizer, out targetVisualizer))
            {
                targetVisualizers.Add(targetVisualizer);
            }

            if (TryGetPostVersion14InstallPath(vsSubKeyNames, visualizer, out targetVisualizer))
            {
                targetVisualizers.Add(targetVisualizer);
            }

            return targetVisualizers.Any();
        }

        private static bool TryGetPreVersion15InstallPath(
            RegistryKey vsMachineKey,
            IEnumerable<string> vsSubKeyNames,
            Visualizer visualizer,
            out Visualizer targetVisualizer)
        {
            var preVersion15Key = vsSubKeyNames.FirstOrDefault(name => name == visualizer.VsFullVersionNumber);

            if (preVersion15Key == null)
            {
                targetVisualizer = null;
                return false;
            }

            var vsSubKey = vsMachineKey.OpenSubKey(preVersion15Key);
            var installPath = vsSubKey?.GetValue("InstallDir") as string;

            if (string.IsNullOrWhiteSpace(installPath))
            {
                targetVisualizer = null;
                return false;
            }

            targetVisualizer = visualizer.With(vsSubKey, installPath);
            return true;
        }

        private static bool TryGetPostVersion14InstallPath(
            IEnumerable<string> vsSubKeyNames,
            Visualizer visualizer,
            out Visualizer targetVisualizer)
        {
            var post2015Key = vsSubKeyNames.FirstOrDefault(name =>
                name.StartsWith(visualizer.VsFullVersionNumber + "_", StringComparison.Ordinal));

            if (post2015Key == null)
            {
                targetVisualizer = null;
                return false;
            }

            var post2015Suffix = post2015Key.Substring(visualizer.VsFullVersionNumber.Length);
            var post2015KeyPath = @"SOFTWARE\Microsoft\VisualStudio" + post2015Suffix + @"\Capabilities";

            var post2015MachineKey = Registry.LocalMachine.OpenSubKey(post2015KeyPath);
            var installPath = post2015MachineKey?.GetValue("ApplicationDescription") as string;

            if (string.IsNullOrWhiteSpace(installPath))
            {
                targetVisualizer = null;
                return false;
            }

            var indexOfIde = installPath.IndexOf("IDE", StringComparison.OrdinalIgnoreCase);

            if (indexOfIde == -1)
            {
                targetVisualizer = null;
                return false;
            }

            installPath = installPath.Substring(0, indexOfIde + "IDE".Length);

            if (installPath.StartsWith("@", StringComparison.Ordinal))
            {
                installPath = installPath.Substring(1);
            }

            targetVisualizer = visualizer.With(post2015MachineKey, installPath);
            return true;
        }

        private static string GetResourceFileName(string resourceName)
        {
            var resourceAssemblyNameLength = (typeof(Visualizer).Namespace?.Length + 1).GetValueOrDefault();
            var resourceFileName = resourceName.Substring(resourceAssemblyNameLength);

            return resourceFileName;
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

        private static void PopulateVsSetupData(Visualizer visualizer)
        {
            var pathToDevEnv = Path.Combine(visualizer.VsInstallDirectory, "devenv.exe");

            if (File.Exists(pathToDevEnv))
            {
                visualizer.VsExePath = pathToDevEnv;
                visualizer.VsSetupArgument = visualizer.RegistryKey?.GetValue("SetupCommandLine") as string ?? "/setup";
            }
        }

        private void Write(Visualizer visualizer)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(Path.GetDirectoryName(visualizer.InstallPath)))
            {
                return;
            }

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

            ResetVsExtensions(visualizer);
        }

        private static void ResetVsExtensions(Visualizer visualizer)
        {
            if (visualizer.VsExePath != null)
            {
                using (Process.Start(visualizer.VsExePath, visualizer.VsSetupArgument)) { }
            }
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
            DeletePreviousManifests(visualizer);

            // ReSharper disable PossibleNullReferenceException
            if (File.Exists(visualizer.VsixManifestPath))
            {
                var vsixManifestDirectory = GetVsixManifestDirectory(visualizer);
                var productDirectory = vsixManifestDirectory.Parent;
                var companyDirectory = productDirectory.Parent;

                vsixManifestDirectory.Delete(recursive: true);

                if (!productDirectory.GetDirectories().Any())
                {
                    productDirectory.Delete(recursive: true);

                    if (!companyDirectory.GetDirectories().Any())
                    {
                        companyDirectory.Delete(recursive: true);
                    }
                }

                ResetVsExtensions(visualizer);
            }
            // ReSharper restore PossibleNullReferenceException

            if (File.Exists(visualizer.InstallPath))
            {
                File.Delete(visualizer.InstallPath);
            }
        }

        private static DirectoryInfo GetVsixManifestDirectory(Visualizer visualizer)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return new DirectoryInfo(Path.GetDirectoryName(visualizer.VsixManifestPath));
        }

        private static void DeletePreviousManifests(Visualizer visualizer)
        {
            var productDirectory = GetVsixManifestDirectory(visualizer).Parent;

            if ((productDirectory == null) || !productDirectory.Exists)
            {
                return;
            }

            var thisVersion = new Version(_thisAssemblyVersion.FileVersion);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var versionDirectory in productDirectory.GetDirectories("*"))
            {
                Version directoryVersion;

                if (Version.TryParse(versionDirectory.Name, out directoryVersion))
                {
                    if (directoryVersion != thisVersion)
                    {
                        versionDirectory.Delete(recursive: true);
                    }
                }
            }
        }
    }
}
