#if NETFRAMEWORK
namespace AgileObjects.ReadableExpressions.Build.Compilation
{
    using System;
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CSharp;
    using NetStandardPolyfills;
    using SourceCode;
    using static BuildConstants;

    internal class NetFrameworkCompiler : ICompiler
    {
        public CompilationResult Compile(string inputFile)
        {
            var codeProvider = new CSharpCodeProvider();
            var parameters = new CompilerParameters { GenerateInMemory = true };

            var compilationResult = codeProvider
                .CompileAssemblyFromFile(parameters, inputFile);

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

            var sourceCodeExpression = GetSourceCodeExpressionOrThrow(compilationResult);

            return new CompilationResult { SourceCodeExpression = sourceCodeExpression };
        }

        private static SourceCodeExpression GetSourceCodeExpressionOrThrow(
            CompilerResults compilationResult)
        {
            var buildMethod = GetBuildMethodOrThrow(compilationResult);
            var buildMethodResult = buildMethod.Invoke(null, Array.Empty<object>());

            if (buildMethodResult == null)
            {
                throw new InvalidOperationException($"{InputClass}.{InputMethod} returned null");
            }

            return (SourceCodeExpression)buildMethodResult;
        }

        private static MethodInfo GetBuildMethodOrThrow(CompilerResults compilationResult)
        {
            var expressionBuilderType = GetBuilderTypeOrThrow(compilationResult);
            var buildMethod = expressionBuilderType.GetPublicStaticMethod(InputMethod);

            if (buildMethod == null)
            {
                throw new NotSupportedException(
                    $"Expected public, static method {InputClass}.{InputMethod} not found");
            }

            if (buildMethod.GetParameters().Any())
            {
                throw new NotSupportedException(
                    $"Expected method {InputClass}.{InputMethod} to be parameterless");
            }

            if (buildMethod.ReturnType != typeof(SourceCodeExpression))
            {
                throw new NotSupportedException(
                    $"Expected method {InputClass}.{InputMethod} to return {nameof(SourceCodeExpression)}");
            }

            return buildMethod;
        }

        private static Type GetBuilderTypeOrThrow(CompilerResults compilationResult)
        {
            var compiledAssembly = compilationResult.CompiledAssembly;

            var expressionBuilderType = compiledAssembly
                .GetType("ExpressionBuilder", throwOnError: false);

            if (expressionBuilderType == null)
            {
                throw new NotSupportedException(
                    "Expected Type ExpressionBuilder not found");
            }

            return expressionBuilderType;
        }
    }
}
#endif