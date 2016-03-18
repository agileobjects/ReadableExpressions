namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Vsix
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.ExtensionManager;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VisualizerInstallerPackage : Package
    {
        public const string PackageGuidString = "1e6db509-e5af-4343-82cf-bb7f09a5e587";

        private const string ExtensionId =
            "AgileObjects.ReadableExpressions.Visualizers.e8573a3a-c522-4287-9858-6812fbd24e86";

        protected override void Initialize()
        {
            base.Initialize();

            string initializedFilePath;

            if (VisualizerAlreadyInstalled(out initializedFilePath))
            {
                return;
            }

            var pathToInstallerExe = typeof(Program).Assembly.Location;

            var currentDomainBaseDirectory = GetCurrentDomainBaseDirectory();
            var installerArguments = $"\"{currentDomainBaseDirectory}\" \"{initializedFilePath}\"";

            var installerStartInfo = new ProcessStartInfo(pathToInstallerExe)
            {
                Arguments = installerArguments,
                Verb = "runas"
            };

            Process.Start(installerStartInfo);
        }

        private bool VisualizerAlreadyInstalled(out string initializedFilePath)
        {
            var extensionManager = GetService(typeof(SVsExtensionManager)) as IVsExtensionManager;

            if (extensionManager != null)
            {
                IInstalledExtension extension;

                if (extensionManager.TryGetInstalledExtension(ExtensionId, out extension))
                {
                    initializedFilePath = Path.Combine(extension.InstallPath, "initialized.txt");

                    return File.Exists(initializedFilePath);
                }
            }

            initializedFilePath = null;
            return false;
        }

        private static string GetCurrentDomainBaseDirectory()
        {
            var currentDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var finalCharacter = currentDomainBaseDirectory.Last();

            if ((finalCharacter == Path.DirectorySeparatorChar) ||
                (finalCharacter == Path.AltDirectorySeparatorChar))
            {
                currentDomainBaseDirectory = currentDomainBaseDirectory
                    .Substring(0, currentDomainBaseDirectory.Length - 1);
            }

            return currentDomainBaseDirectory;
        }
    }
}
