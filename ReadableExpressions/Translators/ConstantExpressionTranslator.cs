namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Extensions;

    internal class ConstantExpressionTranslator : ExpressionTranslatorBase
    {
        internal ConstantExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Constant)
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
                    if (constant.Value.Equals(default(DateTime)))
                    {
                        translation = "default(DateTime)";
                        return true;
                    }
                    break;

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
                        IsDefaultTimeSpan(constant, out translation))
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

        private static bool IsDefaultTimeSpan(ConstantExpression constant, out string translation)
        {
            if ((constant.Type == typeof(TimeSpan)) && constant.Value.Equals(default(TimeSpan)))
            {
                translation = "default(TimeSpan)";
                return true;
            }

            translation = null;
            return false;
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

        private static readonly Regex _funcMatcher = new Regex(@"^System\.(?<Type>Func|Action)`\d+\[(?<Arguments>[^\]]+)\]$");

        private static bool IsFunc(ConstantExpression constant, out string translation)
        {
            var match = _funcMatcher.Match(constant.Value.ToString());

            if (!match.Success)
            {
                translation = null;
                return false;
            }

            var funcType = match.Groups["Type"].Value;

            var argumentTypes = match.Groups["Arguments"].Value
                .Split(',')
                .Select(typeFullName => typeFullName.GetSubstitutionOrNull() ?? typeFullName);

            translation = $"{funcType}<{string.Join(", ", argumentTypes)}>";
            return true;
        }
    }
}