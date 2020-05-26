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

        public string Name => _method.Name;

        public bool IsGenericMethod => _method.IsGenericMethod;

        public virtual Type[] GetGenericArguments()
            => _genericArguments ??= _method.GetGenericArguments();

        public ParameterInfo[] GetParameters() => _method.GetParameters();
    }
}