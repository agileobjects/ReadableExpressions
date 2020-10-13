namespace AgileObjects.ReadableExpressions.Translations.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;
    using static System.Reflection.GenericParameterAttributes;

    internal static class GenericArgument
    {
        #region Factory Method

        public static IGenericArgument For(Type genericArgument)
        {
            if (!genericArgument.IsOpenGenericArgument())
            {
                return new UnconstrainedGenericArgument(genericArgument);
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
                return new UnconstrainedGenericArgument(genericArgument);
            }

            return new ConstrainedGenericArgument(
                genericArgument,
                constraints,
                constraintTypes);
        }

        #endregion

        #region Implementation Classes

        private class UnconstrainedGenericArgument : IGenericArgument
        {
            public UnconstrainedGenericArgument(Type type)
            {
                Type = type;
            }

            public Type Type { get; }

            public bool HasConstraints => false;

            public bool HasClassConstraint => false;

            public bool HasStructConstraint => false;

            public bool HasNewableConstraint => false;

            public ReadOnlyCollection<Type> TypeConstraints
                => Enumerable<Type>.EmptyReadOnlyCollection;
        }

        private class ConstrainedGenericArgument : IGenericArgument
        {
            public ConstrainedGenericArgument(
                Type type,
                GenericParameterAttributes constraints,
                IList<Type> typeConstraints)
            {
                typeConstraints = GetTypeConstraints(typeConstraints);

                Type = type;
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

            public Type Type { get; }

            public bool HasConstraints => true;

            public bool HasClassConstraint { get; }

            public bool HasStructConstraint { get; }

            public bool HasNewableConstraint { get; }

            public ReadOnlyCollection<Type> TypeConstraints { get; }
        }

        #endregion
    }
}