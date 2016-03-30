namespace AgileObjects.ReadableExpressions.Translators.Formatting
{
    using System;
    using System.Linq;

    internal class CodeBlock
    {
        private readonly string[] _blockLines;

        public CodeBlock(params string[] blockLines)
        {
            _blockLines = blockLines.Where(line => line != null).ToArray();
        }

        public bool IsASingleStatement => _blockLines.Length == 1;

        public string AsExpressionBody()
        {
            var expression = _blockLines.First();

            if (!expression.StartsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                expression = " " + expression;
            }

            if (expression.EndsWith(";", StringComparison.Ordinal))
            {
                expression = expression.TrimEnd(';');
            }

            return expression;
        }

        public CodeBlock Indented()
        {
            return new CodeBlock(_blockLines.Select(line => line.Indent()).ToArray());
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