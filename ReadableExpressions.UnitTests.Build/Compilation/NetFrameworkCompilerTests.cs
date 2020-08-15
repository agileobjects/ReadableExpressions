﻿namespace AgileObjects.ReadableExpressions.UnitTests.Build.Compilation
{
    using ReadableExpressions.Build.Compilation;

    public class NetFrameworkCompilerTests : CompilerTestsBase
    {
        internal override ICompiler CreateCompiler()
            => new NetFrameworkCompiler();
    }
}
