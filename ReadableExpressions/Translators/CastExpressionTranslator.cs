namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;

    internal class CastExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<ExpressionType, Func<Expression, string>> _translatorsByType;

        internal CastExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(
                registry,
                ExpressionType.Convert,
                ExpressionType.ConvertChecked,
                ExpressionType.TypeAs,
                ExpressionType.TypeIs)
        {
            _translatorsByType = new Dictionary<ExpressionType, Func<Expression, string>>
            {
                [ExpressionType.Convert] = TranslateCast,
                [ExpressionType.ConvertChecked] = TranslateCast,
                [ExpressionType.TypeAs] = TranslateTypeAs,
                [ExpressionType.TypeIs] = TranslateTypeIs
            };
        }

        public override string Translate(Expression expression)
        {
            if ((expression.NodeType == ExpressionType.Convert) && (expression.Type == typeof(object)))
            {
                // Don't bother showing a boxing operation:
                return Registry.Translate(((UnaryExpression)expression).Operand);
            }

            return _translatorsByType[expression.NodeType].Invoke(expression);
        }

        private string TranslateCast(Expression expression)
        {
            var cast = (UnaryExpression)expression;
            var typeName = cast.Type.GetFriendlyName();
            var subject = Registry.Translate(cast.Operand);

            return $"(({typeName}){subject})";
        }

        private string TranslateTypeAs(Expression expression)
        {
            var typeAs = (UnaryExpression)expression;
            var typeName = typeAs.Type.GetFriendlyName();
            var subject = Registry.Translate(typeAs.Operand);

            return $"({subject} as {typeName})";
        }

        private string TranslateTypeIs(Expression expression)
        {
            var typeIs = (TypeBinaryExpression)expression;
            var typeName = typeIs.TypeOperand.GetFriendlyName();
            var subject = Registry.Translate(typeIs.Expression);

            return $"({subject} is {typeName})";
        }
    }
}