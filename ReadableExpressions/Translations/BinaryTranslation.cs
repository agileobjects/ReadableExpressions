namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class BinaryTranslation : CheckedOperationTranslationBase, ITranslation
    {
        private readonly ITranslationContext _context;
        private readonly ITranslation _leftOperandTranslation;
        private readonly string _operator;
        private readonly ITranslation _rightOperandTranslation;

        private BinaryTranslation(BinaryExpression binary, ITranslationContext context)
            : base(IsCheckedBinary(binary.NodeType), "(", ")")
        {
            _context = context;
            NodeType = binary.NodeType;
            Type = binary.Type;
            _leftOperandTranslation = context.GetTranslationFor(binary.Left);
            _operator = GetOperator(binary);
            _rightOperandTranslation = context.GetTranslationFor(binary.Right);
            TranslationSize = GetTranslationSize();
            FormattingSize = _leftOperandTranslation.FormattingSize + _rightOperandTranslation.FormattingSize;
        }

        public static ITranslation For(BinaryExpression binary, ITranslationContext context)
        {
            switch (binary.NodeType)
            {
                case Add:
                    if (binary.Type != typeof(string))
                    {
                        break;
                    }

                    return StringConcatenationTranslation.ForAddition(binary, context);

                case Equal:
                case NotEqual:
                    if (StandaloneEqualityComparisonTranslation.TryGetTranslation(binary, context, out var translation))
                    {
                        return translation;
                    }

                    break;
            }

            return new BinaryTranslation(binary, context);
        }

        private static bool IsCheckedBinary(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case AddChecked:
                case MultiplyChecked:
                case SubtractChecked:
                    return true;
            }

            return false;
        }

        public static string GetOperator(Expression expression) => GetOperator(expression.NodeType);

        private static string GetOperator(ExpressionType nodeType)
        {
            return nodeType switch
            {
                Add => " + ",
                AddChecked => " + ",
                And => " & ",
                AndAlso => " && ",
                Coalesce => " ?? ",
                Divide => " / ",
                Equal => " == ",
                ExclusiveOr => " ^ ",
                GreaterThan => " > ",
                GreaterThanOrEqual => " >= ",
                LeftShift => " << ",
                LessThan => " < ",
                LessThanOrEqual => " <= ",
                Modulo => " % ",
                Multiply => " * ",
                MultiplyChecked => " * ",
                NotEqual => " != ",
                Or => " | ",
                OrElse => " || ",
                Power => " ** ",
                RightShift => " >> ",
                Subtract => " - ",
                SubtractChecked => " - ",
                _ => null
            };
        }

        public static bool IsBinary(ExpressionType nodeType) => GetOperator(nodeType) != null;

        private int GetTranslationSize()
        {
            var translationSize =
                _leftOperandTranslation.TranslationSize +
                _operator.Length +
                _rightOperandTranslation.TranslationSize;

            if (IsCheckedOperation)
            {
                translationSize += 10;
            }

            return translationSize;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetIndentSize()
        {
            var indentSize = 
                _leftOperandTranslation.GetIndentSize() + 
                _rightOperandTranslation.GetIndentSize();

            if (IsCheckedOperation && IsMultiStatement())
            {
                var indentLength = _context.Settings.IndentLength;

                indentSize +=
                    _leftOperandTranslation.GetLineCount() * indentLength +
                    _rightOperandTranslation.GetLineCount() * indentLength;
            }

            return indentSize;
        }

        public int GetLineCount()
        {
            var isMultiStatement = IsMultiStatement();
            var lineCount = IsCheckedOperation && isMultiStatement ? 2 : 1;

            var leftOperandLineCount = _leftOperandTranslation.GetLineCount();

            if (isMultiStatement)
            {
                lineCount += leftOperandLineCount;
            }
            else if (leftOperandLineCount > 1)
            {
                lineCount = leftOperandLineCount - 1;
            }

            var rightOperandLineCount = _rightOperandTranslation.GetLineCount();

            if (isMultiStatement)
            {
                lineCount += rightOperandLineCount;
            }
            else if (rightOperandLineCount > 1)
            {
                lineCount = rightOperandLineCount - 1;
            }

            return lineCount;
        }

        public void WriteTo(TranslationWriter writer)
        {
            WriteOpeningCheckedIfNecessary(writer, out var isMultiStatementChecked);
            _leftOperandTranslation.WriteInParenthesesIfRequired(writer, _context);
            writer.WriteToTranslation(_operator);
            _rightOperandTranslation.WriteInParenthesesIfRequired(writer, _context);
            WriteClosingCheckedIfNecessary(writer, isMultiStatementChecked);
        }

        protected override bool IsMultiStatement()
            => _leftOperandTranslation.IsMultiStatement() || _rightOperandTranslation.IsMultiStatement();

        private class StandaloneEqualityComparisonTranslation : ITranslation
        {
            private readonly ITranslationContext _context;
            private readonly StandaloneBoolean _standaloneBoolean;
            private readonly ITranslation _operandTranslation;

            private StandaloneEqualityComparisonTranslation(
                ExpressionType nodeType,
                Expression boolean,
                ExpressionType @operator,
                Expression comparison,
                ITranslationContext context)
            {
                _context = context;
                NodeType = nodeType;
                _standaloneBoolean = new StandaloneBoolean(boolean, @operator, comparison);
                _operandTranslation = context.GetTranslationFor(_standaloneBoolean.Expression);
                TranslationSize = _operandTranslation.TranslationSize + 1;
            }

            public static bool TryGetTranslation(BinaryExpression comparison, ITranslationContext context, out ITranslation translation)
            {
                if (IsBooleanConstant(comparison.Right))
                {
                    translation = new StandaloneEqualityComparisonTranslation(
                        comparison.NodeType,
                        comparison.Left,
                        comparison.NodeType,
                        comparison.Right,
                        context);

                    return true;
                }

                if (IsBooleanConstant(comparison.Left))
                {
                    translation = new StandaloneEqualityComparisonTranslation(
                        comparison.NodeType,
                        comparison.Right,
                        comparison.NodeType,
                        comparison.Left,
                        context);

                    return true;
                }

                translation = null;
                return false;
            }

            private static bool IsBooleanConstant(Expression expression)
            {
                return ((expression.NodeType == Constant) || (expression.NodeType == Default)) &&
                        (expression.Type == typeof(bool));
            }

            public ExpressionType NodeType { get; }

            public Type Type => typeof(bool);

            public int TranslationSize { get; }

            public int FormattingSize => _operandTranslation.FormattingSize;

            public int GetIndentSize() => _operandTranslation.GetIndentSize();

            public int GetLineCount() => _operandTranslation.GetLineCount();

            public void WriteTo(TranslationWriter writer)
            {
                if (_standaloneBoolean.IsComparisonToTrue)
                {
                    _operandTranslation.WriteTo(writer);
                    return;
                }

                NegationTranslation.ForNot(_operandTranslation, _context).WriteTo(writer);
            }

            private class StandaloneBoolean
            {
                public StandaloneBoolean(
                    Expression boolean,
                    ExpressionType @operator,
                    Expression comparison)
                {
                    Expression = boolean;

                    var comparisonValue =
                        (comparison.NodeType != Default) &&
                        (bool)((ConstantExpression)comparison).Value;

                    IsComparisonToTrue =
                        (comparisonValue && (@operator == Equal)) ||
                       (!comparisonValue && (@operator == NotEqual));
                }

                public Expression Expression { get; }

                public bool IsComparisonToTrue { get; }
            }
        }
    }
}