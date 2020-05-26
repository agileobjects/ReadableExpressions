namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using NetStandardPolyfills;

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