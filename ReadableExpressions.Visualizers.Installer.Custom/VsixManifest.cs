namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Core;

    internal class VsixManifest
    {
        private static readonly Assembly _thisAssembly = typeof(Visualizer).Assembly;

        private readonly Lazy<string> _resourceNameLoader;
        private readonly Lazy<string> _contentLoader;

        public VsixManifest()
        {
            _resourceNameLoader = new Lazy<string>(GetResourceName);
            _contentLoader = new Lazy<string>(GetContent);
        }

        #region Setup

        private static string GetResourceName()
            => _thisAssembly.GetManifestResourceNamesOfType(".vsixmanifest").First();

        private string GetContent()
        {
            return _thisAssembly
                .GetResourceContent(ResourceName)
                .Replace("$version$", VersionNumber.FileVersion)
                .Replace("$author$", VersionNumber.CompanyName);
        }

        #endregion

        private string ResourceName => _resourceNameLoader.Value;

        public string Content => _contentLoader.Value;
    }
}