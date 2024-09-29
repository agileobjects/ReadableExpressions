namespace AgileObjects.ReadableExpressions.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetStandardPolyfills;
using Translations.Reflection;
#if FEATURE_VALUE_TUPLE
using static System.StringComparison;
#endif

internal static class InternalReflectionExtensions
{
    private static readonly Dictionary<string, string> _typeNameKeywords = new()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            { typeof(string).FullName, "string" },
            { typeof(int).FullName, "int" },
            { typeof(bool).FullName, "bool" },
            { typeof(decimal).FullName, "decimal" },
            { typeof(long).FullName, "long" },
            { typeof(double).FullName, "double" },
            { typeof(object).FullName, "object" },
            { typeof(byte).FullName, "byte" },
            { typeof(short).FullName, "short" },
            { typeof(float).FullName, "float" },
            { typeof(char).FullName, "char" },
            { typeof(uint).FullName, "uint" },
            { typeof(ulong).FullName, "ulong" },
            { typeof(sbyte).FullName, "sbyte" },
            { typeof(ushort).FullName, "ushort" },
            { typeof(void).FullName, "void" }
            // ReSharper restore AssignNullToNotNullAttribute
        };

    public static ICollection<string> TypeNames => _typeNameKeywords.Values;

    public static string GetKeywordOrNull(this IType type)
    {
        if (type.FullName == null)
        {
            return null;
        }

        return _typeNameKeywords.TryGetValue(type.FullName, out var substitutedName)
            ? substitutedName : null;
    }

    public static object GetValue(this MemberInfo member, object subject)
        => member.TryGetValue(subject, out var value) ? value : null;

    public static Type GetMemberInfoType(this MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            MethodInfo method => method.ReturnType,
            _ => null
        };
    }

    public static bool TryGetValue(
        this MemberInfo member,
        object subject,
        out object value)
    {
        var hasSubject = subject != null;

        switch (member)
        {
            case FieldInfo field when hasSubject || field.IsStatic:
                value = field.GetValue(subject);
                break;

            case PropertyInfo property when hasSubject || property.IsStatic():
                value = property.GetValue(subject, Enumerable<object>.EmptyArray);
                break;

            case MethodInfo method when method.IsCallable(subject, out var parameters):
                value = method.Invoke(subject, parameters);
                break;

            default:
                value = null;
                return false;
        }

        return true;
    }

    private static bool IsCallable(
        this MethodInfo method,
        object subject,
        out object[] parameters)
    {
        if (!method.IsPure())
        {
            parameters = null;
            return false;
        }

        var parameterCount = method.GetParameters().Length;
        var isParameterless = parameterCount == 0;

        if (!method.IsStatic)
        {
            parameters = Enumerable<object>.EmptyArray;
            return isParameterless && subject != null;
        }

        if (isParameterless)
        {
            parameters = Enumerable<object>.EmptyArray;
            return true;
        }

        if (parameterCount == 1 && subject != null &&
            method.IsExtensionMethod())
        {
            parameters = [subject];
            return true;
        }

        parameters = null;
        return false;
    }

    private static bool IsPure(this MethodInfo method)
    {
        if (method.DeclaringType == typeof(Enumerable))
        {
            return method.Name switch
            {
                nameof(Enumerable.Any) => true,
                nameof(Enumerable.First) => true,
                nameof(Enumerable.FirstOrDefault) => true,
                nameof(Enumerable.Last) => true,
                nameof(Enumerable.LastOrDefault) => true,
                _ => false
            };
        }

        return method
            .GetCustomAttributes(inherit: false)
            .Any(attr => attr.GetType().Name == "PureAttribute");
    }

#if FEATURE_VALUE_TUPLE
    public static bool IsValueTuple(this Type type)
    {
        return type.IsGenericType() &&
               type.FullName?.StartsWith("System.ValueTuple`", Ordinal) == true;
    }
#endif
}
