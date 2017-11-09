namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    internal class TypeEqualExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly MethodInfo _reduceTypeEqualMethod;

        public TypeEqualExpressionTranslator()
            : base(ExpressionType.TypeEqual)
        {
        }

        static TypeEqualExpressionTranslator()
        {
            try
            {
                _reduceTypeEqualMethod = typeof(TypeBinaryExpression)
                    .GetNonPublicInstanceMethods("ReduceTypeEqual")
                    .FirstOrDefault();
            }
            catch
            {
                // Unable to find or access ReduceTypeEqual - ignore
            }
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            if (_reduceTypeEqualMethod == null)
            {
                return FallbackTranslation(expression, context);
            }

            Expression reducedTypeEqualExpression;

            try
            {
                reducedTypeEqualExpression = (Expression)_reduceTypeEqualMethod.Invoke(expression, null);
            }
            catch
            {
                // Unable to invoke the non-public ReduceTypeEqual method:
                return FallbackTranslation(expression, context);
            }

            var translated = context.Translate(reducedTypeEqualExpression).Unterminated();

            return translated;
        }

        private static string FallbackTranslation(Expression expression, TranslationContext context)
        {
            var typeBinary = (TypeBinaryExpression)expression;
            var operand = context.Translate(typeBinary.Expression);

            return typeBinary.TypeOperand.IsClass()
                ? $"({operand} is {typeBinary.TypeOperand.GetFriendlyName()})"
                : $"({operand} TypeOf typeof({typeBinary.TypeOperand.GetFriendlyName()}))";
        }
    }
}