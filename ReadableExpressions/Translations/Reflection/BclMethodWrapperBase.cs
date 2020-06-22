namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Reflection;

    internal abstract class BclMethodWrapperBase
    {
        private readonly MethodBase _method;
        private Type[] _genericArguments;

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

        public ParameterInfo[] GetParameters() => _method.GetParameters();
    }
}