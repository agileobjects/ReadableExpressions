namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;

internal class Post2015VsInstallation : IDisposable
{
    private static readonly Dictionary<int, Version> _vsVersionsByYear = Visualizer
        .VsYearByVersionNumber
        .Select(kvp => new { Version = kvp.Key, Year = kvp.Value })
        .Where(_ => _.Year > 2015)
        .GroupBy(_ => _.Year)
        .ToDictionary(yearGroup => yearGroup.Key, yearGroup => yearGroup
            .Select(_ => _.Version)
            .OrderBy(version => version)
            .Last());

    private readonly Version _vsVersion;
    private bool _hasVisualizer;

    public Post2015VsInstallation(RegistryKey post2015Key)
    {
        RegistryKey = post2015Key;

        using (var capabilitiesKey = post2015Key.OpenSubKey("Capabilities"))
        {
            InstallDirectory = GetInstallPath(capabilitiesKey);
            _vsVersion = GetVsFullVersion(capabilitiesKey, InstallDirectory);
        }

        IsValid = _vsVersion != null && InstallDirectory != null;
    }

    #region Setup

    private static string GetInstallPath(RegistryKey capabilitiesKey)
    {
        var installPath = capabilitiesKey.GetValue("ApplicationDescription") as string;

        if (string.IsNullOrWhiteSpace(installPath))
        {
            return null;
        }

        var indexOfIde = installPath.IndexOf("IDE", StringComparison.OrdinalIgnoreCase);

        if (indexOfIde == -1)
        {
            return null;
        }

        installPath = installPath.Substring(0, indexOfIde + "IDE".Length);

        if (installPath.StartsWith("@", StringComparison.Ordinal))
        {
            installPath = installPath.Substring(1);
        }

        return Directory.Exists(installPath) ? installPath : null;
    }

    private static Version GetVsFullVersion(
        RegistryKey capabilitiesKey,
        string installDirectory)
    {
        return
            GetVsFullVersion(
                installDirectory,
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) ??
            GetVsFullVersion(
                capabilitiesKey.GetValue("ApplicationName") as string,
                ' ');
    }

    private static Version GetVsFullVersion(
        string installPath,
        params char[] pathSplitCharacters)
    {
        if (string.IsNullOrWhiteSpace(installPath))
        {
            return null;
        }

        if (TryGetDevEnvVersion(installPath, out var version))
        {
            return version;
        }

        var vsYearNumber = installPath
            .Trim()
            .Split(pathSplitCharacters)
            .Reverse()
            .Select(segment => int.TryParse(segment, out var yearNumber) ? yearNumber : default(int?))
            .FirstOrDefault(yearNumber =>
                yearNumber.HasValue &&
               _vsVersionsByYear.ContainsKey(yearNumber.Value));

        return vsYearNumber.HasValue
            ? _vsVersionsByYear[vsYearNumber.Value]
            : null;
    }

    private static bool TryGetDevEnvVersion(
        string installPath,
        out Version vsVersion)
    {
        try
        {
            var devEnvFileInfo =
                new FileInfo(Path.Combine(installPath, "devenv.exe"));

            if (!devEnvFileInfo.Exists)
            {
                vsVersion = null;
                return false;
            }

            var versionInfo = FileVersionInfo
                .GetVersionInfo(devEnvFileInfo.FullName);

            vsVersion = new(versionInfo.FileMajorPart, versionInfo.FileMinorPart);
            return true;
        }
        catch
        {
            vsVersion = null;
            return false;
        }
    }

    #endregion

    public bool IsValid { get; }

    public RegistryKey RegistryKey { get; }

    public string InstallDirectory { get; }

    public bool ShouldInstall(Version visualizerVsVersion)
    {
        if (_hasVisualizer)
        {
            return false;
        }

        if (_vsVersion.Major == visualizerVsVersion.Major &&
            _vsVersion.Minor >= visualizerVsVersion.Minor)
        {
            _hasVisualizer = true;
            return true;
        }

        return false;
    }

    public void Dispose() => RegistryKey.Dispose();
}