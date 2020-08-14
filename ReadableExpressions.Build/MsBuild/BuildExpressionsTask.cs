// ReSharper disable once CheckNamespace
namespace ReBuild
{
    using AgileObjects.ReadableExpressions.Build.Configuration;
    using AgileObjects.ReadableExpressions.Build.SourceCode;
    using MsBuildTask = Microsoft.Build.Utilities.Task;

    /// <summary>
    /// An MSBuild Task to generate a source code file from a <see cref="SourceCodeExpression"/>.
    /// </summary>
    public class BuildExpressionsTask : MsBuildTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildExpressionsTask"/> class.
        /// </summary>
        public BuildExpressionsTask()
            : this(
#if NETFRAMEWORK
                new NetFrameworkConfigManager()
#else
                new NetStandardConfigManager()
#endif
                )
        {
        }

        internal BuildExpressionsTask(IConfigManager configManager)
        {
            ConfigManager = configManager;
        }

        /// <summary>
        /// Gets or sets the root path of the project providing the <see cref="SourceCodeExpression"/>
        /// to build.
        /// </summary>
        public string ContentRoot { get; set; }

        internal IConfigManager ConfigManager { get; }

        /// <summary>
        /// Generates a source code file from a <see cref="SourceCodeExpression"/>.
        /// </summary>
        public override bool Execute()
        {
            try
            {
                var config = ConfigManager.GetConfigOrNull(ContentRoot);

                if (config == null)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
