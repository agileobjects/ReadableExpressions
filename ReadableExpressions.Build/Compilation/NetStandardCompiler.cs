#if NET_STANDARD
namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using NetStandardPolyfills;
    using static Microsoft.CodeAnalysis.OutputKind;

    internal class NetStandardCompiler : ICompiler
    {
        public CompilationResult Compile(
            string expressionBuilderSource,
            ICollection<Type> referenceAssemblyTypes)
        {
            var sourceTree = SyntaxFactory.ParseSyntaxTree(expressionBuilderSource);

            using var outputStream = new MemoryStream();

            var compilationResult = CSharpCompilation
                .Create("ExpressionBuildOutput_" + Path.GetFileNameWithoutExtension(Path.GetTempFileName()))
                .WithOptions(new CSharpCompilationOptions(DynamicallyLinkedLibrary))
                .AddReferences(CreateReferences(referenceAssemblyTypes))
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

            outputStream.Position = 0;

            var compiledAssembly = AssemblyLoadContext.Default.LoadFromStream(outputStream);

            return new CompilationResult { CompiledAssembly = compiledAssembly };
        }

        private static IEnumerable<MetadataReference> CreateReferences(
            ICollection<Type> passedInTypes)
        {
            var referencedAssemblyTypes = new List<Type>(passedInTypes);

            if (!passedInTypes.Contains(typeof(object)))
            {
                referencedAssemblyTypes.Add(typeof(object));
            }

            if (!passedInTypes.Contains(typeof(List<>)))
            {
                referencedAssemblyTypes.Add(typeof(List<>));
            }

            var assemblies = new List<Assembly>();
            CollectAssemblies(referencedAssemblyTypes, assemblies);

            return assemblies.Select(CreateReference);
        }

        private static void CollectAssemblies(
            IEnumerable<Type> assemblyTypes,
            ICollection<Assembly> assemblies)
        {
            foreach (var assemblyType in assemblyTypes.Distinct())
            {
                CollectAssemblies(assemblyType.GetAssembly(), assemblies);
            }
        }

        private static void CollectAssemblies(Assembly assembly, ICollection<Assembly> assemblies)
        {
            if (assemblies.Contains(assembly))
            {
                return;
            }

            assemblies.Add(assembly);

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                CollectAssemblies(Assembly.Load(referencedAssembly), assemblies);
            }
        }

        private static MetadataReference CreateReference(Assembly assembly)
            => MetadataReference.CreateFromFile(assembly.Location);
    }
}
#endif