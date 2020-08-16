namespace MyNamespace
{
    using System;
    using System.Linq.Expressions;
    using AgileObjects.ReadableExpressions.Build;
    using AgileObjects.ReadableExpressions.Build.SourceCode;

    /// <summary>
    /// Supplies an input <see cref="SourceCodeExpression"/> to compile to source code when this
    /// project is built.
    /// </summary>
    public static class ExpressionBuilder
    {
        /// <summary>
        /// Builds the <see cref="SourceCodeExpression"/> to compile to a source code file when this
        /// project is built.
        /// </summary>
        /// <returns>The <see cref="SourceCodeExpression"/> to compile.</returns>
        public static SourceCodeExpression Build()
        {
            // Replace this code with your own, building a SourceCodeExpression
            // to be compiled to a source code file:
            var doNothing = Expression.Lambda<Action>(Expression.Default(typeof(void)));

            return ReadableSourceCodeExpression
                .SourceCode(sc => sc
                    .WithNamespaceOf(typeof(ExpressionBuilder))
                    .WithClass("MyClass", cls => cls
                        .WithMethod("DoNothing", doNothing)));
        }
    }
}
