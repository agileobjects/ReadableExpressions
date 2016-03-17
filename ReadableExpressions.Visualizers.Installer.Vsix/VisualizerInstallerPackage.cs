//------------------------------------------------------------------------------
// <copyright file="VisualizerInstallerPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Vsix
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VisualizerInstallerPackage : Package
    {
        /// <summary>
        /// VisualizerInstallerPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "1e6db509-e5af-4343-82cf-bb7f09a5e587";

        protected override void Initialize()
        {
            base.Initialize();

            var visualizers = VisualizerFinder.GetVisualizers();

            if (!visualizers.Any())
            {
                return;
            }

            var installerArgs = string.Join(
                " ",
                visualizers.SelectMany(v => new[] { v.ResourceName, $"\"{v.InstallPath}\"" }));

            var installerStartInfo = new ProcessStartInfo(typeof(Program).Assembly.Location)
            {
                Arguments = installerArgs,
                Verb = "runas"
            };

            Process.Start(installerStartInfo);
        }
    }
}
