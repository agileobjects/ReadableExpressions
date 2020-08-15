namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System;
    using System.Collections.Generic;

    internal interface ICompiler
    {
        CompilationResult Compile(
            string expressionBuilderSource,
            ICollection<Type> referenceAssemblyTypes);
    }
}
