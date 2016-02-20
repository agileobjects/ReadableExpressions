namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class BlockExpressionTranslator : ExpressionTranslatorBase
    {
        public BlockExpressionTranslator()
            : base(ExpressionType.Block)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
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
                    Translation = translatorRegistry.Translate(exp)
                })
                .Where(d => d.Translation != null)
                .Select(d => GetTerminatedStatement(d.Translation, d.Expression));

            lines = InsertBlockSpacing(lines.ToArray());

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
                translation.IsTerminated() ||
                (expression is CommentExpression))
            {
                return translation;
            }

            return translation + ";";
        }

        private static IEnumerable<string> InsertBlockSpacing(IList<string> lines)
        {
            var finalLine = lines.Last();

            foreach (var line in lines)
            {
                yield return line;

                if ((line != finalLine) && LeaveBlankLineAfter(line))
                {
                    yield return string.Empty;
                }
            }
        }

        private static bool LeaveBlankLineAfter(string line)
        {
            return line.EndsWith("}", StringComparison.Ordinal);
        }
    }
}