namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using static System.StringComparison;

internal class VisualizerInstallerFactory : IDisposable
{
    private readonly Action<string> _logger;
    private readonly VsixManifest _vsixManifest;
    private readonly RegistryKey _msMachineKey;
    private readonly RegistryKey _pre2017VsMachineKey;
    private readonly string[] _pre2017VsKeyNames;
    private readonly Post2015VsInstallation[] _post2015VsInstallations;

    public VisualizerInstallerFactory(
        Action<string> logger, 
        VsixManifest vsixManifest)
    {
        _logger = logger;
        _vsixManifest = vsixManifest;
        const string REGISTRY_KEY = @"SOFTWARE\Microsoft";

        _msMachineKey = Registry.LocalMachine.OpenSubKey(REGISTRY_KEY);

        if (_msMachineKey == null)
        {
            ErrorMessage = $"Unable to open the '{REGISTRY_KEY}' registry key";
            NoVisualStudio = true;
            return;
        }

        var vsKeyNames = _msMachineKey
            .GetSubKeyNames()
            .Where(sk => sk.StartsWith("VisualStudio", Ordinal))
            .ToArray();

        if (vsKeyNames.Length == 0)
        {
            ErrorMessage = $@"Unable to find any '{REGISTRY_KEY}\VisualStudio' registry keys from which to determine your Visual Studio install paths";
            NoVisualStudio = true;
            return;
        }

        _pre2017VsMachineKey = _msMachineKey.OpenSubKey("VisualStudio");
        _pre2017VsKeyNames = _pre2017VsMachineKey?.GetSubKeyNames() ?? new string[0];

        _post2015VsInstallations = vsKeyNames
            .Where(kn => kn.StartsWith("VisualStudio_", OrdinalIgnoreCase))
            .Select(kn => new Post2015VsInstallation(_msMachineKey.OpenSubKey(kn)))
            .Where(inst => inst.IsValid)
            .GroupBy(inst => inst.InstallDirectory)
            .Select(grp => grp.First())
            .ToArray();
    }

    public bool NoVisualStudio { get; }

    public string ErrorMessage { get; }

    public IEnumerable<VisualizerInstaller> GetInstallersFor(Visualizer visualizer)
    {
        return TryPopulateInstallers(visualizer, out var visualizerInstallers)
            ? visualizerInstallers
            : Enumerable.Empty<VisualizerInstaller>();
    }

    private bool TryPopulateInstallers(
        Visualizer visualizer,
        out ICollection<VisualizerInstaller> installers)
    {
        installers = new List<VisualizerInstaller>();

        PopulatePre2017InstallPath(visualizer, installers);
        PopulatePost2015InstallPaths(visualizer, installers);

        return installers.Any();
    }

    private void PopulatePre2017InstallPath(
        Visualizer visualizer,
        ICollection<VisualizerInstaller> installers)
    {
        if (visualizer.VsYear >= 2017)
        {
            return;
        }

        var pre2017Key = _pre2017VsKeyNames
            .FirstOrDefault(name => name == visualizer.VsFullVersionNumber);

        if (pre2017Key == null)
        {
            return;
        }

        using var vsSubKey = _pre2017VsMachineKey.OpenSubKey(pre2017Key);

        var installDirectory = vsSubKey?.GetValue("InstallDir") as string;

        if (string.IsNullOrWhiteSpace(installDirectory) ||
           !Directory.Exists(installDirectory))
        {
            return;
        }

        installers.Add(CreateInstaller(vsSubKey, installDirectory, visualizer));
    }

    private void PopulatePost2015InstallPaths(
        Visualizer visualizer,
        ICollection<VisualizerInstaller> installers)
    {
        if (visualizer.VsYear <= 2015)
        {
            return;
        }

        foreach (var installation in _post2015VsInstallations)
        {
            if (installation.ShouldInstall(visualizer.VsFullVersion))
            {
                installers.Add(CreateInstaller(
                    installation.RegistryKey,
                    installation.InstallDirectory,
                    visualizer));
            }
        }
    }

    private VisualizerInstaller CreateInstaller(
        RegistryKey registryKey,
        string vsInstallDirectory,
        Visualizer visualizer)
    {
        return new VisualizerInstaller(
            _logger,
            _vsixManifest,
            registryKey,
            vsInstallDirectory,
            visualizer);
    }

    public void Dispose()
    {
        _msMachineKey?.Dispose();
        _pre2017VsMachineKey?.Dispose();

        foreach (var vsPost2015DataItem in _post2015VsInstallations)
        {
            vsPost2015DataItem.Dispose();
        }
    }
}