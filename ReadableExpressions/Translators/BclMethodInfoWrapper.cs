namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal class BclMethodInfoWrapper : IMethodInfo
    {
        private readonly MethodInfo _method;

        public BclMethodInfoWrapper(MethodInfo method)
        {
            _method = method;
        }

        public string Name => _method.Name;

        public bool IsGenericMethod => _method.IsGenericMethod;

        public MethodInfo GetGenericMethodDefinition() => _method.GetGenericMethodDefinition();

        public IEnumerable<Type> GetGenericArguments() => _method.GetGenericArguments();

        public IEnumerable<ParameterInfo> GetParameters() => _method.GetParameters();
    }
}