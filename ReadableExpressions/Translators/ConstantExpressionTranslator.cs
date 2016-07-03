namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Extensions;

    internal class ConstantExpressionTranslator : ExpressionTranslatorBase
    {
        internal ConstantExpressionTranslator()
            : base(ExpressionType.Constant)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var constant = (ConstantExpression)expression;

            if (constant.Value == null)
            {
                return "null";
            }

            if (constant.Type.IsEnum)
            {
                return constant.Type.GetFriendlyName() + "." + constant.Value;
            }

            string translation;

            if (TryTranslateByTypeCode(constant, out translation))
            {
                return translation;
            }

            return constant.Value.ToString();
        }

        private static bool TryTranslateByTypeCode(ConstantExpression constant, out string translation)
        {
            switch (Type.GetTypeCode(Nullable.GetUnderlyingType(constant.Type) ?? constant.Type))
            {
                case TypeCode.Boolean:
                    translation = constant.Value.ToString().ToLowerInvariant();
                    return true;

                case TypeCode.Char:
                    translation = $"'{constant.Value}'";
                    return true;

                case TypeCode.DateTime:
                    if (IsDefault<DateTime>(constant, out translation))
                    {
                        return true;
                    }
                    break;

                case (TypeCode)2:
                    // TypeCode.DBNull (2) is unavailable in a PCL, but can
                    // still be translated:
                    translation = "DBNull.Value";
                    return true;

                case TypeCode.Decimal:
                    translation = FormatNumeric((decimal)constant.Value) + "m";
                    return true;

                case TypeCode.Double:
                    translation = FormatNumeric((double)constant.Value) + "d";
                    return true;

                case TypeCode.Int64:
                    translation = constant.Value + "L";
                    return true;

                case TypeCode.Object:
                    if (IsType(constant, out translation) ||
                        IsFunc(constant, out translation) ||
                        IsDefault<Guid>(constant, out translation) ||
                        IsDefault<TimeSpan>(constant, out translation))
                    {
                        return true;
                    }
                    break;

                case TypeCode.Single:
                    translation = FormatNumeric((float)constant.Value) + "f";
                    return true;

                case TypeCode.String:
                    var stringValue = (string)constant.Value;
                    translation = stringValue.IsComment() ? stringValue : $"\"{stringValue}\"";
                    return true;
            }

            translation = null;
            return false;
        }

        private static string FormatNumeric(decimal value)
        {
            return (value % 1).Equals(0) ? value.ToString("0") : value.ToString();
        }

        private static string FormatNumeric(double value)
        {
            return (value % 1).Equals(0) ? value.ToString("0") : value.ToString();
        }

        private static string FormatNumeric(float value)
        {
            return (value % 1).Equals(0) ? value.ToString("0") : value.ToString();
        }

        private static bool IsType(ConstantExpression constant, out string translation)
        {
            if (typeof(Type).IsAssignableFrom(constant.Type))
            {
                translation = $"typeof({((Type)constant.Value).GetFriendlyName()})";
                return true;
            }

            translation = null;
            return false;
        }

        private static readonly Regex _funcMatcher = new Regex(@"^System\.(?<Type>Func|Action)`\d+\[(?<Arguments>.+)\]$");

        private static bool IsFunc(ConstantExpression constant, out string translation)
        {
            var match = _funcMatcher.Match(constant.Value.ToString());

            if (!match.Success)
            {
                translation = null;
                return false;
            }

            var funcType = match.Groups["Type"].Value;
            var argumentTypes = ParseArgumentNames(match.Groups["Arguments"].Value.Split(','));

            translation = GetGenericTypeName(funcType, argumentTypes);
            return true;
        }

        #region IsFunc Helpers

        private static IEnumerable<string> ParseArgumentNames(params string[] arguments)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];
                var genericArgumentsIndex = argument.IndexOf('`');

                if (genericArgumentsIndex == -1)
                {
                    yield return GetTypeName(argument);
                    continue;
                }

                if (argument.StartsWith("System.Nullable`"))
                {
                    yield return GetNullableTypeName(argument);
                    continue;
                }

                var genericTypeName = GetTypeName(argument.Substring(0, genericArgumentsIndex));
                var genericTypeArgumentsStart = argument.IndexOf('[', genericArgumentsIndex) + 1;
                argument = argument.Substring(genericTypeArgumentsStart);
                var genericTypeArgumentsClosesNeeded = 1;
                var genericTypeArgumentsString = argument;

                while (i < arguments.Length - 1)
                {
                    if (argument.IndexOf('`') != -1)
                    {
                        ++genericTypeArgumentsClosesNeeded;
                    }

                    argument = arguments[++i];
                    genericTypeArgumentsString += "," + argument;

                    if (argument.EndsWith(']') && (argument[argument.Length - 2] != '['))
                    {
                        --genericTypeArgumentsClosesNeeded;
                    }

                    if (genericTypeArgumentsClosesNeeded == 0)
                    {
                        genericTypeArgumentsString =
                            genericTypeArgumentsString.Substring(0, genericTypeArgumentsString.Length - 1);
                        break;
                    }
                }

                var genericTypeArguments = ParseArgumentNames(genericTypeArgumentsString.Split(','));

                yield return GetGenericTypeName(genericTypeName, genericTypeArguments);
            }
        }

        private static readonly int _nullableTypeArgumentStart = "System.Nullable`1[".Length;

        private static string GetNullableTypeName(string typeFullName)
        {
            var nullableTypeNameLength = typeFullName.Length - _nullableTypeArgumentStart - 1;
            var nullableTypeName = typeFullName.Substring(_nullableTypeArgumentStart, nullableTypeNameLength);

            return GetTypeName(nullableTypeName) + "?";
        }

        private static string GetTypeName(string typeFullName)
        {
            if (typeFullName.EndsWith("[]", StringComparison.Ordinal))
            {
                return GetTypeName(typeFullName.Substring(0, typeFullName.Length - 2)) + "[]";
            }

            return typeFullName.GetSubstitutionOrNull() ??
                   typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);
        }

        private static string GetGenericTypeName(string typeName, IEnumerable<string> arguments)
            => $"{typeName}<{string.Join(", ", arguments)}>";

        #endregion

        private static bool IsDefault<T>(ConstantExpression constant, out string translation)
        {
            if ((constant.Type == typeof(T)) && constant.Value.Equals(default(T)))
            {
                translation = $"default({typeof(T).Name})";
                return true;
            }

            translation = null;
            return false;
        }
    }
}