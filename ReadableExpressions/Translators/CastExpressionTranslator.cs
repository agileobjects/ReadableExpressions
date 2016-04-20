namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;

    internal class CastExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly Dictionary<ExpressionType, Func<Expression, TranslationContext, string>> _translatorsByType;

        internal CastExpressionTranslator(Translator globalTranslator)
            : base(
                globalTranslator,
                ExpressionType.Convert,
                ExpressionType.ConvertChecked,
                ExpressionType.TypeAs,
                ExpressionType.TypeIs,
                ExpressionType.Unbox)
        {
            _translatorsByType = new Dictionary<ExpressionType, Func<Expression, TranslationContext, string>>
            {
                [ExpressionType.Convert] = TranslateCast,
                [ExpressionType.ConvertChecked] = TranslateCast,
                [ExpressionType.TypeAs] = TranslateTypeAs,
                [ExpressionType.TypeIs] = TranslateTypeIs,
                [ExpressionType.Unbox] = TranslateCast,
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            if ((expression.NodeType == ExpressionType.Convert) && (expression.Type == typeof(object)))
            {
                // Don't bother showing a boxing operation:
                return GetTranslation(((UnaryExpression)expression).Operand, context);
            }

            return _translatorsByType[expression.NodeType].Invoke(expression, context);
        }

        private string TranslateCast(Expression expression, TranslationContext context)
        {
            var cast = (UnaryExpression)expression;
            var typeName = cast.Type.GetFriendlyName();
            var subject = GetTranslation(cast.Operand, context);

            return $"(({typeName}){subject})";
        }

        private string TranslateTypeAs(Expression expression, TranslationContext context)
        {
            var typeAs = (UnaryExpression)expression;
            var typeName = typeAs.Type.GetFriendlyName();
            var subject = GetTranslation(typeAs.Operand, context);

            return $"({subject} as {typeName})";
        }

        private string TranslateTypeIs(Expression expression, TranslationContext context)
        {
            var typeIs = (TypeBinaryExpression)expression;
            var typeName = typeIs.TypeOperand.GetFriendlyName();
            var subject = GetTranslation(typeIs.Expression, context);

            return $"({subject} is {typeName})";
        }
    }
}