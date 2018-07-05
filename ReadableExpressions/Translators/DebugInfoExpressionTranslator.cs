namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Globalization;
#if !NET35
    using System.Linq.Expressions;
#else
    using DebugInfoExpression = Microsoft.Scripting.Ast.DebugInfoExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;

#endif

    internal struct DebugInfoExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.DebugInfo; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var debugInfo = (DebugInfoExpression)expression;

            string debugInfoText;

            if (debugInfo.IsClear)
            {
                debugInfoText = $"Clear debug info from {debugInfo.Document.FileName}";
            }
            else
            {
                debugInfoText = string.Format(
                    CultureInfo.InvariantCulture,
                    "Debug to {0}, {1}, {2} -> {3}, {4}",
                    debugInfo.Document.FileName,
                    debugInfo.StartLine,
                    debugInfo.StartColumn,
                    debugInfo.EndLine,
                    debugInfo.EndColumn);
            }

            var debugInfoComment = ReadableExpression.Comment(debugInfoText);

            return context.Translate(debugInfoComment);
        }
    }
}