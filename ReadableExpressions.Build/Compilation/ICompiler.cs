namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    internal interface ICompiler
    {
        CompilationResult Compile(string expressionBuilderSource);
    }
}
