namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class CastExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, ConversionHelper> _operatorsByNodeType =
            new Dictionary<ExpressionType, ConversionHelper>
            {
                [ExpressionType.Convert] = new ConversionHelper(
                    exp => ((UnaryExpression)exp).Operand,
                    exp => exp.Type,
                    (subject, typeName) => $"(({typeName}){subject})"),
                [ExpressionType.TypeAs] = new ConversionHelper(
                    exp => ((UnaryExpression)exp).Operand,
                    exp => exp.Type,
                    (subject, typeName) => $"({subject} as {typeName})"),
                [ExpressionType.TypeIs] = new ConversionHelper(
                    exp => ((TypeBinaryExpression)exp).Expression,
                    exp => ((TypeBinaryExpression)exp).TypeOperand,
                    (subject, typeName) => $"({subject} is {typeName})")
            };

        internal CastExpressionTranslator()
            : base(_operatorsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            if ((expression.NodeType == ExpressionType.Convert) && (expression.Type == typeof(object)))
            {
                // Don't bother showing a boxing operation:
                return translatorRegistry.Translate(((UnaryExpression)expression).Operand);
            }

            return _operatorsByNodeType[expression.NodeType].Translate(expression, translatorRegistry);
        }

        private class ConversionHelper
        {
            private readonly Func<Expression, IExpressionTranslatorRegistry, string> _translator;

            public ConversionHelper(
                Func<Expression, Expression> subjectGetter,
                Func<Expression, Type> conversionTypeGetter,
                Func<string, string, string> formatter)
            {
                _translator = (expression, translatorRegistry) =>
                {
                    var subject = translatorRegistry.Translate(subjectGetter.Invoke(expression));
                    var typeName = conversionTypeGetter.Invoke(expression).GetFriendlyName();
                    var formatted = formatter.Invoke(subject, typeName);

                    return formatted;
                };
            }

            public string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
            {
                return _translator.Invoke(expression, translatorRegistry);
            }
        }
    }
}