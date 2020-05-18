namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using NetStandardPolyfills;

    internal abstract class GenericTypeNameWriterBase
    {
        public void WriteGenericTypeName(Type genericType)
        {
            var typeGenericTypeArguments = genericType.GetGenericTypeArguments();

            if (!genericType.IsNested)
            {
                WriteTypeNamePrefix(genericType);
                WriteClosedGenericTypeName(genericType, ref typeGenericTypeArguments);
                return;
            }

            var types = new List<Type> { genericType };

            // ReSharper disable once PossibleNullReferenceException
            while (genericType.IsNested)
            {
                genericType = genericType.DeclaringType;
                types.Add(genericType);
            }

            WriteTypeNamePrefix(genericType);

            for (var i = types.Count - 1; ;)
            {
                WriteClosedGenericTypeName(types[i], ref typeGenericTypeArguments);

                if (i == 0)
                {
                    return;
                }

                --i;
                WriteNestedTypeNamesSeparator();
            }
        }

        private void WriteClosedGenericTypeName(Type genericType, ref Type[] typeGenericTypeArguments)
        {
            var typeName = genericType.Name;

            var backtickIndex = typeName.IndexOf("`", StringComparison.Ordinal);

            if (backtickIndex == -1)
            {
                WriteTypeName(typeName, genericType);
                return;
            }

            var numberOfParameters = int.Parse(typeName.Substring(backtickIndex + 1));

            Type[] typeArguments;

            if (numberOfParameters == typeGenericTypeArguments.Length)
            {
                typeArguments = typeGenericTypeArguments;
                goto WriteName;
            }

            switch (numberOfParameters)
            {
                case 1:
                    typeArguments = new[] { typeGenericTypeArguments[0] };
                    break;

                case 2:
                    typeArguments = new[] { typeGenericTypeArguments[0], typeGenericTypeArguments[1] };
                    break;

                default:
                    typeArguments = new Type[numberOfParameters];

                    Array.Copy(
                        typeGenericTypeArguments,
                        typeArguments,
                        numberOfParameters);

                    break;
            }

            var numberOfRemainingTypeArguments = typeGenericTypeArguments.Length - numberOfParameters;
            var typeGenericTypeArgumentsSubset = new Type[numberOfRemainingTypeArguments];

            Array.Copy(
                typeGenericTypeArguments,
                numberOfParameters,
                typeGenericTypeArgumentsSubset,
                0,
                numberOfRemainingTypeArguments);

            typeGenericTypeArguments = typeGenericTypeArgumentsSubset;

        WriteName:
            WriteGenericTypeName(genericType, numberOfParameters, typeArguments);
        }

        private void WriteTypeName(string name, Type type)
        {
            if (type.IsInterface())
            {
                WriteInterfaceName(name);
            }
            else
            {
                WriteTypeName(name);
            }
        }

        protected abstract void WriteTypeName(string name);

        protected abstract void WriteInterfaceName(string name);

        private void WriteGenericTypeName(
            Type type,
            int numberOfParameters,
            IList<Type> typeArguments)
        {
            var isAnonType =
                 type.Name.StartsWith('<') &&
                (type.Name.IndexOf("AnonymousType", StringComparison.Ordinal)) != -1;

            if (isAnonType && TryWriteCustomAnonTypeName(type))
            {
                return;
            }

            string typeName;

            if (isAnonType)
            {
                typeName = "AnonymousType";
            }
            else
            {
                var parameterCountIndex = type.Name.IndexOf("`" + numberOfParameters, StringComparison.Ordinal);
                typeName = type.Name.Substring(0, parameterCountIndex);
            }

            WriteTypeName(typeName, type);
            WriteTypeArgumentNamePrefix();

            for (var i = 0; ;)
            {
                var typeArgument = typeArguments[i];

                WriteTypeName(typeArgument);

                ++i;

                if (i == typeArguments.Count)
                {
                    break;
                }

                WriteTypeArgumentNameSeparator();
            }

            WriteTypeArgumentNameSuffix();
        }

        protected virtual bool TryWriteCustomAnonTypeName(Type anonType) => false;

        protected abstract void WriteTypeArgumentNamePrefix();

        protected abstract void WriteTypeName(Type type);

        protected abstract void WriteTypeArgumentNameSeparator();

        protected abstract void WriteTypeArgumentNameSuffix();

        protected virtual void WriteTypeNamePrefix(Type genericType)
        {
        }

        protected abstract void WriteNestedTypeNamesSeparator();
    }


}