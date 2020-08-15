#if NETFRAMEWORK
namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System.CodeDom.Compiler;
    using System.Linq;
    using Microsoft.CSharp;

    internal class NetFrameworkCompiler : ICompiler
    {
        public CompilationResult Compile(string expressionBuilderSource)
        {
            var codeProvider = new CSharpCodeProvider();
            var parameters = new CompilerParameters { GenerateInMemory = true };

            var compilationResult = codeProvider
                .CompileAssemblyFromSource(parameters, expressionBuilderSource);

            if (compilationResult.Errors.HasErrors)
            {
                return new CompilationResult
                {
                    Errors = compilationResult
                        .Errors
                        .Cast<CompilerError>()
                        .Select(ce => $"Error ({ce.ErrorNumber}): {ce.ErrorText}")
                        .ToList()
                };
            }

            return new CompilationResult
            {
                CompiledAssembly = compilationResult.CompiledAssembly
            };
        }
    }
}
#endif