namespace AgileObjects.ReadableExpressions.UnitTests.Build.Compilation
{
    using ReadableExpressions.Build.Compilation;

    public class NetStandardCompilerTests : CompilerTestsBase
    {
        internal override ICompiler CreateCompiler()
            => new NetStandardCompiler();
    }
}
