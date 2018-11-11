namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Text.RegularExpressions;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
    using Translators;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using Extensions;
    using NetStandardPolyfills;
    using static System.Globalization.CultureInfo;

    internal static class ConstantTranslation
    {
        public static ITranslation For(ConstantExpression constant, ITranslationContext context)
        {
            if (constant.Value == null)
            {
                return FixedValueTranslation("null");
            }

            if (constant.Type.IsEnum())
            {
                return new EnumConstantTranslation(constant, context);
            }

            if (TryTranslateFromTypeCode(constant, context, out var translation))
            {
                return translation;
            }

            var valueType = constant.Value.GetType();

            if (valueType.IsPrimitive() || valueType.IsValueType())
            {
                return FixedValueTranslation(constant.Value);
            }

            return FixedValueTranslation(valueType.GetFriendlyName(context.Settings));
        }

        private static ITranslation FixedValueTranslation(object value) => FixedValueTranslation(value.ToString());

        private static ITranslation FixedValueTranslation(string value, bool isTerminated = false)
        {
            return isTerminated
                ? new FixedTerminatedValueTranslation(Constant, value)
                : new FixedValueTranslation(Constant, value);
        }

        private static bool TryTranslateFromTypeCode(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            switch ((Nullable.GetUnderlyingType(constant.Type) ?? constant.Type).GetTypeCode())
            {
                case NetStandardTypeCode.Boolean:
                    translation = FixedValueTranslation(constant.Value.ToString().ToLowerInvariant());
                    return true;

                case NetStandardTypeCode.Char:
                    translation = new TranslationWrapper(FixedValueTranslation(constant.Value)).WrappedWith("'", "'");

                    return true;

                case NetStandardTypeCode.DateTime:
                    if (!TryTranslateDefault<DateTime>(constant, out translation))
                    {
                        translation = new DateTimeConstantTranslation(constant);
                    }

                    return true;

                case NetStandardTypeCode.DBNull:
                    translation = FixedValueTranslation("DBNull.Value");
                    return true;

                case NetStandardTypeCode.Decimal:
                    translation = GetDecimalTranslation((decimal)constant.Value);
                    return true;

                case NetStandardTypeCode.Double:
                    translation = GetDoubleTranslation((double)constant.Value);
                    return true;

                case NetStandardTypeCode.Int64:
                    translation = new TranslationWrapper(FixedValueTranslation(constant.Value)).WithSuffix("L");
                    return true;

                case NetStandardTypeCode.Int32:
                    translation = FixedValueTranslation(constant.Value);
                    return true;

                case NetStandardTypeCode.Object:
                    if (TryGetTypeTranslation(constant, context, out translation) ||
                        LambdaConstantTranslation.TryCreate(constant, context, out translation) ||
                        FuncConstantTranslation.TryCreate(constant, out translation) ||
                        TryGetRegexTranslation(constant, out translation) ||
                        TryTranslateDefault<Guid>(constant, out translation) ||
                        TimeSpanConstantTranslation.TryCreate(constant, out translation))
                    {
                        return true;
                    }

                    break;

                case NetStandardTypeCode.Single:
                    translation = GetFloatTranslation((float)constant.Value);
                    return true;

                case NetStandardTypeCode.String:
                    var stringValue = (string)constant.Value;

                    if (stringValue.IsComment())
                    {
                        translation = FixedValueTranslation(stringValue, isTerminated: true);
                        return true;
                    }

                    translation = FixedValueTranslation(stringValue.Replace("\"", "\\\""));
                    translation = new TranslationWrapper(translation).WrappedWith("\"", "\"");

                    return true;
            }

            translation = null;
            return false;
        }

        private static bool TryTranslateDefault<T>(ConstantExpression constant, out ITranslation translation)
        {
            if ((constant.Type != typeof(T)) || !constant.Value.Equals(default(T)))
            {
                translation = null;
                return false;
            }

            translation = new TranslationWrapper(FixedValueTranslation(typeof(T).Name)).WrappedWith("default(", ")");
            return true;
        }

        private static ITranslation GetDecimalTranslation(decimal value)
        {
            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture));

            return new TranslationWrapper(valueTranslation).WithSuffix("m");
        }

        private static ITranslation GetDoubleTranslation(double value)
        {
            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture));

            return new TranslationWrapper(valueTranslation).WithSuffix("d");
        }

        private static ITranslation GetFloatTranslation(float value)
        {
            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture));

            return new TranslationWrapper(valueTranslation).WithSuffix("f");
        }

        private static bool TryGetTypeTranslation(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            if (!constant.Type.IsAssignableTo(typeof(Type)))
            {
                translation = null;
                return false;
            }

            translation = context.GetTranslationFor((Type)constant.Value);
            translation = new TranslationWrapper(translation).WrappedWith("typeof(", ")");
            return true;
        }

        private static bool TryGetRegexTranslation(ConstantExpression constant, out ITranslation translation)
        {
            if (constant.Type != typeof(Regex))
            {
                translation = null;
                return false;
            }

            translation = FixedValueTranslation(constant.Value);
            translation = new TranslationWrapper(translation).WrappedWith("Regex /* ", " */");
            return true;
        }

        private class EnumConstantTranslation : ITranslation
        {
            private readonly ITranslation _typeNameTranslation;
            private readonly string _enumValue;

            public EnumConstantTranslation(ConstantExpression constant, ITranslationContext context)
            {
                _typeNameTranslation = context.GetTranslationFor(constant.Type);
                _enumValue = constant.Value.ToString();
                EstimatedSize = _typeNameTranslation.EstimatedSize + 1 + _enumValue.Length;
            }

            public ExpressionType NodeType => Constant;

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _typeNameTranslation.WriteTo(context);
                context.WriteToTranslation('.');
                context.WriteToTranslation(_enumValue);
            }
        }

        private class DateTimeConstantTranslation : ITranslation
        {
            private const string _newDateTime = "new DateTime(";
            private readonly DateTime _value;
            private readonly bool _hasMilliseconds;
            private readonly bool _hasTime;

            public DateTimeConstantTranslation(ConstantExpression constant)
            {
                _value = (DateTime)constant.Value;
                _hasMilliseconds = _value.Millisecond != 0;
                _hasTime = (_value.Hour != 0) || (_value.Minute != 0) || (_value.Second != 0);
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                var estimatedSize = _newDateTime.Length + 4 + 4 + 4;

                if (_hasMilliseconds || _hasTime)
                {
                    estimatedSize += 4 + 4 + 4;

                    if (_hasMilliseconds)
                    {
                        estimatedSize += 2 + 4;
                    }
                }

                return estimatedSize + 1;
            }

            public ExpressionType NodeType => Constant;

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                context.WriteToTranslation(_newDateTime);

                context.WriteToTranslation(_value.Year);
                WriteTwoDigitDatePart(_value.Month, context);
                WriteTwoDigitDatePart(_value.Day, context);

                if (_hasMilliseconds || _hasTime)
                {
                    WriteTwoDigitDatePart(_value.Hour, context);
                    WriteTwoDigitDatePart(_value.Minute, context);
                    WriteTwoDigitDatePart(_value.Second, context);

                    if (_hasMilliseconds)
                    {
                        context.WriteToTranslation(", ");
                        context.WriteToTranslation(_value.Millisecond);
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
        }

        private class LambdaConstantTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
        {
            private readonly ITranslation _lambdaTranslation;

            private LambdaConstantTranslation(LambdaExpression lambda, ITranslationContext context)
            {
                _lambdaTranslation = context.GetCodeBlockTranslationFor(lambda);
            }

            public static bool TryCreate(
                ConstantExpression constant,
                ITranslationContext context,
                out ITranslation lambdaTranslation)
            {
#if NET35
            if (constant.Value is System.Linq.Expressions.LambdaExpression linqLambda)
            {
                var lambda = LinqExpressionToDlrExpressionConverter.Convert(linqLambda);
#else
                if (constant.Value is LambdaExpression lambda)
                {
#endif
                    lambdaTranslation = new LambdaConstantTranslation(lambda, context);
                    return true;
                }

                lambdaTranslation = null;
                return false;
            }

            public ExpressionType NodeType => Constant;

            public int EstimatedSize => _lambdaTranslation.EstimatedSize;

            public bool IsTerminated => true;

            public void WriteTo(ITranslationContext context) => _lambdaTranslation.WriteTo(context);
        }

        private class FuncConstantTranslation : ITranslation
        {
            private static readonly Regex _funcMatcher = new Regex(@"^System\.(?:Func|Action)`\d+\[.+\]$");

            private readonly string _funcString;

            private FuncConstantTranslation(string funcString)
            {
                _funcString = funcString;
                EstimatedSize = funcString.Length;
            }

            public static bool TryCreate(ConstantExpression constant, out ITranslation funcTranslation)
            {
                var match = _funcMatcher.Match(constant.Value.ToString());

                if (match.Success)
                {
                    funcTranslation = new FuncConstantTranslation(match.Value);
                    return true;
                }

                funcTranslation = null;
                return false;
            }

            public ExpressionType NodeType => Constant;

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                var symbols = new[] { '`', ',', '[', ']' };
                var parseIndex = 0;

                while (true)
                {
                    var nextSymbolIndex = _funcString.IndexOfAny(symbols, parseIndex);

                    if (nextSymbolIndex == -1)
                    {
                        var substring = _funcString.Substring(parseIndex);

                        if (substring.Length > 0)
                        {
                            context.WriteToTranslation(GetTypeName(substring));
                        }

                        break;
                    }

                    var substringLength = nextSymbolIndex - parseIndex;

                    if (substringLength > 0)
                    {
                        var substring = _funcString.Substring(parseIndex, substringLength);

                        if (substring == "System.Nullable")
                        {
                            var nullableTypeIndex = parseIndex + "System.Nullable`1[".Length;
                            var nullableTypeEndIndex = _funcString.IndexOf(']', nullableTypeIndex);
                            var nullableTypeLength = nullableTypeEndIndex - nullableTypeIndex;
                            substring = _funcString.Substring(nullableTypeIndex, nullableTypeLength);

                            context.WriteToTranslation(GetTypeName(substring));
                            context.WriteToTranslation('?');
                            parseIndex = nullableTypeEndIndex + 1;
                            continue;
                        }

                        context.WriteToTranslation(GetTypeName(substring));
                        parseIndex = nextSymbolIndex + 1;
                    }

                    switch (_funcString[nextSymbolIndex])
                    {
                        case '`':
                            context.WriteToTranslation('<');
                            parseIndex = _funcString.IndexOf('[', parseIndex) + 1;
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
        }

        private class TimeSpanConstantTranslation : ITranslation
        {
            private readonly TimeSpan _timeSpan;

            private TimeSpanConstantTranslation(TimeSpan timeSpan)
            {
                _timeSpan = timeSpan;
                EstimatedSize = timeSpan.ToString().Length;
            }

            public static bool TryCreate(ConstantExpression constant, out ITranslation timeSpanTranslation)
            {
                if (constant.Type != typeof(TimeSpan))
                {
                    timeSpanTranslation = null;
                    return false;
                }

                if (TryTranslateDefault<TimeSpan>(constant, out timeSpanTranslation))
                {
                    return true;
                }

                timeSpanTranslation = new TimeSpanConstantTranslation((TimeSpan)constant.Value);
                return true;
            }

            public ExpressionType NodeType => Constant;

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                if (TryWriteFactoryMethodCall(_timeSpan.Days, _timeSpan.TotalDays, "Days", context))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Hours, _timeSpan.TotalHours, "Hours", context))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Minutes, _timeSpan.TotalMinutes, "Minutes", context))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Seconds, _timeSpan.TotalSeconds, "Seconds", context))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Milliseconds, _timeSpan.TotalMilliseconds, "Milliseconds", context))
                {
                    return;
                }

                if ((_timeSpan.Days == 0) && (_timeSpan.Hours == 0) && (_timeSpan.Minutes == 0) && (_timeSpan.Seconds == 0))
                {
                    context.WriteToTranslation("TimeSpan.FromTicks(");
                    context.WriteToTranslation(Math.Floor(_timeSpan.TotalMilliseconds * 10000).ToString(CurrentCulture));
                    goto EndTranslation;
                }

                context.WriteToTranslation("new TimeSpan(");

                if (_timeSpan.Days == 0)
                {
                    WriteTimeSpanHoursMinutesSeconds(context, _timeSpan);
                    goto EndTranslation;
                }

                context.WriteToTranslation(_timeSpan.Days);
                context.WriteToTranslation(", ");
                WriteTimeSpanHoursMinutesSeconds(context, _timeSpan);

                if (_timeSpan.Milliseconds != 0)
                {
                    context.WriteToTranslation(", ");
                    context.WriteToTranslation(_timeSpan.Milliseconds);
                }

                EndTranslation:
                context.WriteToTranslation(')');
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
        }
    }
}