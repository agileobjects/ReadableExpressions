namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Deployment.WindowsInstaller;

    public class VisualizerInstallationActions
    {
        private static readonly Assembly _thisAssembly = typeof(Visualizer).Assembly;

        // ReSharper disable once AssignNullToNotNullAttribute
        private static readonly FileVersionInfo _thisAssemblyVersion = FileVersionInfo.GetVersionInfo(_thisAssembly.Location);

        private static readonly Lazy<string> _vsixManifestResourceNameLoader;
        private static readonly Lazy<string> _vsixManifestLoader;

        private static Session _session;
        private static string _resultMessage;

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
            Debugger.Break();
#endif
            _session = session;

            try
            {
                var installed = new List<string> { "Installed visualizers for:" };

                Log("Starting...");

                foreach (var visualizer in GetRelevantVisualizers())
                {
                    Log("Installing visualizer " + visualizer.ResourceName + "...");
                    visualizer.Uninstall();
                    visualizer.Install();

                    installed.Add("Visual Studio " + visualizer.VsFullVersionNumber);
                }

                _resultMessage = string.Join(Environment.NewLine, installed);

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

        [CustomAction]
        public static ActionResult SetResultMessage(Session session)
        {
            session["WIXUI_EXITDIALOGOPTIONALTEXT"] = _resultMessage;
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
                    .Select(visualizerResourceName => new Visualizer(Log, _thisAssemblyVersion, VsixManifest, visualizerResourceName))
                    .SelectMany(visualizer => registryData.GetInstallableVisualizersFor(visualizer))
                    .ToArray();
            }
        }

        private static void Log(string message) => _session?.Log(message);
    }
}
