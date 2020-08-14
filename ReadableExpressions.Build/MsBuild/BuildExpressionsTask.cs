// ReSharper disable once CheckNamespace
namespace ReBuild
{
    using System;
    using AgileObjects.ReadableExpressions.Build.Compilation;
    using AgileObjects.ReadableExpressions.Build.Configuration;
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
        private readonly IConfigManager _configManager;
        private readonly ICompiler _compiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildExpressionsTask"/> class.
        /// </summary>
        public BuildExpressionsTask()
            : this(
                new MsBuildTaskLogger(),
#if NETFRAMEWORK
                new NetFrameworkConfigManager(),
#else
                new NetStandardConfigManager(),
#endif
#if NETFRAMEWORK
                new NetFrameworkCompiler()
#else
                null
#endif
                )
        {
            ((MsBuildTaskLogger)_logger).SetTask(this);
        }

        internal BuildExpressionsTask(
            ILogger logger,
            IConfigManager configManager,
            ICompiler compiler)
        {
            _logger = logger;
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
                var config = _configManager
                    .GetConfigOrNull(ContentRoot, out var configFile);

                if (config == null)
                {
                    _logger.Info($"Config file '{_configManager.ConfigFileName}' could not be found");
                    return true;
                }

                if (config.Empty)
                {
                    _configManager.SetDefaults(configFile);
                    config.InputFile = DefaultInputFile;
                    config.OutputFile = DefaultOutputFile;
                }

                var compilationResult = _compiler.Compile(config.InputFile);

                if (compilationResult.Failed)
                {
                    _logger.Error("Expression compilation failed:");

                    foreach (var error in compilationResult.Errors)
                    {
                        _logger.Error(error);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }
    }
}
