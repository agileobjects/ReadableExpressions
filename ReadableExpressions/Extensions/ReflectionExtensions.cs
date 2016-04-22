namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class ReflectionExtensions
    {
        public static bool IsExtensionMethod(this MethodInfo method)
        {
            return method.IsStatic && method.IsDefined(typeof(ExtensionAttribute), false);
        }
    }
}