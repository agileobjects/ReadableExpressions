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
    public class BclMethodWrapper : BclMethodWrapperBase
    {
        private readonly MethodInfo _method;
        private readonly TranslationSettings _settings;
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
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        [DebuggerStepThrough]
        public BclMethodWrapper(
            MethodInfo method,
            IList<Type> genericArguments,
            TranslationSettings settings)
            : this(method, settings)
        {
            _genericArguments = genericArguments
                .ProjectToArray(GenericArgumentFactory.For)
                .ToReadOnlyCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BclMethodWrapper"/> class for the given
        /// <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The MethodInfo to which the <see cref="BclMethodWrapper"/> relates.</param>
        /// <param name="context">The <see cref="ITranslationContext"/> describing the current translation.</param>
        [DebuggerStepThrough]
        public BclMethodWrapper(MethodInfo method, ITranslationContext context)
            : this(method, context.Settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BclMethodWrapper"/> class for the given
        /// <paramref name="method"/>.
        /// </summary>
        /// <param name="method">The MethodInfo to which the <see cref="BclMethodWrapper"/> relates.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        [DebuggerStepThrough]
        public BclMethodWrapper(MethodInfo method, TranslationSettings settings)
            : base(method)
        {
            _method = method;
            _settings = settings;
            IsExtensionMethod = method.IsExtensionMethod();
        }

        /// <inheritdoc />
        public override bool IsExtensionMethod { get; }

        /// <inheritdoc />
        public override IMethod GetGenericMethodDefinition()
        {
            return _genericMethodDefinition ??=
                   new BclMethodWrapper(_method.GetGenericMethodDefinition(), _settings);
        }

        /// <inheritdoc cref="IMethod.GetGenericArguments" />
        public override ReadOnlyCollection<IGenericArgument> GetGenericArguments()
            => _genericArguments ??= base.GetGenericArguments();

        /// <inheritdoc />
        public override Type ReturnType => _method.ReturnType;
    }
}