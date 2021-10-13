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
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class StringConcatenationTranslation : ITranslation
    {
        private readonly int _operandCount;
        private readonly IList<ITranslation> _operandTranslations;

        private StringConcatenationTranslation(
            ExpressionType nodeType,
            int operandCount,
            IList<Expression> operands,
            ITranslationContext context)
        {
            NodeType = nodeType;

            var translationSize = 0;
            var formattingSize = 0;
            _operandCount = operandCount;
            _operandTranslations = new ITranslation[operandCount];

            for (var i = 0; i < operandCount; ++i)
            {
                var operand = operands[i];

                if (operand.NodeType == Call)
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

        public static ITranslation ForAddition(
            BinaryExpression addition,
            ITranslationContext context)
        {
            return new StringConcatenationTranslation(
                Add,
                operandCount: 2,
                new[] { addition.Left, addition.Right },
                context);
        }

        public static bool TryCreateForConcatCall(
            MethodCallExpression methodCall,
            ITranslationContext context,
            out ITranslation concatTranslation)
        {
            if (!IsStringConcatCall(methodCall))
            {
                concatTranslation = null;
                return false;
            }

            var operands = methodCall.Arguments;
            var operandCount = operands.Count;

            if (operandCount == 1 && operands.First().NodeType == NewArrayInit)
            {
                operands = ((NewArrayExpression)operands.First()).Expressions;
                operandCount = operands.Count;
            }

            concatTranslation =
                new StringConcatenationTranslation(Call, operandCount, operands, context);

            return true;
        }

        private static bool IsStringConcatCall(MethodCallExpression methodCall)
        {
            return methodCall.Method.IsStatic &&
                  (methodCall.Method.DeclaringType == typeof(string)) &&
                  (methodCall.Method.Name == nameof(string.Concat));
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

                if ((operandTranslation.NodeType == Conditional) || operandTranslation.IsAssignment())
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