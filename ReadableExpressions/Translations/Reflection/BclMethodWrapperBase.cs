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
    public abstract class BclMethodWrapperBase : IMethod
    {
        private readonly MethodBase _method;
        private ReadOnlyCollection<IGenericArgument> _genericArguments;
        private ReadOnlyCollection<IParameter> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclMethodWrapperBase"/> class.
        /// </summary>
        /// <param name="method">The MethodBase to which the <see cref="BclMethodWrapperBase"/> relates.</param>
        protected BclMethodWrapperBase(MethodBase method)
        {
            _method = method;
        }

        /// <inheritdoc />
        public Type DeclaringType => _method.DeclaringType;

        /// <inheritdoc />
        public bool IsPublic => _method.IsPublic;

        /// <inheritdoc />
        public bool IsProtectedInternal => _method.IsFamilyOrAssembly;

        /// <inheritdoc />
        public bool IsInternal => _method.IsAssembly;

        /// <inheritdoc />
        public bool IsProtected => _method.IsFamily;

        /// <inheritdoc />
        public bool IsPrivate => _method.IsPrivate;

        /// <inheritdoc />
        public bool IsAbstract => _method.IsAbstract;

        /// <inheritdoc />
        public bool IsStatic => _method.IsStatic;

        /// <inheritdoc />
        public bool IsVirtual => _method.IsVirtual;

        /// <inheritdoc />
        public bool IsOverride => this.IsOverride();

        /// <inheritdoc />
        public string Name => _method.Name;

        /// <inheritdoc />
        public bool IsGenericMethod => _method.IsGenericMethod;

        /// <inheritdoc />
        public abstract bool IsExtensionMethod { get; }

        /// <inheritdoc />
        public abstract IMethod GetGenericMethodDefinition();

        /// <inheritdoc />
        public virtual ReadOnlyCollection<IGenericArgument> GetGenericArguments()
            => _genericArguments ??= _method.GetGenericArgs();

        /// <inheritdoc />
        public ReadOnlyCollection<IParameter> GetParameters()
        {
            return _parameters ??= _method
                .GetParameters()
                .ProjectToArray<ParameterInfo, IParameter>(p => new BclParameterWrapper(p))
                .ToReadOnlyCollection();
        }

        Type IMember.Type => ReturnType;

        /// <inheritdoc />
        public abstract Type ReturnType { get; }

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