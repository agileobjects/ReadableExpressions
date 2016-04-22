namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class CodeBlock
    {
        private readonly Expression _expression;
        private readonly string[] _blockLines;

        public CodeBlock(
            Expression expression,
            params string[] blockLines)
        {
            _expression = expression;
            _blockLines = blockLines.Where(line => line != null).ToArray();
        }

        public bool IsASingleStatement => _blockLines.Length == 1;

        public string AsExpressionBody()
        {
            var expression = _blockLines.First();

            if (!expression.StartsWithNewLine())
            {
                expression = " " + expression;
            }

            return expression.Unterminated();
        }

        public CodeBlock Indented()
        {
            AddSemiColonIfRequired();

            return new CodeBlock(
                _expression,
                _blockLines.Select(line => line.Indent()).ToArray());
        }

        public CodeBlock Insert(params string[] lines)
        {
            return new CodeBlock(_expression, lines.Concat(_blockLines).ToArray());
        }

        public CodeBlock Append(params string[] lines)
        {
            AddSemiColonIfRequired();

            return new CodeBlock(_expression, _blockLines.Concat(lines).ToArray());
        }

        public string WithoutParentheses()
        {
            return GetCodeBlock();
        }

        public bool HasReturn()
        {
            return _blockLines.Last().StartsWith("return ", StringComparison.Ordinal) ||
                ExpressionHasReturn(_expression);
        }

        private static bool ExpressionHasReturn(Expression expression)
        {
            while (true)
            {
                var block = expression as BlockExpression;

                if (block == null)
                {
                    return expression.NodeType == ExpressionType.Goto;
                }

                expression = block.Expressions.Last();
            }
        }

        public CodeBlock WithReturn()
        {
            if (!_expression.IsReturnable() || HasReturn())
            {
                return this;
            }

            return new CodeBlock(
                _expression,
                _blockLines
                    .Take(_blockLines.Length - 1)
                    .Concat(new[] { "return " + _blockLines.Last() })
                    .ToArray());
        }

        public string WithParentheses()
        {
            if (_blockLines.Length == 0)
            {
                return @"
{
}";
            }

            var codeBlock = Indented().GetCodeBlock();

            if (codeBlock.StartsWithNewLine())
            {
                codeBlock = codeBlock.Substring(Environment.NewLine.Length);
            }

            return $@"
{{
{codeBlock}
}}";
        }

        private string GetCodeBlock()
        {
            AddSemiColonIfRequired();

            return string.Join(Environment.NewLine, _blockLines);
        }

        private void AddSemiColonIfRequired()
        {
            if (IsASingleStatement && !_blockLines[0].IsTerminated())
            {
                _blockLines[0] += ";";
            }
        }

        public override string ToString()
        {
            return WithParentheses();
        }
    }
}