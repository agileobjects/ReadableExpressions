namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using Extensions;
    using NetStandardPolyfills;

    /// <summary>
    /// An <see cref="IType"/> describing a System.Type.
    /// </summary>
    public class BclTypeWrapper : IType
    {
        private readonly Type _type;
        private int? _genericParameterCount;
        private IType[] _genericTypeArguments;
        private IType _declaringType;
        private IType _elementType;
        private bool _elementTypeChecked;
        private IType _underlyingNonNullableType;

        /// <summary>
        /// Initializes a new instance of the <see cref="BclTypeWrapper"/> class for the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type to which the <see cref="BclTypeWrapper"/> relates.</param>
        private BclTypeWrapper(Type type)
        {
            _type = type;
        }

        #region Factory Methods

        /// <summary>
        /// Creates an <see cref="IType"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to create an <see cref="IType"/>.</param>
        /// <returns>An <see cref="IType"/> for the given <paramref name="type"/>.</returns>
        public static IType For(Type type)
        {
            return type == typeof(object)
                ? BclObjectTypeWrapper.Instance
                : new BclTypeWrapper(type);
        }

        #endregion

        /// <inheritdoc />
        public string Namespace => _type.Namespace;

        /// <inheritdoc />
        public string Name => _type.Name;

        /// <inheritdoc />
        public string FullName => _type.FullName;

        /// <inheritdoc />
        public bool IsInterface => _type.IsInterface();

        /// <inheritdoc />
        public bool IsClass => _type.IsClass();

        /// <inheritdoc />
        public bool IsPrimitive => _type.IsPrimitive();

        /// <inheritdoc />
        public bool IsAnonymous => _type.IsAnonymous();

        /// <inheritdoc />
        public bool IsEnumerable => _type.IsEnumerable();

        /// <inheritdoc />
        public bool IsDictionary => _type.IsDictionary();

        /// <inheritdoc />
        public bool IsGeneric => _type.IsGenericType();

        /// <inheritdoc />
        public int GenericParameterCount
            => _genericParameterCount ??= GetGenericParameterCount();

        private int GetGenericParameterCount()
        {
            var backtickIndex = Name.IndexOf("`", StringComparison.Ordinal);

            return backtickIndex != -1
                ? int.Parse(Name.Substring(backtickIndex + 1)) : 0;
        }

        /// <inheritdoc />
        public IType[] GenericTypeArguments
            => _genericTypeArguments ??= GetGenericTypeArguments();

        private IType[] GetGenericTypeArguments()
        {
            return IsGeneric
                ? _type.GetGenericTypeArguments().ProjectToArray(For)
                : Enumerable<IType>.EmptyArray;
        }

        /// <inheritdoc />
        public bool IsNested => _type.IsNested;

        /// <inheritdoc />
        public IType DeclaringType => _declaringType ??= GetDeclaringType();

        private IType GetDeclaringType()
        {
            return _type.DeclaringType != null
                ? For(_type.DeclaringType) : null;
        }

        /// <inheritdoc />
        public bool IsArray => _type.IsArray;

        /// <inheritdoc />
        public IType ElementType => _elementType ??= GetElementType();

        private IType GetElementType()
        {
            if (_elementTypeChecked)
            {
                return null;
            }

            _elementTypeChecked = true;

            if (_type.TryGetElementType(out var elementType))
            {
                return For(elementType);
            }

            return null;
        }

        /// <inheritdoc />
        public bool IsObjectType => false;

        /// <inheritdoc />
        public bool IsNullable => NonNullableUnderlyingType != null;

        /// <inheritdoc />
        public IType NonNullableUnderlyingType
            => _underlyingNonNullableType ??= GetUnderlyingNonNullableType();

        private IType GetUnderlyingNonNullableType()
        {
            var nonNullableUnderlyingType = Nullable.GetUnderlyingType(_type);

            return nonNullableUnderlyingType != null
                ? new BclTypeWrapper(nonNullableUnderlyingType) : null;
        }

        /// <inheritdoc />
        public Type AsType() => _type;

        /// <summary>
        /// Gets a string representation of this <see cref="IType"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="IType"/>.</returns>
        public override string ToString() => _type.ToString();

        private class BclObjectTypeWrapper : IType
        {
            public static readonly IType Instance = new BclObjectTypeWrapper();

            public string Namespace => typeof(object).Namespace;

            public string Name => "object";

            public string FullName => typeof(object).FullName;

            public bool IsInterface => false;

            public bool IsClass => true;

            public bool IsPrimitive => false;

            public bool IsAnonymous => false;

            public bool IsEnumerable => false;

            public bool IsDictionary => false;

            public bool IsGeneric => false;

            public int GenericParameterCount => 0;

            public IType[] GenericTypeArguments => Enumerable<IType>.EmptyArray;

            public bool IsNested => false;

            public IType DeclaringType => null;

            public bool IsArray => false;

            public IType ElementType => null;

            public bool IsObjectType => true;

            public bool IsNullable => false;

            public IType NonNullableUnderlyingType => null;

            public Type AsType() => typeof(object);
        }
    }
}