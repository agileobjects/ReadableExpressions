// ReSharper disable once CheckNamespace
namespace ReBuild
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using AgileObjects.NetStandardPolyfills;
    using AgileObjects.ReadableExpressions;
    using AgileObjects.ReadableExpressions.Build;
    using AgileObjects.ReadableExpressions.Build.Compilation;
    using AgileObjects.ReadableExpressions.Build.Configuration;
    using AgileObjects.ReadableExpressions.Build.Io;
    using AgileObjects.ReadableExpressions.Build.Logging;
    using AgileObjects.ReadableExpressions.Build.SourceCode;
    using static AgileObjects.ReadableExpressions.Build.BuildConstants;
    using MsBuildTask = Microsoft.Build.Utilities.Task;

    /// <summary>
    /// An MSBuild Task to generate a source code file from a <see cref="SourceCodeExpression"/>.
    /// </summary>
    public class BuildExpressionsTask : MsBuildTask
    {
        private readonly ILogger _logger;
        private readonly IFileManager _fileManager;
        private readonly IConfigManager _configManager;
        private readonly ICompiler _compiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildExpressionsTask"/> class.
        /// </summary>
        public BuildExpressionsTask()
            : this(
                new MsBuildTaskLogger(),
                BclFileManager.Instance,
#if NETFRAMEWORK
                new NetFrameworkConfigManager(BclFileManager.Instance),
#else
                new NetStandardConfigManager(BclFileManager.Instance),
#endif
#if NETFRAMEWORK
                new NetFrameworkCompiler()
#else
                new NetStandardCompiler()
#endif
                )
        {
            ((MsBuildTaskLogger)_logger).SetTask(this);
        }

        internal BuildExpressionsTask(
            ILogger logger,
            IFileManager fileManager,
            IConfigManager configManager,
            ICompiler compiler)
        {
            _logger = logger;
            _fileManager = fileManager;
            _configManager = configManager;
            _compiler = compiler;
        }

        /// <summary>
        /// Gets or sets the root path of the project providing the <see cref="SourceCodeExpression"/>
        /// to build.
        /// </summary>
        public string ContentRoot { get; set; }

        /// <summary>
        /// Generates a source code file from a <see cref="SourceCodeExpression"/>.
        /// </summary>
        public override bool Execute()
        {
            try
            {
                var config = _configManager.GetConfigOrNull(ContentRoot) ?? new Config();

                if (string.IsNullOrEmpty(config.InputFile))
                {
                    config.InputFile = DefaultInputFile;
                }

                if (!_fileManager.Exists(config.InputFile))
                {
                    _logger.Warning($"Input file {config.InputFile} not found");
                    return false;
                }

                _logger.Info($"Using input file {config.InputFile}");

                if (string.IsNullOrEmpty(config.OutputFile))
                {
                    config.OutputFile = DefaultOutputFile;
                }

                _logger.Info($"Using output file {config.OutputFile}");

                var expressionBuilderSource = _fileManager.Read(config.InputFile);
                var referenceAssemblyTypes = GetReferenceAssemblyTypes(expressionBuilderSource);

                var compilationResult = _compiler
                    .Compile(expressionBuilderSource, referenceAssemblyTypes);

                if (compilationResult.Failed)
                {
                    _logger.Error("Expression compilation failed:");

                    foreach (var error in compilationResult.Errors)
                    {
                        _logger.Error(error);
                    }

                    return false;
                }

                _logger.Info("Expression compilation succeeded");

                var sourceCodeExpression = GetSourceCodeExpressionOrThrow(compilationResult);

                _fileManager.Write(
                    Path.Combine(ContentRoot, config.OutputFile),
                    sourceCodeExpression.ToSourceCode());

                _logger.Info("Expression compilation output updated");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        private static ICollection<Type> GetReferenceAssemblyTypes(string expressionBuilderSource)
        {
            var referenceAssemblyTypes = new List<Type>
            {
                typeof(object),
                typeof(AssemblyExtensionsPolyfill),
                typeof(ReadableExpression),
                typeof(ReadableSourceCodeExpression)
            };

            if (expressionBuilderSource.Contains("using System.Linq"))
            {
                referenceAssemblyTypes.Add(typeof(Enumerable));
            }

            return referenceAssemblyTypes;
        }

        private static SourceCodeExpression GetSourceCodeExpressionOrThrow(
            CompilationResult compilationResult)
        {
            var builderType = GetBuilderTypeOrThrow(compilationResult);
            var buildMethod = GetBuildMethodOrThrow(builderType);
            var buildMethodResult = buildMethod.Invoke(null, Array.Empty<object>());

            if (buildMethodResult == null)
            {
                throw new InvalidOperationException($"{InputClass}.{InputMethod} returned null");
            }

            return (SourceCodeExpression)buildMethodResult;
        }

        private static Type GetBuilderTypeOrThrow(CompilationResult compilationResult)
        {
            var builderTypes = compilationResult
                .CompiledAssembly
                .GetTypes()
                .Where(t => t.Name == InputClass)
                .ToList();

            if (builderTypes.Count == 0)
            {
                throw new NotSupportedException($"Expected input Type {InputClass} not found");
            }

            if (builderTypes.Count > 1)
            {
                throw new NotSupportedException($"Multiple {InputClass} Types found");
            }

            return builderTypes[0];
        }

        private static MethodInfo GetBuildMethodOrThrow(Type builderType)
        {
            var buildMethod = builderType.GetPublicStaticMethod(InputMethod);

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
    }
}
