#if NET_STANDARD
namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System.IO;
    using System.Linq;
    using System.Runtime.Loader;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NetStandardPolyfills;
    using SourceCode;
    using static Microsoft.CodeAnalysis.OutputKind;

    internal class NetStandardCompiler : ICompiler
    {
        public CompilationResult Compile(string expressionBuilderSource)
        {
            var sourceTree = SyntaxFactory.ParseSyntaxTree(expressionBuilderSource);

            using var outputStream = new MemoryStream();

            var compilationResult = CSharpCompilation
                .Create("ExpressionBuildOutput.dll")
                .WithOptions(new CSharpCompilationOptions(DynamicallyLinkedLibrary))
                .AddReferences(
                    CreateReference<object>(),
                    CreateReference<SourceCodeExpression>())
                .AddSyntaxTrees(sourceTree)
                .Emit(outputStream);

            if (!compilationResult.Success)
            {
                return new CompilationResult
                {
                    Errors = compilationResult
                        .Diagnostics
                        .Select(ce => $"Error ({ce.Id}): {ce.GetMessage()}, Line codeIssue.Location.GetLineSpan()")
                        .ToList()
                };
            }

            var compiledAssembly = AssemblyLoadContext.Default.LoadFromStream(outputStream);

            return new CompilationResult { CompiledAssembly = compiledAssembly };
        }

        private static MetadataReference CreateReference<T>()
        {
            var assemblyLocation = typeof(T).GetAssembly().Location;
            var assemblyReference = MetadataReference.CreateFromFile(assemblyLocation);

            return assemblyReference;
        }
    }
}
#endif