namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Deployment.WindowsInstaller;

    public class VisualizerInstallationActions
    {
        private static readonly Assembly _thisAssembly = typeof(Visualizer).Assembly;

        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly FileVersionInfo _thisAssemblyVersion = FileVersionInfo.GetVersionInfo(_thisAssembly.Location);

        private static readonly Lazy<string> _vsixManifestResourceNameLoader;
        private static readonly Lazy<string> _vsixManifestLoader;

        private static Session _session;

        static VisualizerInstallationActions()
        {
            _vsixManifestResourceNameLoader = new Lazy<string>(GetVsixManifestResourceName);
            _vsixManifestLoader = new Lazy<string>(GetVsixManifest);
        }

        #region Setup

        private static string GetVsixManifestResourceName()
        {
            return _thisAssembly
                .GetManifestResourceNames()
                .WithExtension("vsixmanifest")
                .First();
        }

        private static string GetVsixManifest()
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

        #endregion

        private static string VsixManifestResourceName => _vsixManifestResourceNameLoader.Value;

        private static string VsixManifest => _vsixManifestLoader.Value;

        [CustomAction]
        public static ActionResult Install(Session session)
        {
#if DEBUG
            Debugger.Launch();
#endif
            _session = session;

            try
            {
                Log("Starting...");

                foreach (var visualizer in GetRelevantVisualizers())
                {
                    Log("Installing visualizer " + visualizer.ResourceName + "...");
                    Delete(visualizer);
                    Write(visualizer);
                }

                Log("Complete");


                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                return ActionResult.Failure;
            }
            finally
            {
                _session = null;
            }
        }

        private static void Log(string message) => _session?.Log(message);

        private static IEnumerable<Visualizer> GetRelevantVisualizers()
        {
            using (var registryData = new RegistryData())
            {
                if (registryData.NoVisualStudio)
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
                    .SelectMany(visualizer => PopulateInstallPaths(visualizer, registryData))
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

        private static IEnumerable<Visualizer> PopulateInstallPaths(Visualizer visualizer, RegistryData registryData)
        {
            if (NoInstallPathsFound(visualizer, registryData, out ICollection<Visualizer> targetVisualizers))
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

        private static bool NoInstallPathsFound(
            Visualizer visualizer,
            RegistryData registryData,
            out ICollection<Visualizer> targetVisualizers)
        {
            targetVisualizers = new List<Visualizer>();

            PopulatePre2017InstallPath(visualizer, registryData, targetVisualizers);
            TryGetPost2015InstallPaths(visualizer, registryData, targetVisualizers);

            return targetVisualizers.Count == 0;
        }

        private static void PopulatePre2017InstallPath(
            Visualizer visualizer,
            RegistryData registryData,
            ICollection<Visualizer> targetVisualizers)
        {
            var pre2017Key = registryData
                .VsPre2017KeyNames
                .FirstOrDefault(name => name == visualizer.VsFullVersionNumber);

            if (pre2017Key == null)
            {
                return;
            }

            var vsSubKey = registryData.VsPre2017MachineKey.OpenSubKey(pre2017Key);
            var installPath = vsSubKey?.GetValue("InstallDir") as string;

            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
            {
                return;
            }

            targetVisualizers.Add(visualizer.With(vsSubKey, installPath));
        }

        private static void TryGetPost2015InstallPaths(
            Visualizer visualizer,
            RegistryData registryData,
            ICollection<Visualizer> targetVisualizers)
        {
            var relevantDataItems = registryData
                .VsPost2015Data
                .Where(d => d.IsValid && (d.VsFullVersionNumber == visualizer.VsFullVersionNumber));

            foreach (var dataItem in relevantDataItems)
            {
                targetVisualizers.Add(visualizer.With(dataItem.RegistryKey, dataItem.InstallPath));
            }
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

        private static void Write(Visualizer visualizer)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!Directory.Exists(Path.GetDirectoryName(visualizer.InstallPath)))
            {
                Log("Skipping as directory does not exist: " + visualizer.InstallPath);
                return;
            }

            using (var resourceStream = _thisAssembly.GetManifestResourceStream(visualizer.ResourceName))
            using (var visualizerFileStream = File.OpenWrite(visualizer.InstallPath))
            {
                Log("Writing visualizer to " + visualizer.InstallPath);
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(visualizerFileStream);
            }

            var manifestDirectory = Path.GetDirectoryName(visualizer.VsixManifestPath);

            Log("Writing manifest to " + visualizer.VsixManifestPath);
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(manifestDirectory);
            File.WriteAllText(visualizer.VsixManifestPath, VsixManifest, Encoding.ASCII);

            ResetVsExtensions(visualizer);
        }

        private static void ResetVsExtensions(Visualizer visualizer)
        {
            if (visualizer.VsExePath != null)
            {
                Log("Updating VS extension records using " + visualizer.VsExePath);
                using (Process.Start(visualizer.VsExePath, visualizer.VsSetupArgument)) { }
            }
        }

        [CustomAction]
        public static ActionResult Uninstall(Session session)
        {
            try
            {
                foreach (var visualizer in GetRelevantVisualizers())
                {
                    Delete(visualizer);
                }
            }
            catch
            {
                return ActionResult.Failure;
            }

            return ActionResult.Success;
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
                        Log("Deleting previous manifest version directory " + versionDirectory.FullName);
                        versionDirectory.Delete(recursive: true);
                    }
                }
            }
        }
    }
}
