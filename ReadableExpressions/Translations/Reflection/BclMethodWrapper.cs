namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using NetStandardPolyfills;

    /// <summary>
    /// An <see cref="IMethod"/> describing a System.Reflection.MethodInfo.
    /// </summary>
    public class BclMethodWrapper : BclMethodWrapperBase, IMethod
    {
        private readonly MethodInfo _method;
        private IMethod _genericMethodDefinition;
        private Type[] _genericArguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclMethodWrapper"/> class for the given
        /// <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The MethodInfo to which the <see cref="BclMethodWrapper"/> relates.</param>
        /// <param name="genericArguments">
        /// The Types of the <paramref name="method"/>'s generic arguments, or null or an empty array
        /// if the method is non-generic or the collection should be populated by the constructor.
        /// </param>
        [DebuggerStepThrough]
        public BclMethodWrapper(MethodInfo method, Type[] genericArguments = null)
            : base(method)
        {
            _method = method;
            _genericArguments = genericArguments;
            IsExtensionMethod = method.IsExtensionMethod();
        }

        /// <inheritdoc />
        public bool IsExtensionMethod { get; }

        /// <inheritdoc />
        public IMethod GetGenericMethodDefinition()
            => _genericMethodDefinition ??= new BclMethodWrapper(_method.GetGenericMethodDefinition());

        /// <inheritdoc cref="IMethod.GetGenericArguments" />
        public override Type[] GetGenericArguments()
            => _genericArguments ??= base.GetGenericArguments();

        /// <inheritdoc />
        public Type ReturnType => _method.ReturnType;
    }
}