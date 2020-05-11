namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
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
                .WithExtension(".vsixmanifest")
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

                if (NoVisualizersToInstall(out var visualizers, out var errorMessage))
                {
                    MessageBox.Show(
                        errorMessage,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                var installed = new List<string> { "Installed visualizers for:" };

                foreach (var visualizer in visualizers)
                {
                    Log("Installing visualizer " + visualizer.ResourceName + "...");
                    visualizer.Uninstall();
                    visualizer.Install();

                    installed.Add(" - Visual Studio " + visualizer.VsFullVersionNumber);
                }

                MessageBox.Show(
                    string.Join(Environment.NewLine, installed),
                    "Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

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
#if DEBUG
            Debugger.Launch();
#endif
            try
            {
                if (TryGetVisualizers(out var visualizers))
                {
                    foreach (var visualizer in visualizers)
                    {
                        visualizer.Uninstall();
                    }
                }
            }
            catch
            {
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        private static bool TryGetVisualizers(out IEnumerable<Visualizer> visualizers)
            => NoVisualizersToInstall(out visualizers, out _) != true;

        private static bool NoVisualizersToInstall(out IEnumerable<Visualizer> visualizers, out string errorMessage)
        {
            using (var registryData = new RegistryData(_thisAssemblyVersion))
            {
                if (registryData.NoVisualStudio)
                {
                    visualizers = Enumerable.Empty<Visualizer>();
                    errorMessage = registryData.ErrorMessage;
                    return true;
                }

                visualizers = _thisAssembly
                    .GetManifestResourceNames()
                    .WithExtension(".dll")
                    .Select(visualizerResourceName => new Visualizer(Log, _thisAssemblyVersion, VsixManifest, visualizerResourceName))
                    .SelectMany(visualizer => registryData.GetInstallableVisualizersFor(visualizer))
                    .ToArray();

                errorMessage = null;
                return false;
            }
        }

        private static void Log(string message) => _session?.Log(message);
    }
}
