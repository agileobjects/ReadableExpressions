namespace ReadableExpressions.Visualizers.Console.NetCore
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
        }
    }
}
