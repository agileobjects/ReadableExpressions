namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;

    internal class TypeEqualExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly MethodInfo _reduceTypeEqualMethod =
            typeof(TypeBinaryExpression)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "ReduceTypeEqual");

        public TypeEqualExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.TypeEqual)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            if (_reduceTypeEqualMethod == null)
            {
                return FallbackTranslation(expression, context);
            }

            var reducedTypeEqualExpression = (Expression)_reduceTypeEqualMethod.Invoke(expression, null);
            var translated = GetTranslation(reducedTypeEqualExpression, context).Unterminated();

            return translated;
        }

        private string FallbackTranslation(Expression expression, TranslationContext context)
        {
            var typeBinary = (TypeBinaryExpression)expression;
            var operand = GetTranslation(typeBinary.Expression, context);

            return $"({ operand} TypeOf {typeBinary.TypeOperand.GetFriendlyName()})";
        }
    }
}