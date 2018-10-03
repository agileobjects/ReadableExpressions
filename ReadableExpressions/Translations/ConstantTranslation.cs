using System.Text.RegularExpressions;

namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
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

            var valueString = _constant.Value.ToString();

            if (_isEnumValue)
            {
                return _typeNameTranslation.EstimatedSize + ".".Length + valueString.Length;
            }

            return valueString.Length;
        }

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

            }
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

                case NetStandardTypeCode.Object:
                    if (TryWriteType(context) ||
                        TryWriteLambda(context) ||
                        TryWriteFunc(context) ||
                        IsRegex(constant, out translation) ||
                        TryWriteDefault<Guid>(constant, out translation) ||
                        IsTimeSpan(constant, out translation))
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
            if (_constant.Value is LambdaExpression lambda)
            {
                context.GetTranslationFor(lambda).WriteTo(context);
                return true;
            }
#if NET35
            if (_constant.Value is System.Linq.Expressions.LambdaExpression linqLambda)
            {
                lambda = LinqExpressionToDlrExpressionConverter.Convert(linqLambda);
                context.GetTranslationFor(lambda).WriteTo(context);
                return true;
            }
#endif
            return false;
        }

        private static readonly Regex _funcMatcher = new Regex(@"^System\.(?:Func|Action)`\d+\[.+\]$");

        private bool TryWriteFunc(ITranslationContext context)
        {
            var match = _funcMatcher.Match(_constant.Value.ToString());

            if (match.Success)
            {
                WriteFuncFrom(match.Value, context);
                return true;
            }

            return false;
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