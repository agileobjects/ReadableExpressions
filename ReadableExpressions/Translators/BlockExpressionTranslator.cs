namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    internal class BlockExpressionTranslator : ExpressionTranslatorBase
    {
        public BlockExpressionTranslator()
            : base(ExpressionType.Block)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var block = (BlockExpression)expression;

            var expressions = block
                .Expressions
                .Select(exp => new
                {
                    Translation = translatorRegistry.Translate(exp),
                    IsBlock = (exp.NodeType == ExpressionType.Block)
                })
                .Where(d => d.Translation != null)
                .Select(d => d.Translation + (d.IsBlock ? null : ";"))
                .ToArray();

            AddVariableDeclarations(block.Variables, expressions);

            return string.Join(Environment.NewLine, expressions);
        }

        private static void AddVariableDeclarations(
            IEnumerable<ParameterExpression> variables,
            IList<string> expressions)
        {
            foreach (var variable in variables)
            {
                var variableNameRegex = new Regex($"\\b{variable.Name}\\b");

                var variableFirstUse = expressions
                    .Select((exp, i) => new
                    {
                        Expression = exp,
                        Index = i,
                        Match = variableNameRegex.Match(exp)
                    })
                    .FirstOrDefault(exp => exp.Match.Success);

                if (variableFirstUse != null)
                {
                    expressions[variableFirstUse.Index] =
                        expressions[variableFirstUse.Index].Insert(variableFirstUse.Index, "var ");
                }
            }
        }
    }
}