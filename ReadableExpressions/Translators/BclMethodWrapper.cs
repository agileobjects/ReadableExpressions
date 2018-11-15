namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using NetStandardPolyfills;

    internal class BclMethodWrapper : IMethod
    {
        private readonly MethodInfo _method;
        private Type[] _genericArguments;

        [DebuggerStepThrough]
        public BclMethodWrapper(MethodInfo method, Type[] genericArguments = null)
        {
            _method = method;
            _genericArguments = genericArguments;
            IsExtensionMethod = method.IsExtensionMethod();
        }

        public string Name => _method.Name;

        public bool IsGenericMethod => _method.IsGenericMethod;

        public bool IsExtensionMethod { get; }

        public MethodInfo GetGenericMethodDefinition() => _method.GetGenericMethodDefinition();

        public Type[] GetGenericArguments() =>
            (_genericArguments ?? (_genericArguments = _method.GetGenericArguments()));

        public ParameterInfo[] GetParameters() => _method.GetParameters();

        public Type GetGenericArgumentFor(Type parameterType)
        {
            var parameterIndex = Array.IndexOf(_method.GetGenericArguments(), parameterType, 0);

            return _genericArguments[parameterIndex];
        }

        public Type ReturnType => _method.ReturnType;
    }
}