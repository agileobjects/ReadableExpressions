namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class CastExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, Func<string, string, string>> _operatorsByNodeType =
            new Dictionary<ExpressionType, Func<string, string, string>>
            {
                { ExpressionType.Convert, (typeName, subject) => $"(({typeName}){subject})" },
                { ExpressionType.TypeAs, (typeName, subject) => $"({subject} as {typeName})" }
            };

        internal CastExpressionTranslator()
            : base(_operatorsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var conversion = (UnaryExpression)expression;
            var conversionSubject = translatorRegistry.Translate(conversion.Operand);

            if ((expression.NodeType == ExpressionType.Convert) && (expression.Type == typeof(object)))
            {
                // Don't bother showing a boxing operation:
                return conversionSubject;
            }

            var typeName = conversion.Type.GetFriendlyName();

            return _operatorsByNodeType[expression.NodeType].Invoke(typeName, conversionSubject);
        }
    }
}