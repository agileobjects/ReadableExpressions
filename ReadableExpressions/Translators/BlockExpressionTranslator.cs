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

            var varAssignments = VarAssignmentVisitor.GetAssignments(block);
            var variables = GetVariableDeclarations(block, varAssignments);
            var lines = GetBlockLines(block, varAssignments);

            lines = ProcessBlockContents(lines.ToArray(), block.Expressions.Last());

            var blockContents = variables.Concat(lines);

            return string.Join(Environment.NewLine, blockContents);
        }

        private static IEnumerable<string> GetVariableDeclarations(
            BlockExpression block,
            IEnumerable<BinaryExpression> varAssignments)
        {
            var variablesDeclaredWithVar = varAssignments
                .Select(a => a.Left)
                .Cast<ParameterExpression>();

            return block
                .Variables
                .Except(variablesDeclaredWithVar)
                .GroupBy(v => v.Type)
                .Select(vGrp => $"{vGrp.Key.GetFriendlyName()} {string.Join(", ", vGrp)};");
        }

        private IEnumerable<string> GetBlockLines(
            BlockExpression block,
            IEnumerable<BinaryExpression> varAssignments)
        {
            return block
                .Expressions
                .Where(exp => Include(exp) || (exp == block.Result))
                .Select(exp => new
                {
                    Expression = exp,
                    Translation = GetTerminatedStatementOrNull(Registry.Translate(exp), exp, varAssignments)
                })
                .Where(d => d.Translation != null)
                .Select(d => d.Translation);
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

        private static string GetTerminatedStatementOrNull(
            string translation,
            Expression expression,
            IEnumerable<BinaryExpression> varAssignments)
        {
            if (translation == null)
            {
                return null;
            }

            if ((expression.NodeType == ExpressionType.Block) ||
                (expression.NodeType == ExpressionType.Lambda) ||
                translation.IsTerminated() ||
                (expression is CommentExpression))
            {
                return translation;
            }

            translation += ";";

            if ((expression.NodeType == ExpressionType.Assign) &&
                varAssignments.Contains(expression))
            {
                translation = "var " + translation;
            }

            return translation;
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

        #region Helper Classes

        private class VarAssignmentVisitor : ExpressionVisitor
        {
            private readonly List<Expression> _assignedVariables;
            private readonly List<Expression> _accessedVariables;
            private readonly List<BinaryExpression> _assignments;

            private VarAssignmentVisitor()
            {
                _assignedVariables = new List<Expression>();
                _accessedVariables = new List<Expression>();
                _assignments = new List<BinaryExpression>();
            }

            public static IEnumerable<BinaryExpression> GetAssignments(Expression block)
            {
                var visitor = new VarAssignmentVisitor();

                visitor.Visit(block);

                return visitor._assignments;
            }

            protected override Expression VisitParameter(ParameterExpression variable)
            {
                if (VariableHasNotYetBeenAccessed(variable))
                {
                    _accessedVariables.Add(variable);
                }

                return base.VisitParameter(variable);
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                if ((binaryExpression.NodeType == ExpressionType.Assign) &&
                    (binaryExpression.Left.NodeType == ExpressionType.Parameter) &&
                    !_assignedVariables.Contains(binaryExpression.Left))
                {
                    var variable = binaryExpression.Left;

                    if (VariableHasNotYetBeenAccessed(variable))
                    {
                        _assignments.Add(binaryExpression);
                        _assignedVariables.Add(variable);
                    }
                }

                return base.VisitBinary(binaryExpression);
            }

            private bool VariableHasNotYetBeenAccessed(Expression variable)
            {
                return !_accessedVariables.Contains(variable);
            }
        }

        #endregion
    }
}