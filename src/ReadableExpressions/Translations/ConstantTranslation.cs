namespace AgileObjects.ReadableExpressions.Translations;

using System;
using System.Collections.Generic;
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
using static System.Convert;
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
    public static INodeTranslation For(
        ConstantExpression constant,
        ITranslationContext context)
    {
        if (constant.Value == null)
        {
            return DefaultValueTranslation.For(constant, context);
        }

        if (constant.Type.IsEnum())
        {
            return constant.Type.HasAttribute<FlagsAttribute>()
                ? new FlagsEnumConstantTranslation(constant, context)
                : new EnumConstantTranslation(constant, context);
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

        return context.GetTranslationFor(valueType).WithNodeType(Constant);
    }

    private static INodeTranslation FixedValueTranslation(
        ConstantExpression constant,
        TokenType tokenType)
    {
        return FixedValueTranslation(constant.Value, tokenType);
    }

    private static INodeTranslation FixedValueTranslation(
        object value,
        TokenType tokenType = TokenType.Default)
    {
        return FixedValueTranslation(value.ToString(), tokenType);
    }

    private static INodeTranslation FixedValueTranslation(
        string value,
        TokenType tokenType)
    {
        return new FixedValueTranslation(Constant, value, tokenType);
    }

    private static bool TryTranslateFromTypeCode(
        ConstantExpression constant,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        switch (constant.Type.GetNonNullableType().GetTypeCode())
        {
            case NetStandardTypeCode.Boolean:
                translation = FixedValueTranslation(
                    constant.Value.ToString().ToLowerInvariant(),
                    Keyword);

                return true;

            case NetStandardTypeCode.Char:
                var character = (char)constant.Value;
                var value = "'" + (character == '\0' ? @"\0" : character.ToString()) + "'";
                translation = FixedValueTranslation(value, Text);
                return true;

            case NetStandardTypeCode.DateTime:
                if (!TryTranslateDefault<DateTime>(constant, context, out translation))
                {
                    translation = new DateTimeConstantTranslation(constant);
                }

                return true;

            case NetStandardTypeCode.DBNull:
                translation = new DbNullTranslation();
                return true;

            case NetStandardTypeCode.Decimal:
                translation = GetDecimalTranslation(constant);
                return true;

            case NetStandardTypeCode.Double:
                translation = GetDoubleTranslation(constant);
                return true;

            case NetStandardTypeCode.Int64:
                translation = GetLongTranslation(constant);
                return true;

            case NetStandardTypeCode.Int32:
                translation = FixedValueTranslation(constant, Numeric);
                return true;

            case NetStandardTypeCode.Object:
                if (TryGetTypeTranslation(constant, context, out translation) ||
                    TryGetGuidTranslation(constant, context, out translation) ||
                    LambdaConstantTranslation.TryCreate(constant, context, out translation) ||
                    TryGetSimpleArrayTranslation(constant, context, out translation) ||
                    TryGetRegexTranslation(constant, context, out translation) ||
                    TimeSpanConstantTranslation.TryCreate(constant, context, out translation))
                {
                    return true;
                }

                break;

            case NetStandardTypeCode.Single:
                translation = GetFloatTranslation(constant);
                return true;

            case NetStandardTypeCode.String:
                var stringValue = GetStringConstant(constant, out var isVerbatim);
                stringValue = "\"" + stringValue + "\"";

                if (isVerbatim)
                {
                    stringValue = "@" + stringValue;
                }

                translation = FixedValueTranslation(stringValue, Text);
                return true;
        }

        return CannotTranslate(out translation);
    }

    private static bool TryTranslateDefault<T>(
        ConstantExpression constant,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        if (constant.Type != typeof(T) || !constant.Value.Equals(default(T)))
        {
            return CannotTranslate(out translation);
        }

        return TranslateDefault(constant, context, out translation);
    }

    private static bool TranslateDefault(
        ConstantExpression constant,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        translation = DefaultValueTranslation.For(constant, context);
        return true;
    }

    private static INodeTranslation GetDecimalTranslation(ConstantExpression constant)
    {
        var value = (decimal)constant.Value;

        var stringValue = (value % 1).Equals(0)
            ? value.ToString("0")
            : value.ToString(CurrentCulture);

        return FixedValueTranslation(stringValue + "m", Numeric);
    }

    private static INodeTranslation GetDoubleTranslation(ConstantExpression constant)
    {
        var value = (double)constant.Value;

        var stringValue = (value % 1).Equals(0)
            ? value.ToString("0")
            : value.ToString(CurrentCulture);

        return FixedValueTranslation(stringValue + "d", Numeric);
    }

    private static INodeTranslation GetLongTranslation(ConstantExpression constant)
    {
        return FixedValueTranslation(
            (long)constant.Value + "L",
            Numeric);
    }

    private static INodeTranslation GetFloatTranslation(ConstantExpression constant)
    {
        var value = (float)constant.Value;

        var stringValue = (value % 1).Equals(0)
            ? value.ToString("0")
            : value.ToString(CurrentCulture);

        return FixedValueTranslation(
            stringValue + "f",
            Numeric);
    }

    private static readonly char[] _newlineCharacters = { '\r', '\n' };

    private static string GetStringConstant(
        ConstantExpression constant,
        out bool isVerbatim)
    {
        var stringValue = (string)constant.Value;
        isVerbatim = stringValue.IndexOfAny(_newlineCharacters) != -1;

        return stringValue
            .Replace(@"\", @"\\")
            .Replace("\0", @"\0")
            .Replace(@"""", isVerbatim ? @"""""" : @"\""");
    }

    private static bool TryGetTypeTranslation(
        ConstantExpression constant,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        if (constant.Type.IsAssignableTo(typeof(Type)))
        {
            translation = new TypeOfOperatorTranslation(
                (Type)constant.Value,
                context);

            return true;
        }

        return CannotTranslate(out translation);
    }

    private static bool TryGetGuidTranslation(
        ConstantExpression constant,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        if (constant.Type.IsAssignableTo(typeof(Guid?)))
        {
            var guid = (Guid?)constant.Value;

            if (guid == default(Guid))
            {
                return TranslateDefault(constant, context, out translation);
            }

            translation = NewingTranslation.For(
                Expression.New(
                    typeof(Guid).GetPublicInstanceConstructor(typeof(string)),
                    Expression.Constant(guid.ToString(), typeof(string))),
                context);

            return true;
        }

        return CannotTranslate(out translation);
    }

    private static bool TryGetSimpleArrayTranslation(
        ConstantExpression constant,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        if (constant.Value is not Array array)
        {
            return CannotTranslate(out translation);
        }

        var elementType = array.GetType().GetElementType()!;

        if (elementType != typeof(string) &&
           !elementType.IsPrimitive() &&
           !elementType.IsValueType())
        {
            return CannotTranslate(out translation);
        }

        var arrayInit = Expression.NewArrayInit(elementType, array
            .Cast<object>()
            .Project<object, Expression>(item => Expression
                .Constant(item, elementType)));

        translation = constant.Type.IsArray
            ? ArrayInitialisationTranslation.For(arrayInit, context)
            : CastTranslation.For(Expression.Convert(arrayInit, constant.Type), context);

        return true;
    }

    private static bool TryGetRegexTranslation(
        ConstantExpression constant,
        ITranslationContext context,
        out INodeTranslation translation)
    {
        if (constant.Type != typeof(Regex))
        {
            return CannotTranslate(out translation);
        }

        var newRegex = Expression.New(
            typeof(Regex).GetPublicInstanceConstructor(typeof(string)),
            Expression.Constant(constant.Value.ToString()));

        translation = NewingTranslation.For(newRegex, context);
        return true;
    }

    private class FlagsEnumConstantTranslation : INodeTranslation
    {
        private readonly INodeTranslation _typeNameTranslation;
        private readonly IList<string> _enumMemberNames;
        private readonly int _enumMembersCount;

        public FlagsEnumConstantTranslation(
            ConstantExpression constant,
            ITranslationContext context)
        {
            var enumType = constant.Type;
            _typeNameTranslation = context.GetTranslationFor(enumType);

            var enumValue = constant.Value;
            var enumLongValue = GetLongValue(enumValue);

            _enumMemberNames = new List<string>();

            var enumTotalValue = 0L;

            foreach (var enumOption in Enum.GetValues(enumType).Cast<object>().Reverse())
            {
                var enumOptionLongValue = GetLongValue(enumOption);

                if (enumOptionLongValue == 0L)
                {
                    if (enumLongValue != 0L)
                    {
                        continue;
                    }
                }
                else if ((enumLongValue | enumOptionLongValue) != enumLongValue)
                {
                    continue;
                }

                var enumOptionName = enumOption.ToString();
                enumTotalValue += enumOptionLongValue;

                _enumMemberNames.Insert(0, enumOptionName);

                if (enumTotalValue == enumLongValue)
                {
                    break;
                }
            }

            _enumMembersCount = _enumMemberNames.Count;
        }

        #region Setup

        private static long GetLongValue(object enumValue)
            => (long)ChangeType(enumValue, typeof(long));

        #endregion

        public ExpressionType NodeType => Constant;

        public int TranslationLength =>
            _enumMemberNames.Sum(name => name.Length) +
           (_typeNameTranslation.TranslationLength + ".".Length) * _enumMemberNames.Count;

        public void WriteTo(TranslationWriter writer)
        {
            for (var i = 0; ;)
            {
                writer.WriteEnumValue(_typeNameTranslation, _enumMemberNames[i]);
                ++i;

                if (i == _enumMembersCount)
                {
                    return;
                }

                writer.WriteToTranslation(" | ");
            }
        }
    }

    private class EnumConstantTranslation : INodeTranslation
    {
        private readonly INodeTranslation _typeNameTranslation;
        private readonly string _enumMemberName;

        public EnumConstantTranslation(
            ConstantExpression constant,
            ITranslationContext context)
        {
            _typeNameTranslation = context.GetTranslationFor(constant.Type);
            _enumMemberName = constant.Value.ToString();
        }

        public ExpressionType NodeType => Constant;

        public int TranslationLength =>
            _typeNameTranslation.TranslationLength +
            ".".Length +
            _enumMemberName.Length;

        public void WriteTo(TranslationWriter writer)
            => writer.WriteEnumValue(_typeNameTranslation, _enumMemberName);
    }

    private static void WriteEnumValue(
        this TranslationWriter writer,
        ITranslation enumTypeNameTranslation,
        string enumMemberName)
    {
        enumTypeNameTranslation.WriteTo(writer);
        writer.WriteDotToTranslation();
        writer.WriteToTranslation(enumMemberName);
    }

    private class DateTimeConstantTranslation : INodeTranslation
    {
        private readonly DateTime _value;
        private readonly bool _hasMilliseconds;
        private readonly bool _hasTime;

        public DateTimeConstantTranslation(ConstantExpression constant)
        {
            _value = (DateTime)constant.Value;
            _hasMilliseconds = _value.Millisecond != 0;
            _hasTime = _value.Hour != 0 || _value.Minute != 0 || _value.Second != 0;
        }

        public ExpressionType NodeType => Constant;

        public int TranslationLength
        {
            get
            {
                var translationLength = "new DateTime(".Length + 4 + 4 + 4;

                if (_hasMilliseconds || _hasTime)
                {
                    translationLength += 4 + 4 + 4;

                    if (_hasMilliseconds)
                    {
                        translationLength += ", ".Length + 4;
                    }
                }

                return translationLength + 1;
            }
        }

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

    private class LambdaConstantTranslation :
        INodeTranslation,
        IPotentialSelfTerminatingTranslation
    {
        private readonly INodeTranslation _lambdaTranslation;

        private LambdaConstantTranslation(
            LambdaExpression lambda,
            ITranslationContext context)
        {
            _lambdaTranslation = context.GetCodeBlockTranslationFor(lambda);
        }

        public static bool TryCreate(
            ConstantExpression constant,
            ITranslationContext context,
            out INodeTranslation lambdaTranslation)
        {
#if NET35
            if (constant.Value is LinqLambda linqLambda)
            {
                var convertedLambda =
                    LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

                lambdaTranslation =
                    new LambdaConstantTranslation(convertedLambda, context);

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

        public int TranslationLength => _lambdaTranslation.TranslationLength;

        public bool IsTerminated => true;

        public void WriteTo(TranslationWriter writer)
            => _lambdaTranslation.WriteTo(writer);
    }

    private class TimeSpanConstantTranslation : INodeTranslation
    {
        private readonly TimeSpan _timeSpan;

        private TimeSpanConstantTranslation(ConstantExpression timeSpanConstant)
        {
            _timeSpan = (TimeSpan)timeSpanConstant.Value;
        }

        public static bool TryCreate(
            ConstantExpression constant,
            ITranslationContext context,
            out INodeTranslation timeSpanTranslation)
        {
            if (constant.Type != typeof(TimeSpan))
            {
                return CannotTranslate(out timeSpanTranslation);
            }

            if (TryTranslateDefault<TimeSpan>(constant, context, out timeSpanTranslation))
            {
                return true;
            }

            timeSpanTranslation = new TimeSpanConstantTranslation(constant);
            return true;
        }

        public ExpressionType NodeType => Constant;

        public int TranslationLength => _timeSpan.ToString().Length;

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

            if (_timeSpan is { Days: 0, Hours: 0, Minutes: 0, Seconds: 0 })
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

        private static void WriteTimeSpanHoursMinutesSeconds(
            TranslationWriter writer, TimeSpan timeSpan)
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

    private static bool CannotTranslate(out INodeTranslation translation)
    {
        translation = null;
        return false;
    }
}