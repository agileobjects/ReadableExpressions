namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions;

    internal class BclMethodInfoWrapper : IMethodInfo
    {
        private readonly MethodInfo _method;

        public BclMethodInfoWrapper(MethodInfo method)
        {
            _method = method;
            IsExtensionMethod = method.IsExtensionMethod();
        }

        public string Name => _method.Name;

        public bool IsGenericMethod => _method.IsGenericMethod;

        public bool IsExtensionMethod { get; }

        public MethodInfo GetGenericMethodDefinition() => _method.GetGenericMethodDefinition();

        public IEnumerable<Type> GetGenericArguments() => _method.GetGenericArguments();

        public IEnumerable<ParameterInfo> GetParameters() => _method.GetParameters();
    }
}