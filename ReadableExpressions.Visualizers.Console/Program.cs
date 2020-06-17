namespace ReadableExpressions.Visualizers.Console
{
    using System;
    using System.Linq.Expressions;
    using AgileObjects.ReadableExpressions.Visualizers.Dialog;
    using WpfVisualizerDialog = AgileObjects.ReadableExpressions.Visualizers.Dialog.Wpf.VisualizerDialog;

    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var def = Expression.Default(typeof(int));

            Console.WriteLine(def);

            new WpfVisualizerDialog(() => "<span class=\"kw\">default</span>(<span class=\"kw\">void</span>)").ShowDialog();
            //new VisualizerDialog(() => "<span class=\"kw\">default</span>(<span class=\"kw\">void</span>)").ShowDialog();
        }
    }
}
