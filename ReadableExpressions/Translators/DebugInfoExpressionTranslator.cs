namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;

    internal class DebugInfoExpressionTranslator : ExpressionTranslatorBase
    {
        public DebugInfoExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.DebugInfo)
        {
        }

        public override string Translate(Expression expression)
        {
            var debugInfo = (DebugInfoExpression)expression;

            var debugInfoText = string.Format(
                CultureInfo.InvariantCulture,
                "Debug outputs to {0}, {1}, {2} -> {3}, {4}",
                debugInfo.Document.FileName,
                debugInfo.StartLine,
                debugInfo.StartColumn,
                debugInfo.EndLine,
                debugInfo.EndColumn);

            var debugInfoComment = Registry.Translate(ReadableExpression.Comment(debugInfoText));

            return debugInfoComment + Environment.NewLine;
        }
    }
}