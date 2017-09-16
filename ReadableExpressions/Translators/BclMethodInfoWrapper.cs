namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Extensions;

    internal class BclMethodInfoWrapper : IMethodInfo
    {
        private readonly MethodInfo _method;
        private Type[] _genericArguments;

        [DebuggerStepThrough]
        public BclMethodInfoWrapper(MethodInfo method, Type[] genericArguments = null)
        {
            _method = method;
            _genericArguments = genericArguments;
            IsExtensionMethod = method.IsExtensionMethod();
        }

        public string Name => _method.Name;

        public bool IsGenericMethod => _method.IsGenericMethod;

        public bool IsExtensionMethod { get; }

        public MethodInfo GetGenericMethodDefinition() => _method.GetGenericMethodDefinition();

        public IEnumerable<Type> GetGenericArguments() => GetGenericArgumentsArray();

        private Type[] GetGenericArgumentsArray() =>
            (_genericArguments ?? (_genericArguments = _method.GetGenericArguments()));

        public IEnumerable<ParameterInfo> GetParameters() => _method.GetParameters();

        public Type GetGenericArgumentFor(Type parameterType)
        {
            var genericArguments = GetGenericArgumentsArray();
            var parameterIndex = Array.IndexOf(genericArguments, parameterType, 0);

            return genericArguments[parameterIndex];
        }
    }
}