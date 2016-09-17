namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Collections.Generic;
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

            IsASingleStatement = (_blockLines.Length == 1) && !_blockLines[0].Contains(";" + Environment.NewLine);
        }

        public bool IsASingleStatement { get; }

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
                _blockLines.Select(line => line.Indented()).ToArray());
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

        public string WithoutCurlyBraces()
        {
            return GetCodeBlock();
        }

        public bool HasReturn()
        {
            if (ExpressionHasReturn(_expression))
            {
                return true;
            }

            var blockInfo = new BlockInfo(_blockLines);

            return blockInfo.LastLineHasReturnKeyword;
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

        public string WithCurlyBraces()
        {
            if (_blockLines.Length == 0)
            {
                return @"
{
}";
            }

            var codeBlock = WithReturn().Indented().GetCodeBlock();

            if (codeBlock.StartsWithNewLine())
            {
                codeBlock = codeBlock.Substring(Environment.NewLine.Length);
            }

            return $@"
{{
{codeBlock}
}}";
        }

        private CodeBlock WithReturn()
        {
            if (!_expression.IsReturnable() || HasReturn())
            {
                return this;
            }

            return new CodeBlock(_expression, GetBlockLinesWithInsert());
        }

        private string[] GetBlockLinesWithInsert()
        {
            var blockInfo = new BlockInfo(_blockLines);

            return blockInfo.GetBlockLinesWithReturnKeyword();
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

        private class BlockInfo
        {
            private readonly string[] _blockLines;
            private readonly int _lastNonIndentedStatementIndex;
            private readonly string _lastNonIndentedLine;
            private readonly int _lastNonIndentedLineIndex;

            public BlockInfo(string[] blockLines)
            {
                _blockLines = blockLines;
                var lastNonIndentedStatement = blockLines.Last(line => line.IsNotIndented());
                _lastNonIndentedStatementIndex = Array.IndexOf(blockLines, lastNonIndentedStatement);

                var lastStatementLines = lastNonIndentedStatement.SplitToLines();
                _lastNonIndentedLine = lastStatementLines.Last(line => line.IsNotIndented());
                _lastNonIndentedLineIndex = Array.IndexOf(lastStatementLines, _lastNonIndentedLine);
            }

            public bool LastLineHasReturnKeyword =>
                _lastNonIndentedLine.StartsWith("return ", StringComparison.Ordinal);

            public string[] GetBlockLinesWithReturnKeyword()
            {
                var lastNonIndentedStatement = _blockLines.Last(line => line.IsNotIndented());

                var updatedBlockLines = new List<string>();

                var preLastStatementStatements = _blockLines
                    .Take(_blockLines.Length - (_lastNonIndentedStatementIndex + 1));

                updatedBlockLines.AddRange(preLastStatementStatements);

                var lastStatementLines = lastNonIndentedStatement.SplitToLines();

                var preLastLineLines = lastStatementLines
                    .Take(_lastNonIndentedLineIndex)
                    .ToArray();

                var postLastLineLines = lastStatementLines
                    .Skip(preLastLineLines.Length + 1);

                updatedBlockLines.AddRange(preLastLineLines);
                updatedBlockLines.Add("return " + _lastNonIndentedLine);
                updatedBlockLines.AddRange(postLastLineLines);

                return updatedBlockLines.ToArray();
            }
        }
    }
}