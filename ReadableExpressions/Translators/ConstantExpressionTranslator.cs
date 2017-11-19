namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using System.Text.RegularExpressions;
    using Extensions;
    using NetStandardPolyfills;

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

            if (TryTranslateByTypeCode(constant, context, out var translation))
            {
                return translation;
            }

            return constant.Value.ToString();
        }

        private static bool TryTranslateByTypeCode(
            ConstantExpression constant,
            TranslationContext context,
            out string translation)
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
                        IsLambda(constant, context, out translation) ||
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
            return (value % 1).Equals(0) ? value.ToString("0") : value.ToString(CultureInfo.CurrentCulture);
        }

        private static string FormatNumeric(double value)
        {
            return (value % 1).Equals(0) ? value.ToString("0") : value.ToString(CultureInfo.CurrentCulture);
        }

        private static string FormatNumeric(float value)
        {
            return (value % 1).Equals(0) ? value.ToString("0") : value.ToString(CultureInfo.CurrentCulture);
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

        private static bool IsLambda(
            ConstantExpression constant,
            TranslationContext context,
            out string translation)
        {
            if (constant.Value is LambdaExpression lambda)
            {
                translation = context.TranslateAsCodeBlock(lambda);
                return true;
            }

            translation = null;
            return false;
        }

        private static readonly Regex _funcMatcher = new Regex(@"^System\.(?:Func|Action)`\d+\[.+\]$");

        private static bool IsFunc(ConstantExpression constant, out string translation)
        {
            var match = _funcMatcher.Match(constant.Value.ToString());

            if (match.Success)
            {
                translation = ParseFuncFrom(match.Value);
                return true;
            }

            translation = null;
            return false;
        }

        #region IsFunc Helpers

        private static string ParseFuncFrom(string funcString)
        {
            var symbols = new[] { '`', ',', '[', ']' };
            var parseIndex = 0;
            var parsedFunc = string.Empty;

            while (true)
            {
                var nextSymbolIndex = funcString.IndexOfAny(symbols, parseIndex);

                if (nextSymbolIndex == -1)
                {
                    var substring = funcString.Substring(parseIndex);

                    if (substring.Length > 0)
                    {
                        var typeName = GetTypeName(substring);

                        parsedFunc += typeName;
                    }
                    break;
                }

                var substringLength = nextSymbolIndex - parseIndex;

                if (substringLength > 0)
                {
                    var substring = funcString.Substring(parseIndex, substringLength);

                    if (substring == "System.Nullable")
                    {
                        var nullableTypeIndex = parseIndex + "System.Nullable`1[".Length;
                        var nullableTypeEndIndex = funcString.IndexOf(']', nullableTypeIndex);
                        var nullableTypeLength = nullableTypeEndIndex - nullableTypeIndex;
                        substring = funcString.Substring(nullableTypeIndex, nullableTypeLength);

                        var typeName = GetTypeName(substring);

                        parsedFunc += typeName + "?";
                        parseIndex = nullableTypeEndIndex + 1;
                        continue;
                    }
                    else
                    {
                        var typeName = GetTypeName(substring);

                        parsedFunc += typeName;
                        parseIndex = nextSymbolIndex + 1;
                    }
                }

                switch (funcString[nextSymbolIndex])
                {
                    case '`':
                        parsedFunc += "<";
                        parseIndex = funcString.IndexOf('[', parseIndex) + 1;
                        continue;

                    case ',':
                        parsedFunc += ", ";

                        if (substringLength == 0)
                        {
                            ++parseIndex;
                        }
                        continue;

                    case '[':
                        parsedFunc += "[]";
                        ++parseIndex;
                        continue;

                    case ']':
                        parsedFunc += ">";

                        if (substringLength == 0)
                        {
                            ++parseIndex;
                        }
                        break;
                }
            }

            return parsedFunc;
        }

        private static string GetTypeName(string typeFullName)
        {
            return typeFullName.GetSubstitutionOrNull() ??
                   typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);
        }

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