namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System.Collections.Generic;
    using System.Reflection;

    internal class CompilationResult
    {
        public bool Failed => CompiledAssembly == null;

        public Assembly CompiledAssembly { get; set; }

        public ICollection<string> Errors { get; set; }
    }
}