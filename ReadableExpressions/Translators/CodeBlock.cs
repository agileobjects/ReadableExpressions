namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;

    internal class CodeBlock
    {
        private static readonly string[] _newLines = { Environment.NewLine };

        private readonly string[] _blockLines;

        public CodeBlock(params string[] blockLines)
        {
            _blockLines = blockLines.Where(line => line != null).ToArray();
        }

        public bool IsASingleStatement => _blockLines.Length == 1;

        public string AsExpressionBody()
        {
            var expression = _blockLines.First();

            if (expression.EndsWith(";", StringComparison.Ordinal))
            {
                expression = expression.TrimEnd(';');
            }

            return expression;
        }

        public CodeBlock Indented()
        {
            return new CodeBlock(_blockLines.Select(Indent).ToArray());
        }

        private static string Indent(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            if (line.Contains(Environment.NewLine))
            {
                return string.Join(
                    Environment.NewLine,
                    line.Split(_newLines, StringSplitOptions.None).Select(Indent));
            }

            return line.Indented();
        }

        public string WithoutBrackets()
        {
            return GetCodeBlock();
        }

        public string WithBrackets()
        {
            if (_blockLines.Length == 0)
            {
                return @"
{
}";
            }

            var codeBlock = Indented().GetCodeBlock();

            if (codeBlock.StartsWith(Environment.NewLine, StringComparison.Ordinal))
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
            return WithBrackets();
        }
    }
}