namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Text.RegularExpressions;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
    using LinqLambda = System.Linq.Expressions.LambdaExpression;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using Extensions;
    using Interfaces;
    using NetStandardPolyfills;
    using static System.Globalization.CultureInfo;

    internal static class ConstantTranslation
    {
        public static ITranslation For(ConstantExpression constant, ITranslationContext context)
        {
            if (constant.Value == null)
            {
                return FixedValueTranslation("null", constant.Type);
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
                return FixedValueTranslation(constant.Value, valueType);
            }

            return context.GetTranslationFor(valueType).WithNodeType(Constant);
        }

        private static ITranslation FixedValueTranslation(ConstantExpression constant) => FixedValueTranslation(constant.Value, constant.Type);

        private static ITranslation FixedValueTranslation(object value, Type type) => FixedValueTranslation(value.ToString(), type);

        private static ITranslation FixedValueTranslation(string value, Type type, bool isTerminated = false)
        {
            return isTerminated
                ? new FixedTerminatedValueTranslation(Constant, value, type)
                : new FixedValueTranslation(Constant, value, type);
        }

        private static bool TryTranslateFromTypeCode(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            switch ((Nullable.GetUnderlyingType(constant.Type) ?? constant.Type).GetTypeCode())
            {
                case NetStandardTypeCode.Boolean:
                    translation = FixedValueTranslation(constant.Value.ToString().ToLowerInvariant(), constant.Type);
                    return true;

                case NetStandardTypeCode.Char:
                    translation = new TranslationWrapper(FixedValueTranslation(constant)).WrappedWith("'", "'");

                    return true;

                case NetStandardTypeCode.DateTime:
                    if (!TryTranslateDefault<DateTime>(constant, out translation))
                    {
                        translation = new DateTimeConstantTranslation(constant);
                    }

                    return true;

                case NetStandardTypeCode.DBNull:
                    translation = FixedValueTranslation("DBNull.Value", constant.Type);
                    return true;

                case NetStandardTypeCode.Decimal:
                    translation = GetDecimalTranslation(constant);
                    return true;

                case NetStandardTypeCode.Double:
                    translation = GetDoubleTranslation(constant);
                    return true;

                case NetStandardTypeCode.Int64:
                    translation = new TranslationWrapper(FixedValueTranslation(constant)).WithSuffix("L");
                    return true;

                case NetStandardTypeCode.Int32:
                    translation = FixedValueTranslation(constant);
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
                    translation = GetFloatTranslation(constant);
                    return true;

                case NetStandardTypeCode.String:
                    var stringValue = (string)constant.Value;

                    if (stringValue.IsComment())
                    {
                        translation = FixedValueTranslation(stringValue, typeof(string), isTerminated: true);
                        return true;
                    }

                    translation = FixedValueTranslation(stringValue.Replace("\"", "\\\""), typeof(string));
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

            translation = new TranslationWrapper(FixedValueTranslation(typeof(T).Name, typeof(T))).WrappedWith("default(", ")");
            return true;
        }

        private static ITranslation GetDecimalTranslation(ConstantExpression constant)
        {
            var value = (decimal)constant.Value;

            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture), constant.Type);

            return new TranslationWrapper(valueTranslation).WithSuffix("m");
        }

        private static ITranslation GetDoubleTranslation(ConstantExpression constant)
        {
            var value = (double)constant.Value;

            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture), constant.Type);

            return new TranslationWrapper(valueTranslation).WithSuffix("d");
        }

        private static ITranslation GetFloatTranslation(ConstantExpression constant)
        {
            var value = (float)constant.Value;

            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture), constant.Type);

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

            translation = FixedValueTranslation(constant);
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

            public Type Type => _typeNameTranslation.Type;

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                _typeNameTranslation.WriteTo(buffer);
                buffer.WriteToTranslation('.');
                buffer.WriteToTranslation(_enumValue);
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
                Type = constant.Type;
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

            public Type Type { get; }

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                buffer.WriteToTranslation(_newDateTime);

                buffer.WriteToTranslation(_value.Year);
                WriteTwoDigitDatePart(_value.Month, buffer);
                WriteTwoDigitDatePart(_value.Day, buffer);

                if (_hasMilliseconds || _hasTime)
                {
                    WriteTwoDigitDatePart(_value.Hour, buffer);
                    WriteTwoDigitDatePart(_value.Minute, buffer);
                    WriteTwoDigitDatePart(_value.Second, buffer);

                    if (_hasMilliseconds)
                    {
                        buffer.WriteToTranslation(", ");
                        buffer.WriteToTranslation(_value.Millisecond);
                    }
                }

                buffer.WriteToTranslation(")");
            }

            private static void WriteTwoDigitDatePart(int datePart, TranslationBuffer buffer)
            {
                buffer.WriteToTranslation(", ");

                if (datePart > 9)
                {
                    buffer.WriteToTranslation(datePart);
                    return;
                }

                buffer.WriteToTranslation('0');
                buffer.WriteToTranslation(datePart);
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
                if (constant.Value is LinqLambda linqLambda)
                {
                    var convertedLambda = LinqExpressionToDlrExpressionConverter.Convert(linqLambda);
                    lambdaTranslation = new LambdaConstantTranslation(convertedLambda, context);
                    return true;
                }
#endif
                if (constant.Value is LambdaExpression lambda)
                {
                    lambdaTranslation = new LambdaConstantTranslation(lambda, context);
                    return true;
                }

                lambdaTranslation = null;
                return false;
            }

            public ExpressionType NodeType => Constant;

            public Type Type => _lambdaTranslation.Type;

            public int EstimatedSize => _lambdaTranslation.EstimatedSize;

            public bool IsTerminated => true;

            public void WriteTo(TranslationBuffer buffer) => _lambdaTranslation.WriteTo(buffer);
        }

        private class FuncConstantTranslation : ITranslation
        {
            private static readonly Regex _funcMatcher = new Regex(@"^System\.(?:Func|Action)`\d+\[.+\]$");

            private readonly string _funcString;

            private FuncConstantTranslation(Type funcType, string funcString)
            {
                Type = funcType;
                _funcString = funcString;
                EstimatedSize = funcString.Length;
            }

            public static bool TryCreate(ConstantExpression constant, out ITranslation funcTranslation)
            {
                var match = _funcMatcher.Match(constant.Value.ToString());

                if (match.Success)
                {
                    funcTranslation = new FuncConstantTranslation(constant.Type, match.Value);
                    return true;
                }

                funcTranslation = null;
                return false;
            }

            public ExpressionType NodeType => Constant;

            public Type Type { get; }

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
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
                            buffer.WriteToTranslation(GetTypeName(substring));
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

                            buffer.WriteToTranslation(GetTypeName(substring));
                            buffer.WriteToTranslation('?');
                            parseIndex = nullableTypeEndIndex + 1;
                            continue;
                        }

                        buffer.WriteToTranslation(GetTypeName(substring));
                        parseIndex = nextSymbolIndex + 1;
                    }

                    switch (_funcString[nextSymbolIndex])
                    {
                        case '`':
                            buffer.WriteToTranslation('<');
                            parseIndex = _funcString.IndexOf('[', parseIndex) + 1;
                            continue;

                        case ',':
                            buffer.WriteToTranslation(", ");

                            if (substringLength == 0)
                            {
                                ++parseIndex;
                            }

                            continue;

                        case '[':
                            buffer.WriteToTranslation("[]");
                            ++parseIndex;
                            continue;

                        case ']':
                            buffer.WriteToTranslation('>');

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

            private TimeSpanConstantTranslation(ConstantExpression timeSpanConstant)
            {
                Type = timeSpanConstant.Type;
                _timeSpan = (TimeSpan)timeSpanConstant.Value;
                EstimatedSize = _timeSpan.ToString().Length;
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

                timeSpanTranslation = new TimeSpanConstantTranslation(constant);
                return true;
            }

            public ExpressionType NodeType => Constant;

            public Type Type { get; }

            public int EstimatedSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                if (TryWriteFactoryMethodCall(_timeSpan.Days, _timeSpan.TotalDays, "Days", buffer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Hours, _timeSpan.TotalHours, "Hours", buffer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Minutes, _timeSpan.TotalMinutes, "Minutes", buffer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Seconds, _timeSpan.TotalSeconds, "Seconds", buffer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Milliseconds, _timeSpan.TotalMilliseconds, "Milliseconds", buffer))
                {
                    return;
                }

                if ((_timeSpan.Days == 0) && (_timeSpan.Hours == 0) && (_timeSpan.Minutes == 0) && (_timeSpan.Seconds == 0))
                {
                    buffer.WriteToTranslation("TimeSpan.FromTicks(");
                    buffer.WriteToTranslation(Math.Floor(_timeSpan.TotalMilliseconds * 10000).ToString(CurrentCulture));
                    goto EndTranslation;
                }

                buffer.WriteToTranslation("new TimeSpan(");

                if (_timeSpan.Days == 0)
                {
                    WriteTimeSpanHoursMinutesSeconds(buffer, _timeSpan);
                    goto EndTranslation;
                }

                buffer.WriteToTranslation(_timeSpan.Days);
                buffer.WriteToTranslation(", ");
                WriteTimeSpanHoursMinutesSeconds(buffer, _timeSpan);

                if (_timeSpan.Milliseconds != 0)
                {
                    buffer.WriteToTranslation(", ");
                    buffer.WriteToTranslation(_timeSpan.Milliseconds);
                }

                EndTranslation:
                buffer.WriteToTranslation(')');
            }

            private static void WriteTimeSpanHoursMinutesSeconds(TranslationBuffer buffer, TimeSpan timeSpan)
            {
                buffer.WriteToTranslation(timeSpan.Hours);
                buffer.WriteToTranslation(", ");
                buffer.WriteToTranslation(timeSpan.Minutes);
                buffer.WriteToTranslation(", ");
                buffer.WriteToTranslation(timeSpan.Seconds);
            }

            private static bool TryWriteFactoryMethodCall(
                long value,
                double totalValue,
                string valueName,
                TranslationBuffer buffer)
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

                buffer.WriteToTranslation("TimeSpan.From");
                buffer.WriteToTranslation(valueName);
                buffer.WriteToTranslation('(');
                buffer.WriteToTranslation(value);
                buffer.WriteToTranslation(')');
                return true;
            }
        }
    }
}