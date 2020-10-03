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

            var translationSize = 0;
            var formattingSize = 0;
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
                translationSize += operandTranslation.TranslationSize + 3; // <- +3 for ' + '
                formattingSize += operandTranslation.FormattingSize;
            }

            TranslationSize = translationSize;
            FormattingSize = formattingSize;
        }

        public ExpressionType NodeType { get; }

        public Type Type => typeof(string);

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            var indentSize = 0;

            for (var i = 0; ;)
            {
                indentSize += _operandTranslations[i].GetIndentSize();

                ++i;

                if (i == _operandCount)
                {
                    return indentSize;
                }
            }
        }

        public int GetLineCount()
            => _operandTranslations.GetLineCount(_operandCount);

        public void WriteTo(TranslationWriter writer)
        {
            for (var i = 0; ;)
            {
                var operandTranslation = _operandTranslations[i];

                if ((operandTranslation.NodeType == ExpressionType.Conditional) || operandTranslation.IsAssignment())
                {
                    operandTranslation.WriteInParentheses(writer);
                }
                else
                {
                    operandTranslation.WriteTo(writer);
                }

                ++i;

                if (i == _operandCount)
                {
                    break;
                }

                writer.WriteToTranslation(" + ");
            }
        }
    }
}