namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.StringComparison;
using static EmbeddedResourceExtensions;

internal class Visualizer
{
    private static readonly Assembly _thisAssembly = typeof(Visualizer).Assembly;

    private static readonly Regex _versionNumberMatcher =
        new Regex(@"Vs(?<VersionNumber>[\d\.]+)\.dll$", RegexOptions.IgnoreCase);

    public static readonly Dictionary<Version, int> VsYearByVersionNumber = new(6)
    {
        [new(10, 0)] = 2010,
        [new(11, 0)] = 2012,
        [new(12, 0)] = 2013,
        [new(14, 0)] = 2015,
        [new(15, 0)] = 2017,
        [new(16, 0)] = 2019,
        [new(17, 0)] = 2022,
        [new(17, 6)] = 2022
    };

    private readonly Action<string> _logger;
    private readonly CoreAssemblies _coreAssemblies;
    private readonly string _visualizerResourceFileName;
    private readonly string _objectSourceResourceName;
    private readonly string _objectSourceResourceFileName;

    public Visualizer(
        Action<string> logger,
        CoreAssemblies coreAssemblies,
        string visualizerResourceName)
    {
        _logger = logger;
        _coreAssemblies = coreAssemblies;
        VisualizerResourceName = visualizerResourceName;

        _visualizerResourceFileName = GetResourceFileName(visualizerResourceName);

        _objectSourceResourceName = visualizerResourceName.Replace(".dll", ".ObjectSource.dll");
        _objectSourceResourceFileName = GetResourceFileName(_objectSourceResourceName);

        var versionInfoMatch = _versionNumberMatcher.Match(visualizerResourceName);
        var vsVersionNumber = versionInfoMatch.Groups["VersionNumber"].Value;

        if (vsVersionNumber.IndexOf('.') == -1)
        {
            vsVersionNumber += ".0";
        }

        VsFullVersionNumber = vsVersionNumber;
        VsFullVersion = new(vsVersionNumber);
        VsYear = VsYearByVersionNumber[VsFullVersion];
    }

    public string VisualizerResourceName { get; }

    public int VsYear { get; }

    public string VsFullVersionNumber { get; }

    public Version VsFullVersion { get; }

    public void Install(string visualizersDirectory)
    {
        var visualizerInstallPath = GetVisualizerInstallPath(visualizersDirectory);

        Log("Writing visualizer to " + visualizerInstallPath);
        _thisAssembly.WriteFileFromResource(VisualizerResourceName, visualizerInstallPath);

        var objectSourceInstallPaths = GetObjectSourceInstallPaths(visualizersDirectory);

        foreach (var objectSourceInstallPath in objectSourceInstallPaths)
        {
            Log("Writing object source to " + objectSourceInstallPath);
            _coreAssemblies.Install(objectSourceInstallPath);
            _thisAssembly.WriteFileFromResource(_objectSourceResourceName, objectSourceInstallPath);
        }
    }

    private IEnumerable<string> GetObjectSourceInstallPaths(string visualizersDirectory)
    {
        return GetObjectSourceInstallDirectories(visualizersDirectory)
            .Select(GetObjectSourceInstallPath);
    }

    private IEnumerable<string> GetObjectSourceInstallDirectories(string visualizersDirectory)
    {
        if (InstallObjectSourceInVisualizersDirectory)
        {
            yield return visualizersDirectory;
            yield break;
        }

        var netStandardSubDirectories = Directory
            .EnumerateDirectories(visualizersDirectory)
            .Select(path => new DirectoryInfo(path))
            .Where(dir =>
                dir.Name.Equals("net2.0", OrdinalIgnoreCase) ||
                dir.Name.StartsWith("netstandard", OrdinalIgnoreCase));

        foreach (var netStandardSubDirectory in netStandardSubDirectories)
        {
            yield return netStandardSubDirectory.FullName;
        }
    }

    private bool InstallObjectSourceInVisualizersDirectory => VsYear < 2019;

    public void Uninstall(string visualizersDirectory)
    {
        var visualizerInstallPath = GetVisualizerInstallPath(visualizersDirectory);

        DeleteIfExists(visualizerInstallPath);

        var objectSourceInstallDirectories = GetObjectSourceInstallDirectories(visualizersDirectory);

        foreach (var objectSourceInstallDirectory in objectSourceInstallDirectories)
        {
            var objectSourceInstallPath = GetObjectSourceInstallPath(objectSourceInstallDirectory);
            DeleteIfExists(objectSourceInstallPath);

            var legacyVisualizerInstallPath = GetVisualizerInstallPath(objectSourceInstallDirectory);
            DeleteIfExists(legacyVisualizerInstallPath);

            _coreAssemblies.Uninstall(objectSourceInstallPath);
        }
    }

    private string GetVisualizerInstallPath(string visualizersDirectory)
        => Path.Combine(visualizersDirectory, _visualizerResourceFileName);

    private string GetObjectSourceInstallPath(string objectSourceDirectory)
        => Path.Combine(objectSourceDirectory, _objectSourceResourceFileName);

    private static void DeleteIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private void Log(string message) => _logger.Invoke(message);
}