namespace AgileObjects.ReadableExpressions.Visualizers.Installer.Custom
{
    using System;
    using System.Linq;
    using Microsoft.Win32;

    internal class RegistryData : IDisposable
    {
        public RegistryData()
        {
            const string REGISTRY_KEY = @"SOFTWARE\Microsoft";

            MsMachineKey = Registry.LocalMachine.OpenSubKey(REGISTRY_KEY);

            if (MsMachineKey == null)
            {
                NoVisualStudio = true;
                return;
            }

            var vsKeyNames = MsMachineKey
                .GetSubKeyNames()
                .Where(sk => sk.StartsWith("VisualStudio", StringComparison.Ordinal))
                .ToArray();

            if (vsKeyNames.Length == 0)
            {
                NoVisualStudio = true;
                return;
            }

            VsPre2017MachineKey = MsMachineKey.OpenSubKey("VisualStudio");
            VsPre2017KeyNames = VsPre2017MachineKey?.GetSubKeyNames() ?? new string[0];

            VsPost2015Data = vsKeyNames
                .Where(kn => kn.StartsWith("VisualStudio_"))
                .Select(kn => new VsPost2017Data(MsMachineKey.OpenSubKey(kn)))
                .ToArray();
        }

        public RegistryKey MsMachineKey { get; }

        public RegistryKey VsPre2017MachineKey { get; }

        public string[] VsPre2017KeyNames { get; }

        public VsPost2017Data[] VsPost2015Data { get; }

        public bool NoVisualStudio { get; }

        public void Dispose()
        {
            MsMachineKey?.Dispose();
            VsPre2017MachineKey?.Dispose();

            foreach (var vsPost2015DataItem in VsPost2015Data)
            {
                vsPost2015DataItem.Dispose();
            }
        }
    }
}