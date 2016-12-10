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
            IsASingleStatement = IsSingleStatement(_blockLines);
        }

        public static bool IsSingleStatement(IList<string> blockLines)
        {
            if (blockLines.Count != 1)
            {
                return false;
            }

            if (!blockLines[0].Contains(";" + Environment.NewLine))
            {
                return true;
            }

            var numberOfNonIndentedLines = blockLines[0]
                .SplitToLines()
                .Count(line => line.IsNotIndented() && !line.StartsWith('{') && !line.StartsWith('}'));

            return numberOfNonIndentedLines == 1;
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

        public string WithCurlyBracesIfMultiStatement()
        {
            return IsASingleStatement ? WithoutCurlyBraces().Unterminated() : WithCurlyBraces();
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

            return new CodeBlock(_expression, GetBlockLinesWithReturnKeyword(_blockLines));
        }

        public static string InsertReturnKeyword(string multiLineStatement)
        {
            var lines = multiLineStatement.SplitToLines();

            return string.Join(Environment.NewLine, GetBlockLinesWithReturnKeyword(lines));
        }

        private static string[] GetBlockLinesWithReturnKeyword(string[] lines)
        {
            var blockInfo = new BlockInfo(lines);

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
            private readonly string _lastNonIndentedStatement;
            private readonly string[] _lastStatementLines;
            private readonly string _lastNonIndentedLine;

            public BlockInfo(string[] blockLines)
            {
                _blockLines = blockLines;
                _lastNonIndentedStatement = blockLines.Last(line => line.IsNotIndented());

                _lastStatementLines = _lastNonIndentedStatement.SplitToLines();
                _lastNonIndentedLine = _lastStatementLines.Last(line => line.IsNotIndented());
            }

            public bool LastLineHasReturnKeyword =>
                _lastNonIndentedLine.StartsWith("return ", StringComparison.Ordinal);

            public string[] GetBlockLinesWithReturnKeyword()
            {
                var updatedBlockLines = new List<string>();

                var lastNonIndentedStatementIndex = Array.IndexOf(_blockLines, _lastNonIndentedStatement);
                var preLastStatementStatements = _blockLines.Take(lastNonIndentedStatementIndex);

                updatedBlockLines.AddRange(preLastStatementStatements);

                var lastNonIndentedLineIndex = Array.IndexOf(_lastStatementLines, _lastNonIndentedLine);

                var preLastLineLines = _lastStatementLines
                    .Take(lastNonIndentedLineIndex)
                    .ToArray();

                var postLastLineLines = _lastStatementLines
                    .Skip(preLastLineLines.Length + 1);

                updatedBlockLines.AddRange(preLastLineLines);
                updatedBlockLines.Add("return " + _lastNonIndentedLine);
                updatedBlockLines.AddRange(postLastLineLines);

                var finalIndentedLines = _blockLines.Skip(lastNonIndentedStatementIndex + 1);
                updatedBlockLines.AddRange(finalIndentedLines);

                return updatedBlockLines.ToArray();
            }
        }
    }
}