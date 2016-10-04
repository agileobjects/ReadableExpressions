namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Reflection;

    internal static class ReflectionExtensionsPolyFill
    {
        public static bool IsParamsArray(this ParameterInfo parameter)
        {
#if NET_STANDARD
            return parameter.GetCustomAttribute<ParamArrayAttribute>(inherit: false) != null;
#else
            return Attribute.IsDefined(parameter, typeof(ParamArrayAttribute));
#endif
        }
    }
}
