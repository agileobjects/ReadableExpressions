namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.Win32;

    internal class RegistryData : IDisposable
    {
        private readonly FileVersionInfo _thisAssemblyVersion;

        public RegistryData(FileVersionInfo thisAssemblyVersion)
        {
            _thisAssemblyVersion = thisAssemblyVersion;
            const string REGISTRY_KEY = @"SOFTWARE\Microsoft";

            MsMachineKey = Registry.LocalMachine.OpenSubKey(REGISTRY_KEY);

            if (MsMachineKey == null)
            {
                ErrorMessage = $"Unable to open the '{REGISTRY_KEY}' registry key";
                NoVisualStudio = true;
                return;
            }

            var vsKeyNames = MsMachineKey
                .GetSubKeyNames()
                .Where(sk => sk.StartsWith("VisualStudio", StringComparison.Ordinal))
                .ToArray();

            if (vsKeyNames.Length == 0)
            {
                ErrorMessage = $@"Unable to find any '{REGISTRY_KEY}\VisualStudio' registry keys from which to determine your Visual Studio install paths";
                NoVisualStudio = true;
                return;
            }

            VsPre2017MachineKey = MsMachineKey.OpenSubKey("VisualStudio");
            VsPre2017KeyNames = VsPre2017MachineKey?.GetSubKeyNames() ?? new string[0];

            VsPost2015Data = vsKeyNames
                .Where(kn => kn.StartsWith("VisualStudio_"))
                .Select(kn => new VsPost2017Data(MsMachineKey.OpenSubKey(kn)))
                .GroupBy(d => d.InstallPath)
                .Select(grp => grp.First())
                .ToArray();
        }

        public RegistryKey MsMachineKey { get; }

        public RegistryKey VsPre2017MachineKey { get; }

        public string[] VsPre2017KeyNames { get; }

        public VsPost2017Data[] VsPost2015Data { get; }

        public bool NoVisualStudio { get; }

        public string ErrorMessage { get; private set; }

        public IEnumerable<Visualizer> GetInstallableVisualizersFor(Visualizer visualizer)
        {
            if (!TryPopulateInstallPaths(visualizer, out var targetVisualizers))
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
                    var pathToExtensions = GetPathToExtensions(vsInstallPath);

                    targetVisualizer.SetInstallPath(pathToVisualizers);
                    targetVisualizer.SetVsixManifestPath(pathToExtensions);
                    targetVisualizer.PopulateVsSetupData();

                    yield return targetVisualizer;
                }
            }
        }

        private bool TryPopulateInstallPaths(
            Visualizer visualizer,
            out ICollection<Visualizer> targetVisualizers)
        {
            targetVisualizers = new List<Visualizer>();

            PopulatePre2017InstallPath(visualizer, targetVisualizers);
            PopulatePost2015InstallPaths(visualizer, targetVisualizers);

            return targetVisualizers.Count > 0;
        }

        private void PopulatePre2017InstallPath(
            Visualizer visualizer,
            ICollection<Visualizer> targetVisualizers)
        {
            var pre2017Key = VsPre2017KeyNames
                .FirstOrDefault(name => name == visualizer.VsFullVersionNumber);

            if (pre2017Key == null)
            {
                return;
            }

            var vsSubKey = VsPre2017MachineKey.OpenSubKey(pre2017Key);
            var installPath = vsSubKey?.GetValue("InstallDir") as string;

            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
            {
                return;
            }

            targetVisualizers.Add(visualizer.With(vsSubKey, installPath));
        }

        private void PopulatePost2015InstallPaths(
            Visualizer visualizer,
            ICollection<Visualizer> targetVisualizers)
        {
            var relevantDataItems = VsPost2015Data
                .Where(d => d.IsValid && (d.VsFullVersionNumber == visualizer.VsFullVersionNumber));

            foreach (var dataItem in relevantDataItems)
            {
                targetVisualizers.Add(visualizer.With(dataItem.RegistryKey, dataItem.InstallPath));
            }
        }

        private string GetPathToExtensions(string vsInstallPath)
        {
            return Path.Combine(
                vsInstallPath,
                "Extensions",
                _thisAssemblyVersion.CompanyName,
                _thisAssemblyVersion.ProductName,
                _thisAssemblyVersion.FileVersion);
        }

        public void Dispose()
        {
            MsMachineKey?.Dispose();
            VsPre2017MachineKey?.Dispose();

            foreach (var vsPost2015DataItem in VsPost2015Data)
            {
                vsPost2015DataItem.Dispose();
            }
        }
    }
}