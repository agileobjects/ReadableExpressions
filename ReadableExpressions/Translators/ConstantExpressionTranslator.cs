namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal class ConstantExpressionTranslator : ExpressionTranslatorBase
    {
        internal ConstantExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Constant)
        {
        }

        public override string Translate(Expression expression)
        {
            return (expression as CommentExpression)?.Comment
                ?? TranslateConstant((ConstantExpression)expression);
        }

        private static string TranslateConstant(ConstantExpression constant)
        {
            if (constant.Value == null)
            {
                return "null";
            }

            if (constant.Type.IsEnum)
            {
                return constant.Type.GetFriendlyName() + "." + constant.Value;
            }

            switch (Type.GetTypeCode(Nullable.GetUnderlyingType(constant.Type) ?? constant.Type))
            {
                case TypeCode.Boolean:
                    return constant.Value.ToString().ToLowerInvariant();

                case TypeCode.Decimal:
                    return constant.Value + "m";

                case TypeCode.Double:
                    return FormatNumeric((double)constant.Value);

                case TypeCode.Int64:
                    return constant.Value + "L";

                case TypeCode.Single:
                    return FormatNumeric((float)constant.Value) + "f";

                case TypeCode.String:
                    return $"\"{constant.Value}\"";
            }

            if (constant.Type == typeof(Type))
            {
                return $"typeof({((Type)constant.Value).GetFriendlyName()})";
            }

            return constant.Value.ToString();
        }

        private static string FormatNumeric(double value)
        {
            return (value % 1).Equals(0) ? value.ToString("0.00") : value.ToString();
        }

        private static string FormatNumeric(float value)
        {
            return (value % 1).Equals(0) ? value.ToString("0.00") : value.ToString();
        }
    }
}