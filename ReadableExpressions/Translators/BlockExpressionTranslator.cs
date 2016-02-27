namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class BlockExpressionTranslator : ExpressionTranslatorBase
    {
        public BlockExpressionTranslator(IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Block)
        {
        }

        public override string Translate(Expression expression)
        {
            var block = (BlockExpression)expression;

            var variables = block
                .Variables
                .GroupBy(v => v.Type)
                .Select(vGrp => $"{vGrp.Key.GetFriendlyName()} {string.Join(", ", vGrp)};");

            var lines = block
                .Expressions
                .Where(exp => Include(exp) || (exp == block.Result))
                .Select(exp => new
                {
                    Expression = exp,
                    Translation = Registry.Translate(exp)
                })
                .Where(d => d.Translation != null)
                .Select(d => GetTerminatedStatement(d.Translation, d.Expression));

            lines = ProcessBlockContents(lines.ToArray(), block.Expressions.Last());

            var blockContents = variables.Concat(lines);

            return string.Join(Environment.NewLine, blockContents);
        }

        private static bool Include(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Parameter)
            {
                return false;
            }

            if (expression.NodeType != ExpressionType.Constant)
            {
                return true;
            }

            return expression is CommentExpression;
        }

        private static string GetTerminatedStatement(string translation, Expression expression)
        {
            if ((expression.NodeType == ExpressionType.Block) ||
                (expression.NodeType == ExpressionType.Lambda) ||
                translation.IsTerminated() ||
                (expression is CommentExpression))
            {
                return translation;
            }

            return translation + ";";
        }

        private static IEnumerable<string> ProcessBlockContents(IList<string> lines, Expression finalExpression)
        {
            var finalLineIndex = lines.Count - 1;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (i != finalLineIndex)
                {
                    yield return line;

                    if (LeaveBlankLineAfter(line, lines[i + 1]))
                    {
                        yield return string.Empty;
                    }

                    continue;
                }

                yield return (finalExpression.NodeType != ExpressionType.Parameter)
                    ? line : "return " + line;
            }
        }

        private static bool LeaveBlankLineAfter(string line, string nextLine)
        {
            return line.EndsWith("}", StringComparison.Ordinal) &&
                !(string.IsNullOrEmpty(nextLine) || nextLine.StartsWith(Environment.NewLine));
        }
    }
}