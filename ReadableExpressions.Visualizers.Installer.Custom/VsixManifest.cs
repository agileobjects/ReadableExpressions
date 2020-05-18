namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Core;

    internal class VsixManifest
    {
        private static readonly Assembly _thisAssembly = typeof(VisualizerAssembly).Assembly;

        private readonly Lazy<string> _resourceNameLoader;
        private readonly Lazy<string> _contentLoader;

        public VsixManifest()
        {
            _resourceNameLoader = new Lazy<string>(GetResourceName);
            _contentLoader = new Lazy<string>(GetContent);
        }

        #region Setup

        private static string GetResourceName()
        {
            return _thisAssembly
                .GetManifestResourceNames()
                .WithExtension(".vsixmanifest")
                .First();
        }

        private string GetContent()
        {
            using (var resourceStream = _thisAssembly.GetManifestResourceStream(ResourceName))
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var streamReader = new StreamReader(resourceStream))
            {
                return streamReader
                    .ReadToEnd()
                    .Replace("$version$", VersionNumber.FileVersion)
                    .Replace("$author$", VersionNumber.CompanyName);
            }
        }

        #endregion

        private string ResourceName => _resourceNameLoader.Value;

        public string Content => _contentLoader.Value;
    }
}