namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
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

        public TypeEqualExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.TypeEqual)
        {
        }

        public override string Translate(Expression expression)
        {
            if (_reduceTypeEqualMethod == null)
            {
                return FallbackTranslation(expression);
            }

            var reducedTypeEqualExpression = (Expression)_reduceTypeEqualMethod.Invoke(expression, null);
            var translated = Registry.Translate(reducedTypeEqualExpression);

            if (translated.EndsWith(";", StringComparison.Ordinal))
            {
                translated = translated.TrimEnd(';');
            }

            return translated;
        }

        private string FallbackTranslation(Expression expression)
        {
            var typeBinary = (TypeBinaryExpression)expression;
            var operand = Registry.Translate(typeBinary.Expression);

            return $"({ operand} TypeOf {typeBinary.TypeOperand.GetFriendlyName()})";
        }
    }
}