namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using static System.StringComparison;

    internal class CoreAssemblies
    {
        private static readonly Assembly _thisAssembly = typeof(CoreAssemblies).Assembly;

        private static readonly string[] _coreAssemblyNames =
        {
            "AgileObjects.NetStandardPolyfills.dll",
            "AgileObjects.ReadableExpressions.dll"
        };

        private readonly Lazy<IEnumerable<CoreAssembly>> _coreAssembliesLoader;

        public CoreAssemblies()
        {
            _coreAssembliesLoader = new Lazy<IEnumerable<CoreAssembly>>(GetCoreAssemblies);
        }

        #region Setup

        private static IEnumerable<CoreAssembly> GetCoreAssemblies()
        {
            var assemblyResourceNames = _thisAssembly
                .GetManifestResourceNamesOfType(".dll")
                .ToArray();

            return _coreAssemblyNames
                .Select(assemblyName => new CoreAssembly
                {
                    ResourceName = assemblyResourceNames.First(resourceName =>
                        resourceName.EndsWith(assemblyName, Ordinal)),

                    FileName = assemblyName
                })
                .ToArray();
        }

        #endregion

        public static bool IsCoreAssembly(string resourceName)
            => _coreAssemblyNames.Any(resourceName.Contains);

        private IEnumerable<CoreAssembly> Assemblies => _coreAssembliesLoader.Value;

        public void Install(string objectSourceInstallPath)
        {
            var objectSourceInstallDirectory = Path.GetDirectoryName(objectSourceInstallPath);

            foreach (var assembly in Assemblies)
            {
                _thisAssembly.WriteFileFromResource(
                    assembly.ResourceName,
                    Path.Combine(objectSourceInstallDirectory, assembly.FileName));
            }
        }

        public void Uninstall(string objectSourceInstallPath)
        {
            var objectSourceInstallDirectory = Path.GetDirectoryName(objectSourceInstallPath);

            foreach (var assembly in Assemblies)
            {
                var installPath = Path.Combine(
                    objectSourceInstallDirectory,
                    assembly.FileName);

                if (File.Exists(installPath))
                {
                    File.Delete(installPath);
                }
            }
        }

        private class CoreAssembly
        {
            public string ResourceName { get; set; }

            public string FileName { get; set; }
        }
    }
}