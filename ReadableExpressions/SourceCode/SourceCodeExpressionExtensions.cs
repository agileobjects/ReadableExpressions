namespace AgileObjects.ReadableExpressions.SourceCode
{
    using Api;
    using Extensions;

    internal static class SourceCodeExpressionExtensions
    {
        private const string _defaultName = "GeneratedExpressionClass";

        public static string GetClassName(
            this SourceCodeExpression sourceCodeExpression,
            IClassNamingContext classCtx)
        {
            if (sourceCodeExpression == null)
            {
                return _defaultName;
            }

            var sourceCodeClasses = sourceCodeExpression.Classes;
            var @class = (ClassExpression)classCtx;

            if (sourceCodeClasses.Count == 1)
            {
                return GetName(@class);
            }

            var classIndex = sourceCodeClasses.IndexOf(@class);

            return GetName(@class) + (classIndex + 1);
        }

        private static string GetName(ClassExpression @class)
        {
            if (@class.Interfaces.Count != 1)
            {
                return _defaultName;
            }

            var interfaceName = @class.Interfaces[0].GetFriendlyName();
            interfaceName = interfaceName.Substring(interfaceName.LastIndexOf('.') + 1);

            if (interfaceName[0] == 'I' &&
                interfaceName.Length > 1 &&
                char.IsUpper(interfaceName[1]))
            {
                return interfaceName.Substring(1);
            }

            return interfaceName;
        }
    }
}