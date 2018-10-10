namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Text.RegularExpressions;
#if NET35
    using Microsoft.Scripting.Ast;
    using Translators;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using NetStandardPolyfills;
    using static System.Globalization.CultureInfo;

    internal class ConstantTranslation : ITranslation
    {
        private const string _null = "null";

        private readonly ConstantExpression _constant;
        private readonly bool _isEnumValue;
        private readonly ITranslation _typeNameTranslation;

        public ConstantTranslation(ConstantExpression constant, ITranslationContext context)
        {
            _constant = constant;
            _isEnumValue = constant.Type.IsEnum();

            if (_isEnumValue)
            {
                _typeNameTranslation = context.GetTranslationFor(constant.Type);
            }

            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize()
        {
            if (_constant.Value == null)
            {
                return _null.Length;
            }

            // TODO: Update to use lambda translation.

            var valueString = _constant.Value.ToString();

            if (_isEnumValue)
            {
                return _typeNameTranslation.EstimatedSize + ".".Length + valueString.Length;
            }

            return valueString.Length;
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_constant.Value == null)
            {
                context.WriteToTranslation(_null);
                return;
            }

            if (_isEnumValue)
            {
                _typeNameTranslation.WriteTo(context);
                context.WriteToTranslation('.');
                context.WriteToTranslation(_constant.Value);
                return;
            }

            if (TryWriteFromTypeCode(context))
            {
                return;
            }

            var valueType = _constant.Value.GetType();

            if (valueType.IsPrimitive() || valueType.IsValueType())
            {
                context.WriteToTranslation(_constant.Value);
                return;
            }

            context.WriteToTranslation(valueType.GetFriendlyName(context.Settings));
        }

        private bool TryWriteFromTypeCode(ITranslationContext context)
        {
            switch ((Nullable.GetUnderlyingType(_constant.Type) ?? _constant.Type).GetTypeCode())
            {
                case NetStandardTypeCode.Boolean:
                    context.WriteToTranslation(_constant.Value.ToString().ToLowerInvariant());
                    return true;

                case NetStandardTypeCode.Char:
                    context.WriteToTranslation('\'');
                    context.WriteToTranslation(_constant.Value);
                    context.WriteToTranslation('\'');
                    return true;

                case NetStandardTypeCode.DateTime:
                    if (!TryWriteDefault<DateTime>(context))
                    {
                        WriteDateTime((DateTime)_constant.Value, context);
                    }

                    return true;

                case NetStandardTypeCode.DBNull:
                    context.WriteToTranslation("DBNull.Value");
                    return true;

                case NetStandardTypeCode.Decimal:
                    WriteNumeric((decimal)_constant.Value, 'm', context);
                    return true;

                case NetStandardTypeCode.Double:
                    WriteNumeric((double)_constant.Value, 'd', context);
                    return true;

                case NetStandardTypeCode.Int64:
                    context.WriteToTranslation(_constant.Value);
                    context.WriteToTranslation('L');
                    return true;

                case NetStandardTypeCode.Int32:
                    context.WriteToTranslation((int)_constant.Value);
                    return true;

                case NetStandardTypeCode.Object:
                    if (TryWriteType(context) ||
                        TryWriteLambda(context) ||
                        TryWriteFunc(context) ||
                        TryWriteRegex(context) ||
                        TryWriteDefault<Guid>(context) ||
                        TryWriteTimeSpan(context))
                    {
                        return true;
                    }
                    break;

                case NetStandardTypeCode.Single:
                    WriteNumeric((float)_constant.Value, 'f', context);
                    return true;

                case NetStandardTypeCode.String:
                    var stringValue = (string)_constant.Value;

                    if (stringValue.IsComment())
                    {
                        context.WriteToTranslation(stringValue);
                        return true;
                    }

                    context.WriteToTranslation('"');
                    context.WriteToTranslation(stringValue.Replace("\"", "\\\""));
                    context.WriteToTranslation('"');
                    return true;
            }

            return false;
        }

        private static void WriteDateTime(DateTime value, ITranslationContext context)
        {
            var hasMilliseconds = value.Millisecond != 0;
            var hasTime = (value.Hour != 0) || (value.Minute != 0) || (value.Second != 0);

            context.WriteToTranslation("new DateTime(");

            context.WriteToTranslation(value.Year);
            WriteTwoDigitDatePart(value.Month, context);
            WriteTwoDigitDatePart(value.Day, context);

            if (hasMilliseconds || hasTime)
            {
                WriteTwoDigitDatePart(value.Hour, context);
                WriteTwoDigitDatePart(value.Minute, context);
                WriteTwoDigitDatePart(value.Second, context);

                if (hasMilliseconds)
                {
                    context.WriteToTranslation(", ");
                    context.WriteToTranslation(value.Millisecond);
                }
            }

            context.WriteToTranslation(")");
        }

        private static void WriteTwoDigitDatePart(int datePart, ITranslationContext context)
        {
            context.WriteToTranslation(", ");

            if (datePart > 9)
            {
                context.WriteToTranslation(datePart);
                return;
            }

            context.WriteToTranslation('0');
            context.WriteToTranslation(datePart);
        }

        private static void WriteNumeric(decimal value, char suffix, ITranslationContext context)
        {
            context.WriteToTranslation((value % 1).Equals(0)
                ? value.ToString("0") : value.ToString(CurrentCulture));

            context.WriteToTranslation(suffix);
        }

        private static void WriteNumeric(double value, char suffix, ITranslationContext context)
        {
            context.WriteToTranslation((value % 1).Equals(0)
                ? value.ToString("0") : value.ToString(CurrentCulture));

            context.WriteToTranslation(suffix);
        }

        private static void WriteNumeric(float value, char suffix, ITranslationContext context)
        {
            context.WriteToTranslation((value % 1).Equals(0)
                ? value.ToString("0") : value.ToString(CurrentCulture));

            context.WriteToTranslation(suffix);
        }

        private bool TryWriteType(ITranslationContext context)
        {
            if (!_constant.Type.IsAssignableTo(typeof(Type)))
            {
                return false;
            }

            context.WriteToTranslation("typeof(");
            context.GetTranslationFor((Type)_constant.Value).WriteTo(context);
            context.WriteToTranslation(")");
            return true;
        }

        private bool TryWriteLambda(ITranslationContext context)
        {
#if NET35
            if (_constant.Value is System.Linq.Expressions.LambdaExpression linqLambda)
            {
                var lambda = LinqExpressionToDlrExpressionConverter.Convert(linqLambda);
#else
            if (_constant.Value is LambdaExpression lambda)
            {
#endif
                context.WriteCodeBlockToTranslation(context.GetTranslationFor(lambda));
                return true;
            }

            return false;
        }

        private static readonly Regex _funcMatcher = new Regex(@"^System\.(?:Func|Action)`\d+\[.+\]$");

        private bool TryWriteFunc(ITranslationContext context)
        {
            var match = _funcMatcher.Match(_constant.Value.ToString());

            if (match.Success == false)
            {
                return false;
            }

            WriteFuncFrom(match.Value, context);
            return true;

        }

        #region IsFunc Helpers

        private static void WriteFuncFrom(string funcString, ITranslationContext context)
        {
            var symbols = new[] { '`', ',', '[', ']' };
            var parseIndex = 0;

            while (true)
            {
                var nextSymbolIndex = funcString.IndexOfAny(symbols, parseIndex);

                if (nextSymbolIndex == -1)
                {
                    var substring = funcString.Substring(parseIndex);

                    if (substring.Length > 0)
                    {
                        context.WriteToTranslation(GetTypeName(substring));
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

                        context.WriteToTranslation(GetTypeName(substring));
                        context.WriteToTranslation('?');
                        parseIndex = nullableTypeEndIndex + 1;
                        continue;
                    }
                    else
                    {
                        context.WriteToTranslation(GetTypeName(substring));
                        parseIndex = nextSymbolIndex + 1;
                    }
                }

                switch (funcString[nextSymbolIndex])
                {
                    case '`':
                        context.WriteToTranslation('<');
                        parseIndex = funcString.IndexOf('[', parseIndex) + 1;
                        continue;

                    case ',':
                        context.WriteToTranslation(", ");

                        if (substringLength == 0)
                        {
                            ++parseIndex;
                        }

                        continue;

                    case '[':
                        context.WriteToTranslation("[]");
                        ++parseIndex;
                        continue;

                    case ']':
                        context.WriteToTranslation('>');

                        if (substringLength == 0)
                        {
                            ++parseIndex;
                        }

                        break;
                }
            }
        }

        private static string GetTypeName(string typeFullName)
        {
            return typeFullName.GetSubstitutionOrNull() ??
                   typeFullName.Substring(typeFullName.LastIndexOf('.') + 1);
        }

        #endregion

        private bool TryWriteRegex(ITranslationContext context)
        {
            if (_constant.Type != typeof(Regex))
            {
                return false;
            }

            context.WriteToTranslation("Regex /* ");
            context.WriteToTranslation(_constant.Value);
            context.WriteToTranslation(" */");
            return true;
        }

        private bool TryWriteTimeSpan(ITranslationContext context)
        {
            if (_constant.Type != typeof(TimeSpan))
            {
                return false;
            }

            if (TryWriteDefault<TimeSpan>(context))
            {
                return true;
            }

            var timeSpan = (TimeSpan)_constant.Value;

            if (TryWriteFactoryMethodCall(timeSpan.Days, timeSpan.TotalDays, "Days", context))
            {
                return true;
            }

            if (TryWriteFactoryMethodCall(timeSpan.Hours, timeSpan.TotalHours, "Hours", context))
            {
                return true;
            }

            if (TryWriteFactoryMethodCall(timeSpan.Minutes, timeSpan.TotalMinutes, "Minutes", context))
            {
                return true;
            }

            if (TryWriteFactoryMethodCall(timeSpan.Seconds, timeSpan.TotalSeconds, "Seconds", context))
            {
                return true;
            }

            if (TryWriteFactoryMethodCall(timeSpan.Milliseconds, timeSpan.TotalMilliseconds, "Milliseconds", context))
            {
                return true;
            }


            if ((timeSpan.Days == 0) && (timeSpan.Hours == 0) && (timeSpan.Minutes == 0) && (timeSpan.Seconds == 0))
            {
                context.WriteToTranslation("TimeSpan.FromTicks(");
                context.WriteToTranslation(Math.Floor(timeSpan.TotalMilliseconds * 10000).ToString(CurrentCulture));
                goto EndTranslation;
            }

            context.WriteToTranslation("new TimeSpan(");

            if (timeSpan.Days == 0)
            {
                WriteTimeSpanHoursMinutesSeconds(context, timeSpan);
                goto EndTranslation;
            }

            context.WriteToTranslation(timeSpan.Days);
            context.WriteToTranslation(", ");
            WriteTimeSpanHoursMinutesSeconds(context, timeSpan);

            if (timeSpan.Milliseconds != 0)
            {
                context.WriteToTranslation(", ");
                context.WriteToTranslation(timeSpan.Milliseconds);
            }

            EndTranslation:
            context.WriteToTranslation(')');
            return true;
        }

        private static void WriteTimeSpanHoursMinutesSeconds(ITranslationContext context, TimeSpan timeSpan)
        {
            context.WriteToTranslation(timeSpan.Hours);
            context.WriteToTranslation(", ");
            context.WriteToTranslation(timeSpan.Minutes);
            context.WriteToTranslation(", ");
            context.WriteToTranslation(timeSpan.Seconds);
        }

        private static bool TryWriteFactoryMethodCall(
            long value,
            double totalValue,
            string valueName,
            ITranslationContext context)
        {
            if (value == 0)
            {
                return false;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value != totalValue)
            {
                return false;
            }

            context.WriteToTranslation("TimeSpan.From");
            context.WriteToTranslation(valueName);
            context.WriteToTranslation('(');
            context.WriteToTranslation(value);
            context.WriteToTranslation(')');
            return true;
        }

        private bool TryWriteDefault<T>(ITranslationContext context)
        {
            if ((_constant.Type != typeof(T)) || !_constant.Value.Equals(default(T)))
            {
                return false;
            }

            context.WriteToTranslation("default(");
            context.WriteToTranslation(typeof(T).Name);
            context.WriteToTranslation(")");
            return true;

        }
    }
}