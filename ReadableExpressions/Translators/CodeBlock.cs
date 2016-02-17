namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CodeBlock
    {
        private const string Indent = "    ";

        private readonly IEnumerable<string> _blockLines;

        public CodeBlock(params string[] blockLines)
        {
            _blockLines = blockLines;
        }

        public bool IsASingleStatement => _blockLines.Count() == 1;

        public string AsExpressionBody()
        {
            var expression = _blockLines.First();

            if (expression.EndsWith(";", StringComparison.Ordinal))
            {
                expression = expression.TrimEnd(';');
            }

            return new CodeBlock(expression).WithoutBrackets();
        }

        public string WithoutBrackets()
        {
            return GetCodeBlock(_blockLines);
        }

        public string WithBrackets()
        {
            var codeBlock = GetCodeBlock(_blockLines
                .Select(line => line.EndsWith(";", StringComparison.Ordinal) ? line : line + ";")
                .Select(line => Indent + line));

            return $@"
{{
{codeBlock}
}}";
        }

        private static string GetCodeBlock(IEnumerable<string> lines)
        {
            return string.Join(Environment.NewLine, lines);
        }

        public override string ToString()
        {
            return WithBrackets();
        }
    }
}