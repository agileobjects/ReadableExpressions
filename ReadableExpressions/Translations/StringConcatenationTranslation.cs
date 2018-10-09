namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal class StringConcatenationTranslation : ITranslatable
    {
        private readonly int _operandCount;
        private readonly IList<ITranslation> _operandTranslations;

        public StringConcatenationTranslation(IList<Expression> operands, ITranslationContext context)
        {
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
                _operandTranslations[i] = operandTranslation;
                estimatedSize += operandTranslation.EstimatedSize + 3; // <- +3 for ' + '
            }

            EstimatedSize = estimatedSize;
        }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            for (int i = 0, l = _operandCount - 1; ; ++i)
            {
                var operand = _operandTranslations[i];

                if (operand.NodeType == ExpressionType.Conditional)
                {
                    operand.WriteInParentheses(context);
                }
                else
                {
                    operand.WriteTo(context);
                }

                if (i == l)
                {
                    break;
                }

                context.WriteToTranslation(" + ");
            }
        }
    }
}