﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Text.RegularExpressions;
    using Extensions;
    using Formatting;
    using Interfaces;
    using NetStandardPolyfills;
    using static System.Globalization.CultureInfo;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif
#if NET35
    using LinqLambda = System.Linq.Expressions.LambdaExpression;
#endif
    using static Formatting.TokenType;

    internal static class ConstantTranslation
    {
        public static ITranslation For(ConstantExpression constant, ITranslationContext context)
        {
            if (context.Settings.ConstantExpressionValueFactory != null)
            {
                var userTranslation = context.Settings.ConstantExpressionValueFactory(constant.Type, constant.Value);

                return (userTranslation == null)
                    ? NullTranslation(constant.Type)
                    : FixedValueTranslation(userTranslation, constant.Type);
            }

            if (constant.Value == null)
            {
                return NullTranslation(constant.Type);
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

        private static ITranslation FixedValueTranslation(ConstantExpression constant, TokenType tokenType = TokenType.Default)
            => FixedValueTranslation(constant.Value, constant.Type, tokenType);

        private static ITranslation FixedValueTranslation(object value, Type type, TokenType tokenType = TokenType.Default)
            => FixedValueTranslation(value.ToString(), type, tokenType);

        private static ITranslation NullTranslation(Type type)
            => FixedValueTranslation("null", type, Keyword);

        private static ITranslation FixedValueTranslation(
            string value,
            Type type,
            TokenType tokenType)
        {
            return new FixedValueTranslation(Constant, value, type, tokenType);
        }

        private static ITranslation FixedTerminatedValueTranslation(
            string value,
            Type type,
            TokenType tokenType)
        {
            return new FixedTerminatedValueTranslation(Constant, value, type, tokenType);
        }

        private static ITranslation FixedValuesTranslation(
            Type type,
            int estimatedSize,
            params Action<TranslationBuffer>[] valueWriters)
        {
            return new FixedValuesTranslation(Constant, type, estimatedSize, valueWriters);
        }

        private static bool TryTranslateFromTypeCode(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            var type = constant.Type;

            switch ((Nullable.GetUnderlyingType(type) ?? type).GetTypeCode())
            {
                case NetStandardTypeCode.Boolean:
                    translation = FixedValueTranslation(
                        constant.Value.ToString().ToLowerInvariant(),
                        type,
                        Keyword);

                    return true;

                case NetStandardTypeCode.Char:
                    var character = (char)constant.Value;
                    var value = "'" + (character == '\0' ? @"\0" : character.ToString()) + "'";
                    translation = FixedValueTranslation(value, type, Text);
                    return true;

                case NetStandardTypeCode.DateTime:
                    if (!TryTranslateDefault<DateTime>(constant, out translation))
                    {
                        translation = new DateTimeConstantTranslation(constant);
                    }

                    return true;

                case NetStandardTypeCode.DBNull:
                    translation = GetDbNullTranslation(constant);
                    return true;

                case NetStandardTypeCode.Decimal:
                    translation = GetDecimalTranslation(constant);
                    return true;

                case NetStandardTypeCode.Double:
                    translation = GetDoubleTranslation(constant);
                    return true;

                case NetStandardTypeCode.Int64:
                    translation = FixedValueTranslation(constant, Numeric).WithSuffix("L", Numeric);
                    return true;

                case NetStandardTypeCode.Int32:
                    translation = FixedValueTranslation(constant, Numeric);
                    return true;

                case NetStandardTypeCode.Object:
                    if (TryGetTypeTranslation(constant, context, out translation) ||
                        LambdaConstantTranslation.TryCreate(constant, context, out translation) ||
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
                    var stringValue = ((string)constant.Value).Replace("\0", @"\0");

                    if (stringValue.IsComment())
                    {
                        translation = FixedTerminatedValueTranslation(stringValue, typeof(string), Comment);
                        return true;
                    }

                    stringValue = "\"" + stringValue.Replace("\"", "\\\"") + "\"";
                    translation = FixedValueTranslation(stringValue, typeof(string), Text);
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

        private static ITranslation GetDbNullTranslation(Expression constant)
        {
            return FixedValuesTranslation(
                constant.Type,
                "DBNull.Value".Length,
                b => b.WriteTypeNameToTranslation("DBNull"),
                b => b.WriteDotToTranslation(),
                b => b.WriteToTranslation("Value"));
        }

        private static ITranslation GetDecimalTranslation(ConstantExpression constant)
        {
            var value = (decimal)constant.Value;

            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture), constant.Type, Numeric);

            return valueTranslation.WithSuffix("m", Numeric);
        }

        private static ITranslation GetDoubleTranslation(ConstantExpression constant)
        {
            var value = (double)constant.Value;

            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture), constant.Type, Numeric);

            return valueTranslation.WithSuffix("d", Numeric);
        }

        private static ITranslation GetFloatTranslation(ConstantExpression constant)
        {
            var value = (float)constant.Value;

            var valueTranslation = FixedValueTranslation((value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture), constant.Type, Numeric);

            return valueTranslation.WithSuffix("f", Numeric);
        }

        private static bool TryGetTypeTranslation(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            if (constant.Type.IsAssignableTo(typeof(Type)))
            {
                translation = new TypeLiteralTranslation((Type)constant.Value, context);
                return true;
            }

            translation = null;
            return false;
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
                buffer.WriteDotToTranslation();
                buffer.WriteToTranslation(_enumValue);
            }
        }

        private class DateTimeConstantTranslation : ITranslation
        {
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
                var estimatedSize = "new DateTime(".Length + 4 + 4 + 4;

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
                buffer.WriteNewToTranslation();
                buffer.WriteTypeNameToTranslation(nameof(DateTime));
                buffer.WriteToTranslation('(');

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

                buffer.WriteToTranslation('0', Numeric);
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
                    buffer.WriteTypeNameToTranslation(nameof(TimeSpan));
                    buffer.WriteDotToTranslation();
                    buffer.WriteToTranslation("FromTicks", MethodName);
                    buffer.WriteToTranslation('(');
                    buffer.WriteToTranslation(Math.Floor(_timeSpan.TotalMilliseconds * 10000).ToString(CurrentCulture));
                    goto EndTranslation;
                }

                buffer.WriteNewToTranslation();
                buffer.WriteTypeNameToTranslation(nameof(TimeSpan));
                buffer.WriteToTranslation('(');

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

                buffer.WriteTypeNameToTranslation(nameof(TimeSpan));
                buffer.WriteDotToTranslation();
                buffer.WriteToTranslation("From", MethodName);
                buffer.WriteToTranslation(valueName, MethodName);
                buffer.WriteToTranslation('(');
                buffer.WriteToTranslation(value);
                buffer.WriteToTranslation(')');
                return true;
            }
        }
    }
}