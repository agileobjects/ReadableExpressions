namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;

    internal class CodeBlock
    {
        public const string Indent = "    ";

        private readonly string[] _blockLines;

        public CodeBlock(Type returnType, params string[] blockLines)
        {
            ReturnType = returnType;
            _blockLines = blockLines;
        }

        public Type ReturnType { get; }

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
            return new CodeBlock(
                ReturnType,
                _blockLines.Select(line => Indent + line).ToArray());
        }

        public string WithoutBrackets()
        {
            return GetCodeBlock();
        }

        public string WithBrackets()
        {
            var codeBlock = Indented().GetCodeBlock();

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
            if (IsASingleStatement &&
                !_blockLines[0].EndsWith(";", StringComparison.Ordinal))
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