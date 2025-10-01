namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class InstallationExtensions
    {
        private static readonly int _resourceAssemblyNameLength =
            (typeof(Visualizer).Namespace?.Length + 1).GetValueOrDefault();

        private static readonly string _ideDirectory =
            Path.DirectorySeparatorChar +
            "IDE" +
            Path.DirectorySeparatorChar;

        public static IEnumerable<string> GetManifestResourceNamesOfType(
            this Assembly assembly,
            string extension)
        {
            return assembly
                .GetManifestResourceNames()
                .Where(resourceName => Path.GetExtension(resourceName).ToLowerInvariant() == extension);
        }

        public static void WriteFileFromResource(
            this Assembly assembly,
            string resourceName,
            string filePath)
        {
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            using (var objectSourceFileStream = File.OpenWrite(filePath))
            {
                // ReSharper disable once PossibleNullReferenceException
                resourceStream.CopyTo(objectSourceFileStream);
            }
        }

        public static string GetResourceContent(
            this Assembly assembly,
            string resourceName)
        {
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static string GetResourceFileName(string resourceName)
            => resourceName.Substring(_resourceAssemblyNameLength);

        public static int StartIndexOfIde(this string installPath) =>
            installPath.IndexOf(_ideDirectory, StringComparison.OrdinalIgnoreCase);

        public static int EndIndexOfIde(this string installPath)
        {
            var startIndex = installPath.StartIndexOfIde();

            return startIndex != -1
                ? startIndex + _ideDirectory.Length
                : -1;
        }
    }
}