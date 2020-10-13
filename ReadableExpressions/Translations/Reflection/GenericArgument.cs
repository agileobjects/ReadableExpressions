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
            var implementedInterfaces = genericArgument.GetImplementedInterfaces();
            var hasInterfaces = implementedInterfaces.Any();

            if (constraints == None && !hasInterfaces)
            {
                return new UnconstrainedGenericArgument(genericArgument);
            }

            return new ConstrainedGenericArgument(
                genericArgument,
                constraints,
                implementedInterfaces);
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
                Type = type;
                TypeConstraints = GetTypeConstraints(typeConstraints);
                HasStructConstraint = (constraints | NotNullableValueTypeConstraint) == constraints;

                if (HasStructConstraint)
                {
                    return;
                }

                HasClassConstraint = (constraints | ReferenceTypeConstraint) == constraints;
                HasNewableConstraint = (constraints | DefaultConstructorConstraint) == constraints;
            }

            #region Setup

            private static ReadOnlyCollection<Type> GetTypeConstraints(IList<Type> typeConstraints)
            {
                var constraintCount = typeConstraints.Count;

                switch (constraintCount)
                {
                    case 0:
                        return Enumerable<Type>.EmptyReadOnlyCollection;

                    case 1:
                        return typeConstraints.ToReadOnlyCollection();

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

                        return constraints.ToReadOnlyCollection();
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