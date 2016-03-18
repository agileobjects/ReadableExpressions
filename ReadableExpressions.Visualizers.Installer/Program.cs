namespace AgileObjects.ReadableExpressions.Visualizers.Installer
{
    using System;
    using System.IO;
    using System.Linq;

    public class Program
    {
        public static void Main(string[] args)
        {
            var currentDomainBaseDirectory = args.FirstOrDefault();

            var visualizer = VisualizerFinder.GetRelevantVisualizer(currentDomainBaseDirectory);

            if (visualizer == null)
            {
                return;
            }

            try
            {
                WriteVisualizerFile(visualizer);
            }
            catch (IOException ioEx)
            {
                throw new InvalidOperationException("Unable to write Visualizer assembly", ioEx);
            }

            var initializedFilePath = args.ElementAtOrDefault(1);

            if (initializedFilePath != null)
            {
                File.WriteAllText(initializedFilePath, $"Visualizer installed for VS{visualizer.VsVersionNumber}.");
            }
        }

        private static void WriteVisualizerFile(VisualizerFinder.Visualizer visualizer)
        {
            using (var resourceStream = typeof(Program).Assembly.GetManifestResourceStream(visualizer.ResourceName))
            using (var visualizerFileStream = File.OpenWrite(visualizer.InstallPath))
            {
                resourceStream.CopyTo(visualizerFileStream);
            }
        }
    }
}
