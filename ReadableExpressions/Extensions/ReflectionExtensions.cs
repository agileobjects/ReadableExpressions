namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class ReflectionExtensions
    {
        [DebuggerStepThrough]
        public static bool IsExtensionMethod(this MethodInfo method)
            => method.IsStatic && method.IsDefined(typeof(ExtensionAttribute), false);

        [DebuggerStepThrough]
        public static bool IsImplicitOperator(this MethodInfo method)
            => method.IsSpecialName && method.IsStatic && (method.Name == "op_Implicit");

        [DebuggerStepThrough]
        public static bool IsExplicitOperator(this MethodInfo method)
            => method.IsSpecialName && method.IsStatic && (method.Name == "op_Explicit");
    }
}