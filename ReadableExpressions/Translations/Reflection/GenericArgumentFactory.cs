namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
#if !NET35
    using System.Linq;
#endif
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;
    using static System.Reflection.GenericParameterAttributes;

    /// <summary>
    /// Factory class for creating <see cref="IGenericArgument"/> instances.
    /// </summary>
    public static class GenericArgumentFactory
    {
        /// <summary>
        /// Creates an <see cref="IGenericArgument"/> for the given <paramref name="genericArgument"/>,
        /// using the given <paramref name="settings"/>.
        /// </summary>
        /// <param name="genericArgument">The Type representing the generic argument.</param>
        /// <param name="settings">The <see cref="TranslationSettings"/> to use.</param>
        /// <returns>An <see cref="IGenericArgument"/> for the given <paramref name="genericArgument"/> Type.</returns>
        public static IGenericArgument For(
            Type genericArgument,
            TranslationSettings settings)
        {
            if (!genericArgument.IsGenericParameter())
            {
                return new UnconstrainedGenericArgument(genericArgument, settings);
            }

            var constraints = genericArgument.GetConstraints();
            var constraintTypes = genericArgument.GetImplementedInterfaces();

            var argumentBaseType = genericArgument.GetBaseType();

            if (argumentBaseType != typeof(object))
            {
                if (constraintTypes.IsReadOnly)
                {
                    constraintTypes = new List<Type>(constraintTypes);
                }

                constraintTypes.Insert(0, argumentBaseType);
            }

            var hasTypeConstraints = constraintTypes.Any();

            if (constraints == None && !hasTypeConstraints)
            {
                return new UnconstrainedGenericArgument(genericArgument, settings);
            }

            return new ConstrainedGenericArgument(
                genericArgument,
                constraints,
                constraintTypes,
                settings);
        }

        #region Implementation Classes

        private abstract class GenericArgumentBase
        {
            private readonly TranslationSettings _settings;
            private string _typeName;

            protected GenericArgumentBase(Type type, TranslationSettings settings)
            {
                _settings = settings;
                Type = type;
            }

            public Type Type { get; }

            public string TypeName
                => _typeName ??= Type.GetFriendlyName(_settings);

            public bool IsClosed => Type?.FullName != null;
        }

        private class UnconstrainedGenericArgument : GenericArgumentBase, IGenericArgument
        {
            public UnconstrainedGenericArgument(Type type, TranslationSettings settings)
                : base(type, settings)
            {
            }

            public bool HasConstraints => false;

            public bool HasClassConstraint => false;

            public bool HasStructConstraint => false;

            public bool HasNewableConstraint => false;

            public ReadOnlyCollection<Type> TypeConstraints
                => Enumerable<Type>.EmptyReadOnlyCollection;
        }

        private class ConstrainedGenericArgument : GenericArgumentBase, IGenericArgument
        {
            public ConstrainedGenericArgument(
                Type type,
                GenericParameterAttributes constraints,
                IList<Type> typeConstraints,
                TranslationSettings settings)
                : base(type, settings)
            {
                typeConstraints = GetTypeConstraints(typeConstraints);

                HasStructConstraint = (constraints | NotNullableValueTypeConstraint) == constraints;

                if (HasStructConstraint)
                {
                    typeConstraints.Remove(typeof(ValueType));
                }
                else
                {
                    HasClassConstraint = (constraints | ReferenceTypeConstraint) == constraints;
                    HasNewableConstraint = (constraints | DefaultConstructorConstraint) == constraints;
                }

                TypeConstraints = typeConstraints.ToReadOnlyCollection();
            }

            #region Setup

            private static IList<Type> GetTypeConstraints(IList<Type> typeConstraints)
            {
                var constraintCount = typeConstraints.Count;

                switch (constraintCount)
                {
                    case 0:
                        return Enumerable<Type>.EmptyArray;

                    case 1:
                        return typeConstraints;

                    default:
                        var previousConstraint = typeConstraints[0];
                        var previousConstraintInterfaces = previousConstraint.GetAllInterfaces();
                        var constraints = new List<Type> { previousConstraint };

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
                                previousConstraintInterfaces = previousConstraint.GetAllInterfaces();
                            }
                        }

                        return constraints;
                }
            }

            #endregion

            public bool HasConstraints => true;

            public bool HasClassConstraint { get; }

            public bool HasStructConstraint { get; }

            public bool HasNewableConstraint { get; }

            public ReadOnlyCollection<Type> TypeConstraints { get; }
        }

        #endregion
    }
}