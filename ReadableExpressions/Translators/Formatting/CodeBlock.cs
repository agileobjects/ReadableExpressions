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

            if (!expression.StartsWithNewLine())
            {
                expression = " " + expression;
            }

            return expression.Unterminated();
        }

        public CodeBlock Indented()
        {
            return new CodeBlock(_blockLines.Select(line => line.Indent()).ToArray());
        }

        public string WithoutParentheses()
        {
            return GetCodeBlock();
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