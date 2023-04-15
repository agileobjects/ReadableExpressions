namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using Microsoft.Deployment.WindowsInstaller;

    public class VisualizerInstallationActions
    {
        private static readonly Assembly _thisAssembly = typeof(Visualizer).Assembly;

        private static readonly Lazy<CoreAssemblies> _coreAssembliesLoader;
        private static readonly Lazy<VsixManifest> _vsixManifestLoader;

        private static Session _session;

        static VisualizerInstallationActions()
        {
            _coreAssembliesLoader = new Lazy<CoreAssemblies>(() => new CoreAssemblies());
            _vsixManifestLoader = new Lazy<VsixManifest>(() => new VsixManifest());
        }

        private static CoreAssemblies CoreAssemblies => _coreAssembliesLoader.Value;

        private static VsixManifest VsixManifest => _vsixManifestLoader.Value;

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

                if (NoVisualizersToInstall(out var installers, out var errorMessage))
                {
                    MessageBox.Show(
                        errorMessage,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                var installed = new List<string> { "Installed visualizers for:" };

                foreach (var installer in installers)
                {
                    if (installer.Install())
                    {
                        installed.Add(" - Visual Studio " + installer.VsId);
                    }
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
                if (TryGetVisualizers(out var installers))
                {
                    foreach (var installer in installers)
                    {
                        installer.Uninstall();
                    }
                }
            }
            catch
            {
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        private static bool TryGetVisualizers(out IEnumerable<VisualizerInstaller> installers)
            => NoVisualizersToInstall(out installers, out _) != true;

        private static bool NoVisualizersToInstall(
            out IEnumerable<VisualizerInstaller> installers,
            out string errorMessage)
        {
            using (var installerFactory = new VisualizerInstallerFactory(Log, VsixManifest))
            {
                if (installerFactory.NoVisualStudio)
                {
                    installers = Enumerable.Empty<VisualizerInstaller>();
                    errorMessage = installerFactory.ErrorMessage;
                    return true;
                }

                installers = _thisAssembly
                    .GetManifestResourceNamesOfType(".dll")
                    .Where(IsVisualizerAssembly)
                    .Select(visualizerResourceName => new Visualizer(Log, CoreAssemblies, visualizerResourceName))
                    .SelectMany(visualizer => installerFactory.GetInstallersFor(visualizer))
                    .ToArray();

                errorMessage = null;
                return false;
            }
        }

        private static bool IsVisualizerAssembly(string assemblyResourceName)
        {
            return assemblyResourceName.Contains(nameof(Visualizers)) &&
                  !assemblyResourceName.Contains("ObjectSource") &&
                  !CoreAssemblies.IsCoreAssembly(assemblyResourceName);
        }

        private static void Log(string message) => _session?.Log(message);
    }
}
