namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    /// <summary>
    /// An <see cref="IMethod"/> describing a System.Reflection.MethodInfo.
    /// </summary>
    public class BclMethodWrapper : BclMethodWrapperBase, IMethod
    {
        private readonly MethodInfo _method;
        private IMethod _genericMethodDefinition;
        private ReadOnlyCollection<IGenericArgument> _genericArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclMethodWrapper"/> class for the given
        /// <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The MethodInfo to which the <see cref="BclMethodWrapper"/> relates.</param>
        /// <param name="genericArguments">
        /// The Types of the <paramref name="method"/>'s generic arguments, if any.
        /// </param>
        [DebuggerStepThrough]
        public BclMethodWrapper(MethodInfo method, IList<Type> genericArguments)
            : this(method)
        {
            _genericArguments = genericArguments
                .ProjectToArray(GenericArgument.For)
                .ToReadOnlyCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BclMethodWrapper"/> class for the given
        /// <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The MethodInfo to which the <see cref="BclMethodWrapper"/> relates.</param>
        [DebuggerStepThrough]
        public BclMethodWrapper(MethodInfo method)
            : base(method)
        {
            _method = method;
            IsExtensionMethod = method.IsExtensionMethod();
        }

        /// <inheritdoc />
        public bool IsExtensionMethod { get; }

        /// <inheritdoc />
        public IMethod GetGenericMethodDefinition()
            => _genericMethodDefinition ??= new BclMethodWrapper(_method.GetGenericMethodDefinition());

        /// <inheritdoc cref="IMethod.GetGenericArguments" />
        public override ReadOnlyCollection<IGenericArgument> GetGenericArguments()
            => _genericArguments ??= base.GetGenericArguments();

        /// <inheritdoc />
        public Type ReturnType => _method.ReturnType;
    }
}