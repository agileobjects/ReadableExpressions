namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
{
    using System;
    using System.IO;
    using System.Linq;
#if FEATURE_COMPILATION
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
#endif

    public static class FluentAssertionExtensions
    {
        public static void ShouldBeCompilableMethod(this string methodCode) 
            => ShouldBeCompilableClass("public class GeneratedExpressionClass { " + methodCode + " }");

        public static void ShouldBeCompilableClass(this string classCode) 
            => ShouldCompile("namespace GeneratedExpressionCode { " + classCode + " }");

        public static void ShouldCompile(this string sourceCode)
        {
#if FEATURE_COMPILATION

            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

            var compilation = CSharpCompilation.Create(
                "ReadableExpressionsTestAssembly" + Guid.NewGuid(),
                new[] { syntaxTree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
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
                    emitResult.Diagnostics.Select(d => d.Descriptor.Description));

                throw new NotSupportedException(errors);
            }
#endif
        }
    }
}
