namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Msi.Custom
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using Microsoft.Win32;

    [RunInstaller(true)]
    public partial class VisualizerInstaller : Installer
    {
        private static readonly Assembly _thisAssembly = typeof(VisualizerInstaller).Assembly;

        public VisualizerInstaller()
        {
            InitializeComponent();
        }

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
            return _thisAssembly
                .GetManifestResourceNames()
                .Select(visualizerResourceName => new Visualizer
                {
                    ResourceName = visualizerResourceName,
                    VsVersionNumber = GetVsVersionNumber(visualizerResourceName)
                })
                .Where(TryPopulateInstallPath);
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

        private static bool TryPopulateInstallPath(Visualizer visualizer)
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
                var resourceAssemblyNameLength = (typeof(Visualizer).Namespace?.Length + 1).GetValueOrDefault();
                var visualizerAssemblyName = visualizer.ResourceName.Substring(resourceAssemblyNameLength);

                visualizer.InstallPath = Path.Combine(pathToVisualizers, visualizerAssemblyName);
                return true;
            }
        }

        private static void Write(Visualizer visualizer)
        {
            using (var resourceStream = _thisAssembly.GetManifestResourceStream(visualizer.ResourceName))
            using (var visualizerFileStream = File.OpenWrite(visualizer.InstallPath))
            {
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(visualizerFileStream);
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
            File.Delete(visualizer.InstallPath);
        }
    }
}
