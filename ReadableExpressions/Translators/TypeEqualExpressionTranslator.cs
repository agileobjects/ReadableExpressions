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

        public TypeEqualExpressionTranslator()
            : base(ExpressionType.TypeEqual)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            if (_reduceTypeEqualMethod == null)
            {
                return FallbackTranslation(expression, translatorRegistry);
            }

            var reducedTypeEqualExpression = (Expression)_reduceTypeEqualMethod.Invoke(expression, null);
            var translated = translatorRegistry.Translate(reducedTypeEqualExpression);

            if (translated.EndsWith(";", StringComparison.Ordinal))
            {
                translated = translated.TrimEnd(';');
            }

            return translated;
        }

        private static string FallbackTranslation(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var typeBinary = (TypeBinaryExpression)expression;
            var operand = translatorRegistry.Translate(typeBinary.Expression);

            return $"({ operand} TypeOf {typeBinary.TypeOperand.GetFriendlyName()})";
        }
    }
}