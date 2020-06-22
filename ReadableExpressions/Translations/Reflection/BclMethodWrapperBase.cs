namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    internal abstract class BclMethodWrapperBase
    {
        private readonly MethodBase _method;
        private Type[] _genericArguments;
        private IParameter[] _parameters;

        protected BclMethodWrapperBase(MethodBase method)
        {
            _method = method;
        }

        public Type DeclaringType => _method.DeclaringType;

        public bool IsPublic => _method.IsPublic;

        public bool IsProtectedInternal => _method.IsFamilyOrAssembly;

        public bool IsInternal => _method.IsAssembly;

        public bool IsProtected => _method.IsFamily;

        public bool IsPrivate => _method.IsPrivate;

        public bool IsAbstract => _method.IsAbstract;

        public bool IsStatic => _method.IsStatic;

        public bool IsVirtual => _method.IsVirtual;

        public string Name => _method.Name;

        public bool IsGenericMethod => _method.IsGenericMethod;

        public virtual Type[] GetGenericArguments()
            => _genericArguments ??= _method.GetGenericArguments();

        public IParameter[] GetParameters()
        {
            return _parameters ??= _method
                .GetParameters()
                .ProjectToArray(p => (IParameter)new BclParameterWrapper(p));
        }

        private class BclParameterWrapper : IParameter
        {
            private readonly ParameterInfo _parameter;

            public BclParameterWrapper(ParameterInfo parameter)
            {
                _parameter = parameter;
            }

            public Type Type => _parameter.ParameterType;

            public string Name => _parameter.Name;

            public bool IsOut => _parameter.IsOut;

            public bool IsParamsArray => _parameter.IsParamsArray();
        }
    }
}