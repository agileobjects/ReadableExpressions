namespace AgileObjects.ReadableExpressions.Extensions;

using System.Collections.Generic;
using Translations.Reflection;
using static System.StringComparison;

internal abstract class GenericTypeNameWriterBase
{
    public void WriteGenericTypeName(IType genericType) => 
        WriteGenericTypeName(genericType, genericType.GenericTypeArguments);

    public void WriteGenericTypeName(IType genericType, IList<IType> typeGenericTypeArguments)
    {
        if (!genericType.IsNested)
        {
            WriteTypeNamePrefix(genericType);
            WriteClosedGenericTypeName(genericType, ref typeGenericTypeArguments);
            return;
        }

        var types = new List<IType> { genericType };

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

    private void WriteClosedGenericTypeName(
        IType genericType,
        ref IList<IType> typeGenericTypeArguments)
    {
        var typeName = genericType.Name;
        var numberOfParameters = genericType.GenericParameterCount;

        if (numberOfParameters == 0)
        {
            WriteTypeName(typeName, genericType);
            return;
        }

        IList<IType> typeArguments;

        if (numberOfParameters == typeGenericTypeArguments.Count)
        {
            typeArguments = typeGenericTypeArguments;
            goto WriteName;
        }

        switch (numberOfParameters)
        {
            case 1:
                typeArguments = [typeGenericTypeArguments[0]];
                break;

            case 2:
                typeArguments = [typeGenericTypeArguments[0], typeGenericTypeArguments[1]];
                break;

            default:
                typeArguments = new IType[numberOfParameters];
                typeGenericTypeArguments.CopyTo(typeArguments, numberOfParameters);
                break;
        }

        var numberOfRemainingTypeArguments = typeGenericTypeArguments.Count - numberOfParameters;
        var typeGenericTypeArgumentsSubset = new IType[numberOfRemainingTypeArguments];

        typeGenericTypeArguments.CopyTo(
            typeGenericTypeArgumentsSubset,
            numberOfParameters,
            0,
            numberOfRemainingTypeArguments);

        typeGenericTypeArguments = typeGenericTypeArgumentsSubset;

    WriteName:
        WriteGenericTypeName(genericType, numberOfParameters, typeArguments);
    }

    private void WriteTypeName(string name, IType type)
    {
        if (type.IsInterface)
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
        IType type,
        int numberOfParameters,
        IList<IType> typeArguments)
    {
        var isAnonType = type.IsAnonymous;

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
            var parameterCountIndex = type.Name.IndexOf("`" + numberOfParameters, Ordinal);
            typeName = type.Name.Substring(0, parameterCountIndex);
        }

        WriteTypeName(typeName, type);
        WriteTypeArgumentNamePrefix();

        for (var i = 0; ;)
        {
            var typeArgument = typeArguments[i];

            WriteTypeArgumentName(typeArgument);
            ++i;

            if (i == typeArguments.Count)
            {
                break;
            }

            WriteTypeArgumentNameSeparator();
        }

        WriteTypeArgumentNameSuffix();
    }

    protected virtual bool TryWriteCustomAnonTypeName(IType anonType) => false;

    protected abstract void WriteTypeArgumentNamePrefix();

    protected abstract void WriteTypeArgumentName(IType type);

    protected abstract void WriteTypeArgumentNameSeparator();

    protected abstract void WriteTypeArgumentNameSuffix();

    protected virtual void WriteTypeNamePrefix(IType genericType)
    {
    }

    protected abstract void WriteNestedTypeNamesSeparator();
}