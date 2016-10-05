namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
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

            if (constant.Type.IsEnum())
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
            switch ((Nullable.GetUnderlyingType(constant.Type) ?? constant.Type).GetTypeCode())
            {
                case NetStandardTypeCode.Boolean:
                    translation = constant.Value.ToString().ToLowerInvariant();
                    return true;

                case NetStandardTypeCode.Char:
                    translation = $"'{constant.Value}'";
                    return true;

                case NetStandardTypeCode.DateTime:
                    if (IsDefault<DateTime>(constant, out translation))
                    {
                        return true;
                    }

                    translation = TranslateDateTime((DateTime)constant.Value);
                    return true;

                case NetStandardTypeCode.DBNull:
                    translation = "DBNull.Value";
                    return true;

                case NetStandardTypeCode.Decimal:
                    translation = FormatNumeric((decimal)constant.Value) + "m";
                    return true;

                case NetStandardTypeCode.Double:
                    translation = FormatNumeric((double)constant.Value) + "d";
                    return true;

                case NetStandardTypeCode.Int64:
                    translation = constant.Value + "L";
                    return true;

                case NetStandardTypeCode.Object:
                    if (IsType(constant, out translation) ||
                        IsFunc(constant, out translation) ||
                        IsDefault<Guid>(constant, out translation) ||
                        IsTimeSpan(constant, out translation))
                    {
                        return true;
                    }
                    break;

                case NetStandardTypeCode.Single:
                    translation = FormatNumeric((float)constant.Value) + "f";
                    return true;

                case NetStandardTypeCode.String:
                    var stringValue = (string)constant.Value;
                    translation = stringValue.IsComment() ? stringValue : $"\"{stringValue}\"";
                    return true;
            }

            translation = null;
            return false;
        }

        private static string TranslateDateTime(DateTime value)
        {
            var month = PadToTwoDigits(value.Month);
            var day = PadToTwoDigits(value.Day);
            var hour = PadToTwoDigits(value.Hour);
            var minute = PadToTwoDigits(value.Minute);
            var second = PadToTwoDigits(value.Second);

            if (value.Millisecond != 0)
            {
                return $"new DateTime({value.Year}, {month}, {day}, {hour}, {minute}, {second}, {value.Millisecond})";
            }

            if ((value.Hour == 0) && (value.Minute == 0) && (value.Second == 0))
            {
                return $"new DateTime({value.Year}, {month}, {day})";
            }

            return $"new DateTime({value.Year}, {month}, {day}, {hour}, {minute}, {second})";
        }

        private static string PadToTwoDigits(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
        }

        private static bool IsTimeSpan(ConstantExpression constant, out string translation)
        {
            if (constant.Type != typeof(TimeSpan))
            {
                translation = null;
                return false;
            }

            if (IsDefault<TimeSpan>(constant, out translation))
            {
                return true;
            }

            var timeSpan = (TimeSpan)constant.Value;

            if (TryGetFactoryMethodCall(timeSpan.Days, timeSpan.TotalDays, "Days", out translation))
            {
                return true;
            }

            if (TryGetFactoryMethodCall(timeSpan.Hours, timeSpan.TotalHours, "Hours", out translation))
            {
                return true;
            }

            if (TryGetFactoryMethodCall(timeSpan.Minutes, timeSpan.TotalMinutes, "Minutes", out translation))
            {
                return true;
            }

            if (TryGetFactoryMethodCall(timeSpan.Seconds, timeSpan.TotalSeconds, "Seconds", out translation))
            {
                return true;
            }

            if (TryGetFactoryMethodCall(timeSpan.Milliseconds, timeSpan.TotalMilliseconds, "Milliseconds", out translation))
            {
                return true;
            }

            if (timeSpan.Days != 0)
            {
                if (timeSpan.Milliseconds != 0)
                {
                    translation = $"new TimeSpan({timeSpan.Days}, {timeSpan.Hours}, {timeSpan.Minutes}, {timeSpan.Seconds}, {timeSpan.Milliseconds})";
                    return true;
                }

                translation = $"new TimeSpan({timeSpan.Days}, {timeSpan.Hours}, {timeSpan.Minutes}, {timeSpan.Seconds})";
                return true;
            }

            if ((timeSpan.Hours > 0) || (timeSpan.Minutes > 0) || (timeSpan.Seconds > 0))
            {
                translation = $"new TimeSpan({timeSpan.Hours}, {timeSpan.Minutes}, {timeSpan.Seconds})";
                return true;
            }

            translation = $"TimeSpan.FromTicks({Math.Floor(timeSpan.TotalMilliseconds * 10000)})";
            return true;
        }

        private static bool TryGetFactoryMethodCall(
            long value,
            double totalValue,
            string valueName,
            out string translation)
        {
            if (value != 0)
            {
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (value == totalValue)
                {
                    translation = "TimeSpan.From" + valueName + "(" + value + ")";
                    return true;
                }
                // ReSharper restore CompareOfFloatsByEqualityOperator
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