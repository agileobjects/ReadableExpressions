namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System.Collections.Generic;
    using SourceCode;

    internal class CompilationResult
    {
        public bool Failed => SourceCodeExpression == null;

        public SourceCodeExpression SourceCodeExpression { get; set; }

        public ICollection<string> Errors { get; set; }
    }
}