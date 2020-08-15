#if NETFRAMEWORK
namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.CSharp;

    internal class NetFrameworkCompiler : ICompiler
    {
        public CompilationResult Compile(
            string expressionBuilderSource,
            ICollection<Type> referenceAssemblyTypes)
        {
            var codeProvider = new CSharpCodeProvider();

            var parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false
            };

            parameters.ReferencedAssemblies.AddRange(referenceAssemblyTypes
                .Select(GetAssemblyFileName).ToArray());

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

        private static string GetAssemblyFileName(Type type)
            => Path.GetFileName(type.Assembly.Location);
    }
}
#endif