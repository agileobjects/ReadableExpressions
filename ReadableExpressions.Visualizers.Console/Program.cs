namespace ReadableExpressions.Visualizers.Console
{
    using System;
    using AgileObjects.ReadableExpressions.Visualizers.Core;

    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new VisualizerDialog(() => "default(void)").ShowDialog();
        }
    }
}
