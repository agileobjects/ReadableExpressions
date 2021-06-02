namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    /// <summary>
    /// Helper base class for <see cref="IMethod"/> implementations which wrap MethodBase-derived
    /// objects.
    /// </summary>
    public abstract class ClrMethodWrapperBase : IMethod
    {
        private readonly MethodBase _method;
        private IType _declaringType;
        private ReadOnlyCollection<IGenericParameter> _genericArguments;
        private ReadOnlyCollection<IParameter> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClrMethodWrapperBase"/> class.
        /// </summary>
        /// <param name="method">The MethodBase to which the <see cref="ClrMethodWrapperBase"/> relates.</param>
        protected ClrMethodWrapperBase(MethodBase method)
        {
            _method = method;
        }

        /// <inheritdoc />
        public IType DeclaringType
            => _declaringType ??= ClrTypeWrapper.For(_method.DeclaringType);

        /// <inheritdoc />
        public bool IsPublic => _method.IsPublic;

        /// <inheritdoc />
        public bool IsInternal => _method.IsAssembly;

        /// <inheritdoc />
        public bool IsProtectedInternal => _method.IsFamilyOrAssembly;

        /// <inheritdoc />
        public bool IsProtected => _method.IsFamily;

        /// <inheritdoc />
        public bool IsPrivateProtected => _method.IsFamilyAndAssembly;

        /// <inheritdoc />
        public bool IsPrivate => _method.IsPrivate;

        /// <inheritdoc />
        public bool IsAbstract => _method.IsAbstract;

        /// <inheritdoc />
        public bool IsStatic => _method.IsStatic;

        /// <inheritdoc />
        public bool IsVirtual => !IsAbstract && _method.IsVirtual;

        /// <inheritdoc />
        public bool IsOverride => this.IsOverride();

        /// <inheritdoc />
        public string Name => _method.Name;

        /// <inheritdoc />
        public bool IsGenericMethod => _method.IsGenericMethod;

        /// <inheritdoc />
        public abstract bool IsExtensionMethod { get; }

        /// <inheritdoc />
        public bool HasBody => !IsAbstract;

        /// <inheritdoc />
        public abstract IMethod GetGenericMethodDefinition();

        /// <inheritdoc />
        public virtual ReadOnlyCollection<IGenericParameter> GetGenericArguments()
            => _genericArguments ??= _method.GetGenericArgs();

        /// <inheritdoc />
        public ReadOnlyCollection<IParameter> GetParameters()
        {
            return _parameters ??= _method
                .GetParameters()
                .ProjectToArray<ParameterInfo, IParameter>(p => new ClrParameterWrapper(p))
                .ToReadOnlyCollection();
        }

        IType IMember.Type => ReturnType;

        /// <inheritdoc />
        public abstract IType ReturnType { get; }

        private class ClrParameterWrapper : IParameter
        {
            private readonly ParameterInfo _parameter;
            private IType _type;

            public ClrParameterWrapper(ParameterInfo parameter)
            {
                _parameter = parameter;
            }

            public IType Type
                => _type ??= ClrTypeWrapper.For(ParameterType);

            private Type ParameterType => _parameter.ParameterType;

            public string Name => _parameter.Name;

            public bool IsOut => _parameter.IsOut;

            public bool IsRef => !IsOut && ParameterType.IsByRef;

            public bool IsParamsArray => _parameter.IsParamsArray();
        }
    }
}