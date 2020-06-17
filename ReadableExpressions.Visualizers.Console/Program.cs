namespace ReadableExpressions.Visualizers.Console
{
    using System;
    using System.Linq.Expressions;

    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var def = Expression.Default(typeof(int));

            Console.WriteLine(def);

            //new VisualizerDialog(() => "<span class=\"kw\">default</span>(<span class=\"kw\">void</span>)").ShowDialog();
        }
    }
}
