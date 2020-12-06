namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using Extensions;

    /// <summary>
    /// An <see cref="IConstructor"/> describing a System.Reflection.ConstructorInfo.
    /// </summary>
    public class BclCtorInfoWrapper : BclMethodWrapperBase, IConstructor
    {
        private readonly ConstructorInfo _ctorInfo;
        private IType _declaringType;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclCtorInfoWrapper"/> class for the given
        /// <paramref name="ctorInfo"/>.
        /// </summary>
        /// <param name="ctorInfo">
        /// The ConstructorInfo to which the <see cref="BclCtorInfoWrapper"/> relates.
        /// </param>
        [DebuggerStepThrough]
        public BclCtorInfoWrapper(ConstructorInfo ctorInfo)
            : base(ctorInfo)
        {
            _ctorInfo = ctorInfo;
        }

        /// <inheritdoc />
        public override bool IsExtensionMethod => false;

        /// <inheritdoc />
        public override IMethod GetGenericMethodDefinition() => null;

        /// <inheritdoc />
        public override IType ReturnType
            => _declaringType ??= BclTypeWrapper.For(_ctorInfo.DeclaringType);

        /// <inheritdoc />
        public override ReadOnlyCollection<IGenericParameter> GetGenericArguments()
            => Enumerable<IGenericParameter>.EmptyReadOnlyCollection;
    }
}