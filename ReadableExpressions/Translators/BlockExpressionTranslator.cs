namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
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

            var expressions = block
                .Expressions
                .Where(exp => IncludeExpression(exp) || (exp == block.Result))
                .Select(exp => new
                {
                    Translation = translatorRegistry.Translate(exp),
                    IsStatement = IsStatement(exp)
                })
                .Where(d => d.Translation != null)
                .Select(d => d.Translation + (d.IsStatement ? ";" : null));

            var blockContents = variables.Concat(expressions);

            return string.Join(Environment.NewLine, blockContents);
        }

        private static bool IncludeExpression(Expression expression)
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

        private static bool IsStatement(Expression expression)
        {
            return !((expression.NodeType == ExpressionType.Block) || (expression is CommentExpression));
        }
    }
}