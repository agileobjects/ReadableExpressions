namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using static System.Reflection.GenericParameterAttributes;

    /// <summary>
    /// Factory class for creating <see cref="IGenericParameter"/> instances.
    /// </summary>
    public static class GenericParameterFactory
    {
        /// <summary>
        /// Creates an <see cref="IGenericParameter"/> for the given <paramref name="genericParameter"/>.
        /// </summary>
        /// <param name="genericParameter">The Type representing the generic argument.</param>
        /// <returns>An <see cref="IGenericParameter"/> for the given <paramref name="genericParameter"/> Type.</returns>
        public static IGenericParameter For(Type genericParameter)
            => For(BclTypeWrapper.For(genericParameter));

        /// <summary>
        /// Creates an <see cref="IGenericParameter"/> for the given <paramref name="genericParameter"/>.
        /// </summary>
        /// <param name="genericParameter">The <see cref="IType"/> representing the generic argument.</param>
        /// <returns>An <see cref="IGenericParameter"/> for the given <paramref name="genericParameter"/> Type.</returns>
        public static IGenericParameter For(IType genericParameter)
        {
            if (!genericParameter.IsGenericParameter ||
                (genericParameter.Constraints == None && !genericParameter.ConstraintTypes.Any()))
            {
                return new UnconstrainedGenericParameter(genericParameter);
            }

            return new ConstrainedGenericParameter(genericParameter);
        }

        #region Implementation Classes

        private abstract class GenericParameterBase : IType
        {
            private readonly IType _type;

            protected GenericParameterBase(IType type)
            {
                _type = type;
            }

            #region IType Members

            public IType BaseType => _type.BaseType;

            public ReadOnlyCollection<IType> AllInterfaces => _type.AllInterfaces;

            public string Namespace => _type.Namespace;

            public string Name => _type.Name;

            public string FullName => _type.FullName;

            public bool IsInterface => _type.IsInterface;

            public bool IsClass => _type.IsClass;

            public bool IsEnum => _type.IsEnum;

            public bool IsPrimitive => _type.IsPrimitive;

            public bool IsAnonymous => _type.IsAnonymous;

            public bool IsAbstract => _type.IsAbstract;

            public bool IsSealed => _type.IsSealed;

            public bool IsEnumerable => _type.IsEnumerable;

            public bool IsDictionary => _type.IsDictionary;

            public bool IsGeneric => _type.IsGeneric;

            public bool IsGenericDefinition => _type.IsGenericDefinition;

            public IType GenericDefinition => _type.GenericDefinition;

            public int GenericParameterCount => _type.GenericParameterCount;

            public ReadOnlyCollection<IType> GenericTypeArguments => _type.GenericTypeArguments;

            public bool IsGenericParameter => _type.IsGenericParameter;

            public abstract GenericParameterAttributes Constraints { get; }

            public abstract ReadOnlyCollection<IType> ConstraintTypes { get; }

            public bool IsNested => _type.IsNested;

            public IType DeclaringType => _type.DeclaringType;

            public bool IsArray => _type.IsArray;

            public IType ElementType => _type.ElementType;

            public bool IsObjectType => _type.IsObjectType;

            public bool IsNullable => _type.IsNullable;

            public IType NonNullableUnderlyingType => _type.NonNullableUnderlyingType;

            public bool IsByRef => _type.IsByRef;

            public IEnumerable<IMember> AllMembers => _type.AllMembers;

            public IEnumerable<IMember> GetMembers(Action<MemberSelector> selectionConfiguator)
                => _type.GetMembers(selectionConfiguator);

            public IEnumerable<TMember> GetMembers<TMember>(Action<MemberSelector> selectionConfiguator)
                where TMember : IMember
            {
                return _type.GetMembers(selectionConfiguator).OfType<TMember>();
            }

            public bool Equals(IType otherType) => _type.Equals(otherType);

            public Type AsType() => _type.AsType();

            #endregion

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
                        return Equals(BclTypeWrapper.For(bclType));

                    default:
                        return false;
                }
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return _type.GetHashCode() * 397;
                }
            }
        }

        private class UnconstrainedGenericParameter : GenericParameterBase, IGenericParameter
        {
            public UnconstrainedGenericParameter(IType type)
                : base(type)
            {
            }

            public bool HasConstraints => false;

            public bool HasClassConstraint => false;

            public bool HasStructConstraint => false;

            public bool HasNewableConstraint => false;

            public override GenericParameterAttributes Constraints => None;

            public override ReadOnlyCollection<IType> ConstraintTypes
                => Enumerable<IType>.EmptyReadOnlyCollection;
        }

        private class ConstrainedGenericParameter : GenericParameterBase, IGenericParameter
        {
            public ConstrainedGenericParameter(IType type)
                : base(type)
            {
                var constraints = Constraints = type.Constraints;

                var constraintTypes = new List<IType>(type.ConstraintTypes);

                HasStructConstraint = (constraints | NotNullableValueTypeConstraint) == constraints;

                if (HasStructConstraint)
                {
                    constraintTypes.Remove(BclTypeWrapper.ValueType);
                }
                else
                {
                    HasClassConstraint = (constraints | ReferenceTypeConstraint) == constraints;
                    HasNewableConstraint = (constraints | DefaultConstructorConstraint) == constraints;
                }

                ConstraintTypes = constraintTypes.ToReadOnlyCollection();
            }

            public bool HasConstraints => true;

            public bool HasClassConstraint { get; }

            public bool HasStructConstraint { get; }

            public bool HasNewableConstraint { get; }

            public override GenericParameterAttributes Constraints { get; }

            public override ReadOnlyCollection<IType> ConstraintTypes { get; }
        }

        #endregion
    }
}