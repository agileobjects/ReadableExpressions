namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
#if FEATURE_CONCURRENT_DICTIONARY
    using System.Collections.Concurrent;
#endif
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    /// <summary>
    /// An <see cref="IType"/> describing a System.Type.
    /// </summary>
    public class BclTypeWrapper : IType
    {
        /// <summary>
        /// Gets the singleton <see cref="IType"/> representing System.Object.
        /// </summary>
        public static readonly IType Object = new BclBaseTypeWrapper<object>(baseType: null);

        /// <summary>
        /// Gets the singleton <see cref="IType"/> representing System.ValueType.
        /// </summary>
        public static readonly IType ValueType = new BclBaseTypeWrapper<ValueType>(baseType: Object);

        /// <summary>
        /// Gets the singleton <see cref="IType"/> representing System.String.
        /// </summary>
        public static readonly IType String = new BclTypeWrapper(typeof(string));

        /// <summary>
        /// Gets the singleton <see cref="IType"/> representing System.Enum.
        /// </summary>
        public static readonly IType Enum = new BclBaseTypeWrapper<Enum>(baseType: ValueType);

        /// <summary>
        /// Gets the singleton <see cref="IType"/> representing System.Void.
        /// </summary>
        public static readonly IType Void = new BclTypeWrapper(typeof(void));

#if FEATURE_CONCURRENT_DICTIONARY
        private static readonly ConcurrentDictionary<Type, Lazy<IType>> _types = new();
#else
        private static readonly object _typeCacheLock = new object();
        private static readonly Dictionary<Type, IType> _types = new Dictionary<Type, IType>();
#endif
        private readonly Type _type;
        private IType _baseType;
        private int? _genericParameterCount;
        private ReadOnlyCollection<IType> _genericTypeArguments;
        private ReadOnlyCollection<IType> _constraintTypes;
        private IType _declaringType;
        private IType _elementType;
        private bool _elementTypeChecked;
        private IType _underlyingNonNullableType;
        private ReadOnlyCollection<IType> _allInterfaces;
        private IEnumerable<IMember> _allMembers;

        private BclTypeWrapper(Type type)
        {
            _type = type;
        }

        #region Factory Methods

        /// <summary>
        /// Creates an <see cref="IType"/> for the given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The Type for which to create an <see cref="IType"/>.</typeparam>
        /// <returns>An <see cref="IType"/> for the given <typeparamref name="T"/> type.</returns>
        public static IType For<T>() => For(typeof(T));

        /// <summary>
        /// Creates an <see cref="IType"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The Type for which to create an <see cref="IType"/>.</param>
        /// <returns>An <see cref="IType"/> for the given <paramref name="type"/>.</returns>
        public static IType For(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (type == typeof(void))
            {
                return Void;
            }

            if (type == typeof(string))
            {
                return String;
            }

            if (type == typeof(object))
            {
                return Object;
            }

            if (type == typeof(ValueType))
            {
                return ValueType;
            }

#if FEATURE_CONCURRENT_DICTIONARY
            return _types.GetOrAdd(type, new Lazy<IType>(() => new BclTypeWrapper(type))).Value;
#else
            lock (_typeCacheLock)
            {
                if (!_types.TryGetValue(type, out var typeWrapper))
                {
                    _types.Add(type, typeWrapper = new BclTypeWrapper(type));
                }

                return typeWrapper;
            }
#endif
        }

        #endregion

        /// <summary>
        /// Determines whether the given <paramref name="type"/> represents the same Type as the
        /// given <paramref name="otherType"/>.
        /// </summary>
        /// <param name="type">The first <see cref="IType"/> for which to make the determination.</param>
        /// <param name="otherType">The second <see cref="IType"/> for which to make the determination.</param>
        /// <returns>
        /// True if the given <paramref name="type"/> represents the same Type as the given
        /// <paramref name="otherType"/>, otherwise false.
        /// </returns>
        public static bool AreEqual(IType type, IType otherType)
        {
            if (ReferenceEquals(otherType, type))
            {
                return true;
            }

            return otherType?.FullName != null && otherType.FullName == type.FullName;
        }

        /// <inheritdoc />
        public IType BaseType
            => _baseType ??= For(_type.GetBaseType());

        /// <inheritdoc />
        public ReadOnlyCollection<IType> AllInterfaces
            => _allInterfaces ??= GetAllInterfaces().ToReadOnlyCollection();

        private IList<IType> GetAllInterfaces()
            => _type.GetAllInterfaces().ProjectToArray(For);

        /// <inheritdoc />
        public Assembly Assembly => _type.GetAssembly();

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
        public bool IsEnum => _type.IsEnum();

        /// <inheritdoc />
        public bool IsPrimitive => _type.IsPrimitive();

        /// <inheritdoc />
        public bool IsAnonymous => _type.IsAnonymous();

        /// <inheritdoc />
        public bool IsAbstract => _type.IsAbstract();

        /// <inheritdoc />
        public bool IsSealed => _type.IsSealed();

        /// <inheritdoc />
        public bool IsEnumerable => _type.IsEnumerable();

        /// <inheritdoc />
        public bool IsDictionary => _type.IsDictionary();

        /// <inheritdoc />
        public bool IsGeneric => _type.IsGenericType();

        /// <inheritdoc />
        public bool IsGenericDefinition
#if NETSTANDARD1_0
            => _type.GetTypeInfo().IsGenericTypeDefinition;
#else
            => _type.IsGenericTypeDefinition;

#endif
        /// <inheritdoc />
        public IType GenericDefinition => IsGenericDefinition
            ? For(_type.GetGenericTypeDefinition())
            : null;

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
        public ReadOnlyCollection<IType> GenericTypeArguments
            => _genericTypeArguments ??= GetGenericTypeArguments().ToReadOnlyCollection();

        private IType[] GetGenericTypeArguments()
        {
            return IsGeneric
                ? _type.GetGenericTypeArguments().ProjectToArray(For)
                : Enumerable<IType>.EmptyArray;
        }

        /// <inheritdoc />
        public bool IsGenericParameter
        {
            get
            {
#if NETSTANDARD1_0
                return _type.GetTypeInfo().IsGenericParameter;
#else
                return _type.IsGenericParameter;
#endif
            }
        }

        /// <inheritdoc />
        public GenericParameterAttributes Constraints
        {
            get
            {
#if NETSTANDARD1_0
                return _type.GetTypeInfo().GenericParameterAttributes;
#else
                return _type.GenericParameterAttributes;
#endif
            }
        }

        /// <inheritdoc />
        public ReadOnlyCollection<IType> ConstraintTypes
        {
            get
            {
                return _constraintTypes ??= GetUniqueConstraintTypes()
                    .ToReadOnlyCollection();
            }
        }

        private IList<IType> GetUniqueConstraintTypes()
        {
            var typeConstraints = GetConstraintTypes().ProjectToArray(For);
            var constraintCount = typeConstraints.Length;

            switch (constraintCount)
            {
                case 0:
                    return Enumerable<IType>.EmptyArray;

                case 1:
                    return typeConstraints;

                default:
                    var previousConstraint = typeConstraints[0];
                    var previousConstraintInterfaces = previousConstraint.AllInterfaces;
                    var constraints = new List<IType> { previousConstraint };

                    for (var i = 1; ;)
                    {
                        var constraint = typeConstraints[i];

                        var addConstraint =
                            !constraint.IsAssignableTo(previousConstraint) &&
                            !previousConstraintInterfaces.Contains(constraint);

                        if (addConstraint)
                        {
                            constraints.Add(constraint);
                        }

                        ++i;

                        if (i == constraintCount)
                        {
                            break;
                        }

                        if (addConstraint)
                        {
                            previousConstraint = constraint;
                            previousConstraintInterfaces = previousConstraint.AllInterfaces;
                        }
                    }

                    return constraints;
            }
        }

        private IList<Type> GetConstraintTypes()
        {
#if NETSTANDARD1_0
            return _type.GetTypeInfo().GetGenericParameterConstraints();
#else
            return _type.GetGenericParameterConstraints();
#endif
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

            if (IsByRef)
            {
                return For(_type.GetElementType());
            }

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

        /// <inheritdoc />
        public bool IsByRef => _type.IsByRef;

        private IType GetUnderlyingNonNullableType()
        {
            var nonNullableUnderlyingType = Nullable.GetUnderlyingType(_type);

            return nonNullableUnderlyingType != null
                ? new BclTypeWrapper(nonNullableUnderlyingType) : null;
        }

        /// <inheritdoc />
        public IEnumerable<IMember> AllMembers => _allMembers ??= _type.GetAllMembers();

        /// <inheritdoc />
        public IEnumerable<IMember> GetMembers(Action<MemberSelector> selectionConfiguator)
            => AllMembers.Select(selectionConfiguator);

        /// <inheritdoc />
        public IEnumerable<TMember> GetMembers<TMember>(Action<MemberSelector> selectionConfiguator)
            where TMember : IMember
        {
            return GetMembers(selectionConfiguator).OfType<TMember>();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            switch (obj)
            {
                case IType type:
                    return Equals(type);

                case Type bclType:
                    return Equals(For(bclType));

                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public bool Equals(IType otherType) => AreEqual(this, otherType);

        /// <inheritdoc />
        public Type AsType() => _type;

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return _type.GetHashCode() * 397;
            }
        }

        /// <summary>
        /// Gets a string representation of this <see cref="IType"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="IType"/>.</returns>
        public override string ToString() => _type.ToString();

        private class BclBaseTypeWrapper<T> : IType
        {
            private IEnumerable<IMember> _allMembers;

            public BclBaseTypeWrapper(IType baseType)
            {
                BaseType = baseType;
            }

            public IType BaseType { get; }

            public ReadOnlyCollection<IType> AllInterfaces
                => Enumerable<IType>.EmptyReadOnlyCollection;

            public Assembly Assembly => typeof(T).GetAssembly();

            public string Namespace => typeof(T).Namespace;

            public string Name => typeof(T).Name;

            public string FullName => typeof(T).FullName;

            public bool IsInterface => false;

            public bool IsClass => typeof(T).IsClass();

            public bool IsEnum => typeof(T).IsEnum();

            public bool IsPrimitive => false;

            public bool IsAnonymous => false;

            public bool IsAbstract => typeof(T).IsAbstract();

            public bool IsSealed => typeof(T).IsSealed();

            public bool IsEnumerable => false;

            public bool IsDictionary => false;

            public bool IsGeneric => false;

            public bool IsGenericDefinition => false;

            public IType GenericDefinition => null;

            public int GenericParameterCount => 0;

            public ReadOnlyCollection<IType> GenericTypeArguments
                => Enumerable<IType>.EmptyReadOnlyCollection;

            public bool IsGenericParameter => false;

            public GenericParameterAttributes Constraints => default;

            public ReadOnlyCollection<IType> ConstraintTypes
                => Enumerable<IType>.EmptyReadOnlyCollection;

            public bool IsNested => false;

            public IType DeclaringType => null;

            public bool IsArray => false;

            public IType ElementType => IsByRef ? this : null;

            public bool IsObjectType => true;

            public bool IsNullable => false;

            public IType NonNullableUnderlyingType => null;

            public bool IsByRef => false;

            public IEnumerable<IMember> AllMembers => _allMembers ??= typeof(T).GetAllMembers();

            public IEnumerable<IMember> GetMembers(Action<MemberSelector> selectionConfiguator)
                => AllMembers.Select(selectionConfiguator);

            public IEnumerable<TMember> GetMembers<TMember>(Action<MemberSelector> selectionConfiguator)
                where TMember : IMember
            {
                return GetMembers(selectionConfiguator).OfType<TMember>();
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                switch (obj)
                {
                    case IType type:
                        return Equals(type);

                    case Type bclType:
                        return Equals(For(bclType));

                    default:
                        return false;
                }
            }

            public bool Equals(IType otherType) => AreEqual(this, otherType);

            public Type AsType() => typeof(T);

            public override int GetHashCode()
            {
                unchecked
                {
                    return typeof(T).GetHashCode() * 397;
                }
            }
        }
    }
}