namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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
                    visualizer.Uninstall();
                    visualizer.Install();
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

        [CustomAction]
        public static ActionResult Uninstall(Session session)
        {
            try
            {
                foreach (var visualizer in GetRelevantVisualizers())
                {
                    visualizer.Uninstall();
                }
            }
            catch
            {
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        private static IEnumerable<Visualizer> GetRelevantVisualizers()
        {
            using (var registryData = new RegistryData(_thisAssemblyVersion))
            {
                if (registryData.NoVisualStudio)
                {
                    return Enumerable.Empty<Visualizer>();
                }

                return _thisAssembly
                    .GetManifestResourceNames()
                    .WithExtension("dll")
                    .Select(visualizerResourceName => new Visualizer(Log, _thisAssemblyVersion, VsixManifest)
                    {
                        ResourceName = visualizerResourceName,
                        VsVersionNumber = GetVsVersionNumber(visualizerResourceName)
                    })
                    .SelectMany(visualizer => registryData.GetInstallableVisualizersFor(visualizer))
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

        private static void Log(string message) => _session?.Log(message);
    }
}
