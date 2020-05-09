namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal static class EmbeddedResourceExtensions
    {
        public static IEnumerable<string> WithExtension(this string[] resourceNames, string extension)
        {
            return resourceNames
                .Where(resourceName => Path.GetExtension(resourceName)?.ToLowerInvariant() == extension);
        }
    }
}