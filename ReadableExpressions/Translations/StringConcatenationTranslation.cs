namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class StringConcatenationTranslation : ITranslation
    {
        private readonly int _operandCount;
        private readonly IList<ITranslation> _operandTranslations;

        public StringConcatenationTranslation(
            ExpressionType nodeType,
            IList<Expression> operands,
            ITranslationContext context)
        {
            NodeType = nodeType;

            var estimatedSize = 0;
            _operandCount = operands.Count;
            _operandTranslations = new ITranslation[_operandCount];

            for (var i = 0; i < _operandCount; ++i)
            {
                var operand = operands[i];

                if (operand.NodeType == ExpressionType.Call)
                {
                    var methodCall = (MethodCallExpression)operand;

                    if ((methodCall.Method.Name == nameof(ToString)) && !methodCall.Arguments.Any())
                    {
                        operand = methodCall.GetSubject();
                    }
                }

                var operandTranslation = context.GetTranslationFor(operand);

                if ((operandTranslation.Type != typeof(string)) && operandTranslation.IsBinary())
                {
                    operandTranslation = operandTranslation.WithParentheses();
                }

                _operandTranslations[i] = operandTranslation;
                estimatedSize += operandTranslation.EstimatedSize + 3; // <- +3 for ' + '
            }

            EstimatedSize = estimatedSize;
        }

        public ExpressionType NodeType { get; }

        public Type Type => typeof(string);

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            for (var i = 0; ;)
            {
                var operandTranslation = _operandTranslations[i];

                if ((operandTranslation.NodeType == ExpressionType.Conditional) || operandTranslation.IsAssignment())
                {
                    operandTranslation.WriteInParentheses(buffer);
                }
                else
                {
                    operandTranslation.WriteTo(buffer);
                }

                if (++i == _operandCount)
                {
                    break;
                }

                buffer.WriteToTranslation(" + ");
            }
        }
    }
}