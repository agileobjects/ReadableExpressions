namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Text.RegularExpressions;
    using Extensions;
    using Formatting;
    using Initialisations;
    using NetStandardPolyfills;
    using static System.Globalization.CultureInfo;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
    using LinqLambda = System.Linq.Expressions.LambdaExpression;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif
    using static Formatting.TokenType;

    internal static class ConstantTranslation
    {
        public static ITranslation For(ConstantExpression constant, ITranslationContext context)
        {
            if (context.Settings.ConstantExpressionValueFactory != null)
            {
                var userTranslation = context.Settings
                    .ConstantExpressionValueFactory(constant.Type, constant.Value);

                return (userTranslation == null)
                    ? DefaultValueTranslation.For(constant, context)
                    : FixedValueTranslation(userTranslation, constant.Type, context);
            }

            if (constant.Value == null)
            {
                return DefaultValueTranslation.For(constant, context);
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
                return FixedValueTranslation(constant.Value, valueType, context);
            }

            return context.GetTranslationFor(valueType).WithNodeType(Constant);
        }

        private static ITranslation FixedValueTranslation(
            ConstantExpression constant,
            ITranslationContext context,
            TokenType tokenType = TokenType.Default)
        {
            return FixedValueTranslation(constant.Value, constant.Type, context, tokenType);
        }

        private static ITranslation FixedValueTranslation(
            object value,
            Type type,
            ITranslationContext context,
            TokenType tokenType = TokenType.Default)
        {
            return FixedValueTranslation(value.ToString(), type, tokenType, context);
        }

        private static ITranslation FixedValueTranslation(
            string value,
            Type type,
            TokenType tokenType,
            ITranslationContext context)
        {
            return new FixedValueTranslation(Constant, value, type, tokenType, context);
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
                        Keyword,
                        context);

                    return true;

                case NetStandardTypeCode.Char:
                    var character = (char)constant.Value;
                    var value = "'" + (character == '\0' ? @"\0" : character.ToString()) + "'";
                    translation = FixedValueTranslation(value, type, Text, context);
                    return true;

                case NetStandardTypeCode.DateTime:
                    if (!TryTranslateDefault<DateTime>(constant, context, out translation))
                    {
                        translation = new DateTimeConstantTranslation(constant, context);
                    }

                    return true;

                case NetStandardTypeCode.DBNull:
                    translation = new DbNullTranslation(constant, context);
                    return true;

                case NetStandardTypeCode.Decimal:
                    translation = GetDecimalTranslation(constant, context);
                    return true;

                case NetStandardTypeCode.Double:
                    translation = GetDoubleTranslation(constant, context);
                    return true;

                case NetStandardTypeCode.Int64:
                    translation = GetLongTranslation(constant, context);
                    return true;

                case NetStandardTypeCode.Int32:
                    translation = FixedValueTranslation(constant, context, Numeric);
                    return true;

                case NetStandardTypeCode.Object:
                    if (TryGetTypeTranslation(constant, context, out translation) ||
                        LambdaConstantTranslation.TryCreate(constant, context, out translation) ||
                        TryGetSimpleArrayTranslation(constant, context, out translation) ||
                        TryGetRegexTranslation(constant, context, out translation) ||
                        TryTranslateDefault<Guid>(constant, context, out translation) ||
                        TimeSpanConstantTranslation.TryCreate(constant, context, out translation))
                    {
                        return true;
                    }

                    break;

                case NetStandardTypeCode.Single:
                    translation = GetFloatTranslation(constant, context);
                    return true;

                case NetStandardTypeCode.String:
                    var stringValue = ((string)constant.Value)
                        .Replace(@"\", @"\\")
                        .Replace("\0", @"\0")
                        .Replace(@"""", @"\""");

                    stringValue = "\"" + stringValue + "\"";
                    translation = FixedValueTranslation(stringValue, typeof(string), Text, context);
                    return true;
            }

            return CannotTranslate(out translation);
        }

        private static bool TryTranslateDefault<T>(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            if ((constant.Type != typeof(T)) || !constant.Value.Equals(default(T)))
            {
                return CannotTranslate(out translation);
            }

            translation = DefaultValueTranslation.For(constant, context);
            return true;
        }

        private static ITranslation GetDecimalTranslation(
            ConstantExpression constant,
            ITranslationContext context)
        {
            var value = (decimal)constant.Value;

            var stringValue = (value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture);

            return FixedValueTranslation(stringValue + "m", constant.Type, Numeric, context);
        }

        private static ITranslation GetDoubleTranslation(
            ConstantExpression constant,
            ITranslationContext context)
        {
            var value = (double)constant.Value;

            var stringValue = (value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture);

            return FixedValueTranslation(stringValue + "d", constant.Type, Numeric, context);
        }

        private static ITranslation GetFloatTranslation(
            ConstantExpression constant,
            ITranslationContext context)
        {
            var value = (float)constant.Value;

            var stringValue = (value % 1).Equals(0)
                ? value.ToString("0")
                : value.ToString(CurrentCulture);

            return FixedValueTranslation(stringValue + "f", constant.Type, Numeric, context);
        }

        private static ITranslation GetLongTranslation(
            ConstantExpression constant,
            ITranslationContext context)
        {
            return FixedValueTranslation((long)constant.Value + "L", constant.Type, Numeric, context);
        }

        private static bool TryGetTypeTranslation(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            if (constant.Type.IsAssignableTo(typeof(Type)))
            {
                translation = new TypeOfOperatorTranslation((Type)constant.Value, context);
                return true;
            }

            return CannotTranslate(out translation);
        }

        private static bool TryGetSimpleArrayTranslation(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            if (!constant.Type.IsArray)
            {
                return CannotTranslate(out translation);
            }

            var elementType = constant.Type.GetElementType();

            if (elementType != typeof(string) &&
               !elementType.IsPrimitive() &&
               !elementType.IsValueType())
            {
                return CannotTranslate(out translation);
            }

            var array = (Array)constant.Value;

            var arrayInit = Expression.NewArrayInit(elementType, array
                .Cast<object>()
                .Project<object, Expression>(item => Expression.Constant(item, elementType)));

            translation = ArrayInitialisationTranslation.For(arrayInit, context);
            return true;
        }

        private static bool TryGetRegexTranslation(
            ConstantExpression constant,
            ITranslationContext context,
            out ITranslation translation)
        {
            if (constant.Type != typeof(Regex))
            {
                return CannotTranslate(out translation);
            }

            translation = FixedValueTranslation(constant, context);
            translation = new WrappedTranslation("Regex /* ", translation, " */");
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
                TranslationSize = _typeNameTranslation.TranslationSize + 1 + _enumValue.Length;
            }

            public ExpressionType NodeType => Constant;

            public Type Type => _typeNameTranslation.Type;

            public int TranslationSize { get; }

            public int FormattingSize => _typeNameTranslation.FormattingSize;

            public int GetIndentSize() => _typeNameTranslation.GetIndentSize();

            public int GetLineCount() => _typeNameTranslation.GetLineCount();

            public void WriteTo(TranslationWriter writer)
            {
                _typeNameTranslation.WriteTo(writer);
                writer.WriteDotToTranslation();
                writer.WriteToTranslation(_enumValue);
            }
        }

        private class DateTimeConstantTranslation : ITranslation
        {
            private readonly DateTime _value;
            private readonly bool _hasMilliseconds;
            private readonly bool _hasTime;

            public DateTimeConstantTranslation(ConstantExpression constant, ITranslationContext context)
            {
                Type = constant.Type;
                _value = (DateTime)constant.Value;
                _hasMilliseconds = _value.Millisecond != 0;
                _hasTime = (_value.Hour != 0) || (_value.Minute != 0) || (_value.Second != 0);

                var translationSize = "new DateTime(".Length + 4 + 4 + 4;
                var numericFormattingSize = context.GetFormattingSize(Numeric);
                var formattingSize = context.GetTypeNameFormattingSize() + numericFormattingSize * 3;

                if (_hasMilliseconds || _hasTime)
                {
                    translationSize += 4 + 4 + 4;
                    formattingSize += numericFormattingSize * 3;

                    if (_hasMilliseconds)
                    {
                        translationSize += ", ".Length + 4;
                        formattingSize += numericFormattingSize;
                    }
                }

                TranslationSize = translationSize + 1;
                FormattingSize = formattingSize;
            }

            public ExpressionType NodeType => Constant;

            public Type Type { get; }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize() => 0;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationWriter writer)
            {
                writer.WriteNewToTranslation();
                writer.WriteTypeNameToTranslation(nameof(DateTime));
                writer.WriteToTranslation('(');

                writer.WriteToTranslation(_value.Year);
                WriteTwoDigitDatePart(_value.Month, writer);
                WriteTwoDigitDatePart(_value.Day, writer);

                if (_hasMilliseconds || _hasTime)
                {
                    WriteTwoDigitDatePart(_value.Hour, writer);
                    WriteTwoDigitDatePart(_value.Minute, writer);
                    WriteTwoDigitDatePart(_value.Second, writer);

                    if (_hasMilliseconds)
                    {
                        writer.WriteToTranslation(", ");
                        writer.WriteToTranslation(_value.Millisecond);
                    }
                }

                writer.WriteToTranslation(")");
            }

            private static void WriteTwoDigitDatePart(int datePart, TranslationWriter writer)
            {
                writer.WriteToTranslation(", ");

                if (datePart > 9)
                {
                    writer.WriteToTranslation(datePart);
                    return;
                }

                writer.WriteToTranslation(0);
                writer.WriteToTranslation(datePart);
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
                if (constant.Value is not LambdaExpression lambda)
                {
                    return CannotTranslate(out lambdaTranslation);
                }

                lambdaTranslation = new LambdaConstantTranslation(lambda, context);
                return true;
            }

            public ExpressionType NodeType => Constant;

            public Type Type => _lambdaTranslation.Type;

            public int TranslationSize => _lambdaTranslation.TranslationSize;

            public int FormattingSize => _lambdaTranslation.FormattingSize;

            public bool IsTerminated => true;

            public int GetIndentSize() => _lambdaTranslation.GetIndentSize();

            public int GetLineCount() => _lambdaTranslation.GetLineCount();

            public void WriteTo(TranslationWriter writer) => _lambdaTranslation.WriteTo(writer);
        }

        private class TimeSpanConstantTranslation : ITranslation
        {
            private readonly TimeSpan _timeSpan;

            private TimeSpanConstantTranslation(
                ConstantExpression timeSpanConstant,
                ITranslationContext context)
            {
                Type = timeSpanConstant.Type;
                _timeSpan = (TimeSpan)timeSpanConstant.Value;
                TranslationSize = _timeSpan.ToString().Length;

                var formattingSize = context.GetTypeNameFormattingSize();

                FormattingSize = formattingSize;
            }

            public static bool TryCreate(
                ConstantExpression constant,
                ITranslationContext context,
                out ITranslation timeSpanTranslation)
            {
                if (constant.Type != typeof(TimeSpan))
                {
                    return CannotTranslate(out timeSpanTranslation);
                }

                if (TryTranslateDefault<TimeSpan>(constant, context, out timeSpanTranslation))
                {
                    return true;
                }

                timeSpanTranslation = new TimeSpanConstantTranslation(constant, context);
                return true;
            }

            public ExpressionType NodeType => Constant;

            public Type Type { get; }

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize() => 0;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationWriter writer)
            {
                if (TryWriteFactoryMethodCall(_timeSpan.Days, _timeSpan.TotalDays, "Days", writer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Hours, _timeSpan.TotalHours, "Hours", writer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Minutes, _timeSpan.TotalMinutes, "Minutes", writer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Seconds, _timeSpan.TotalSeconds, "Seconds", writer))
                {
                    return;
                }

                if (TryWriteFactoryMethodCall(_timeSpan.Milliseconds, _timeSpan.TotalMilliseconds, "Milliseconds", writer))
                {
                    return;
                }

                if ((_timeSpan.Days == 0) && (_timeSpan.Hours == 0) && (_timeSpan.Minutes == 0) && (_timeSpan.Seconds == 0))
                {
                    writer.WriteTypeNameToTranslation(nameof(TimeSpan));
                    writer.WriteDotToTranslation();
                    writer.WriteToTranslation("FromTicks", MethodName);
                    writer.WriteToTranslation('(');
                    writer.WriteToTranslation(Math.Floor(_timeSpan.TotalMilliseconds * 10000).ToString(CurrentCulture));
                    goto EndTranslation;
                }

                writer.WriteNewToTranslation();
                writer.WriteTypeNameToTranslation(nameof(TimeSpan));
                writer.WriteToTranslation('(');

                if (_timeSpan.Days == 0)
                {
                    WriteTimeSpanHoursMinutesSeconds(writer, _timeSpan);
                    goto EndTranslation;
                }

                writer.WriteToTranslation(_timeSpan.Days);
                writer.WriteToTranslation(", ");
                WriteTimeSpanHoursMinutesSeconds(writer, _timeSpan);

                if (_timeSpan.Milliseconds != 0)
                {
                    writer.WriteToTranslation(", ");
                    writer.WriteToTranslation(_timeSpan.Milliseconds);
                }

            EndTranslation:
                writer.WriteToTranslation(')');
            }

            private static void WriteTimeSpanHoursMinutesSeconds(TranslationWriter writer, TimeSpan timeSpan)
            {
                writer.WriteToTranslation(timeSpan.Hours);
                writer.WriteToTranslation(", ");
                writer.WriteToTranslation(timeSpan.Minutes);
                writer.WriteToTranslation(", ");
                writer.WriteToTranslation(timeSpan.Seconds);
            }

            private static bool TryWriteFactoryMethodCall(
                long value,
                double totalValue,
                string valueName,
                TranslationWriter writer)
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

                var factoryMethodName = "From" + valueName;

                writer.WriteTypeNameToTranslation(nameof(TimeSpan));
                writer.WriteDotToTranslation();
                writer.WriteToTranslation(factoryMethodName, MethodName);
                writer.WriteToTranslation('(');
                writer.WriteToTranslation(value);
                writer.WriteToTranslation(')');
                return true;
            }
        }

        private static bool CannotTranslate(out ITranslation translation)
        {
            translation = null;
            return false;
        }
    }
}