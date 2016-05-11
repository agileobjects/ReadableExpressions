namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;
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

            if ((constant.Type == typeof(TimeSpan)) && constant.Value.Equals(default(TimeSpan)))
            {
                return "default(TimeSpan)";
            }

            if (typeof(Type).IsAssignableFrom(constant.Type))
            {
                return $"typeof({((Type)constant.Value).GetFriendlyName()})";
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
    }
}