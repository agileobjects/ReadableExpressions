namespace AgileObjects.ReadableExpressions.UnitTests.Build
{
#if FEATURE_COMPILATION
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
#endif

    public static class FluentAssertionExtensions
    {
        public static void ShouldCompile(this string sourceCode)
        {
#if FEATURE_COMPILATION

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var compilation = CSharpCompilation.Create(
                "ReadableExpressionsTestAssembly" + Guid.NewGuid(),
                new[] { syntaxTree },
                new[]
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Regex).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(FluentAssertionExtensions).Assembly.Location),
                },
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var dllStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(dllStream, pdbStream);

                if (emitResult.Success)
                {
                    return;
                }

                var errors = string.Join(
                    Environment.NewLine,
                    emitResult.Diagnostics.Select(d => d.ToString()));

                throw new NotSupportedException(errors);
            }
#endif
        }
    }
}
