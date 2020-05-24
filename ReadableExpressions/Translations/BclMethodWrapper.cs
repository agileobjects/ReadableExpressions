namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Interfaces;
    using NetStandardPolyfills;

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

    internal class CtorInfoWrapper : BclMethodWrapperBase, IMethod
    {
        private readonly ConstructorInfo _ctorInfo;

        public CtorInfoWrapper(ConstructorInfo ctorInfo)
            : base(ctorInfo)
        {
            _ctorInfo = ctorInfo;
        }

        public bool IsExtensionMethod => false;

        public IMethod GetGenericMethodDefinition() => null;

        public Type ReturnType => _ctorInfo.DeclaringType;
    }

    internal class BclMethodWrapper : BclMethodWrapperBase, IMethod
    {
        private readonly MethodInfo _method;
        private IMethod _genericMethodDefinition;
        private Type[] _genericArguments;

        [DebuggerStepThrough]
        public BclMethodWrapper(MethodInfo method, Type[] genericArguments = null)
            : base(method)
        {
            _method = method;
            _genericArguments = genericArguments;
            IsExtensionMethod = method.IsExtensionMethod();
        }

        public bool IsExtensionMethod { get; }

        public IMethod GetGenericMethodDefinition()
            => _genericMethodDefinition ??= new BclMethodWrapper(_method.GetGenericMethodDefinition());

        public override Type[] GetGenericArguments()
            => _genericArguments ??= base.GetGenericArguments();

        public Type ReturnType => _method.ReturnType;
    }
}