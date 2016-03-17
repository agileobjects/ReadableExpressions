namespace AgileObjects.ReadableExpressions.Visualizers.Installer
{
    using System;
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            for (var i = 0; i < args.Length; i += 2)
            {
                try
                {
                    var visualizer = new VisualizerFinder.Visualizer
                    {
                        ResourceName = args[i],
                        InstallPath = args[i + 1]
                    };

                    WriteVisualizerFile(visualizer);
                }
                catch (IOException ioEx)
                {
                    throw new InvalidOperationException("Unable to write Visualizer assembly", ioEx);
                }
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
