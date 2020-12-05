namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System.Reflection;

    /// <summary>
    /// An <see cref="IField"/> describing a System.Reflection.FieldInfo.
    /// </summary>
    public class BclFieldWrapper : IField
    {
        private readonly FieldInfo _field;
        private IType _declaringType;
        private IType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclFieldWrapper"/> class.
        /// </summary>
        /// <param name="field">The FieldInfo to which the <see cref="BclFieldWrapper"/> relates.</param>
        public BclFieldWrapper(FieldInfo field)
        {
            _field = field;
        }

        /// <inheritdoc />
        public IType DeclaringType
            => _declaringType ??= BclTypeWrapper.For(_field.DeclaringType);

        /// <inheritdoc />
        public IType Type
            => _type ??= BclTypeWrapper.For(_field.FieldType);

        /// <inheritdoc />
        public string Name => _field.Name;

        /// <inheritdoc />
        public bool IsStatic => _field.IsStatic;

        /// <inheritdoc />
        public bool IsPublic => _field.IsPublic;

        /// <inheritdoc />
        public bool IsInternal => _field.IsAssembly;

        /// <inheritdoc />
        public bool IsProtectedInternal => _field.IsFamilyOrAssembly;

        /// <inheritdoc />
        public bool IsProtected => _field.IsFamily;

        /// <inheritdoc />
        public bool IsPrivateProtected => _field.IsFamilyAndAssembly;

        /// <inheritdoc />
        public bool IsPrivate => _field.IsPrivate;

        /// <inheritdoc />
        public bool IsConstant => _field.IsLiteral;

        /// <inheritdoc />
        public bool IsReadonly => IsConstant || _field.IsInitOnly;
    }
}