namespace AgileObjects.ReadableExpressions.Build
{
    internal static class BuildConstants
    {
        public const string InputFileKey = "ReBuildInput";
        public const string OutputFileKey = "ReBuildOutput";

        public const string InputClass = "ExpressionBuilder";
        public const string InputMethod = "Build";

        public const string DefaultInputFile = InputClass + ".cs";
        public const string DefaultOutputFile = InputClass + "Output.cs";
    }
}
