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

            var joinedAssignments = JoinedAssignmentVisitor.GetAssignments(block);
            var variables = GetVariableDeclarations(block, joinedAssignments);
            var lines = GetBlockLines(block, joinedAssignments);

            lines = ProcessBlockContents(lines.ToArray(), block.Expressions.Last());

            var blockContents = variables.Concat(lines);

            return string.Join(Environment.NewLine, blockContents);
        }

        private static IEnumerable<string> GetVariableDeclarations(
            BlockExpression block,
            IEnumerable<BinaryExpression> joinedAssignments)
        {
            var joinedAssignmentVariables = joinedAssignments
                .Select(a => a.Left)
                .Cast<ParameterExpression>();

            return block
                .Variables
                .Except(joinedAssignmentVariables)
                .GroupBy(v => v.Type)
                .Select(vGrp => $"{vGrp.Key.GetFriendlyName()} {string.Join(", ", vGrp)};");
        }

        private IEnumerable<string> GetBlockLines(
            BlockExpression block,
            IEnumerable<BinaryExpression> joinedAssignments)
        {
            return block
                .Expressions
                .Where(exp => Include(exp) || (exp == block.Result))
                .Select(exp => new
                {
                    Expression = exp,
                    Translation = GetTerminatedStatementOrNull(Registry.Translate(exp), exp, joinedAssignments)
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
            IEnumerable<BinaryExpression> joinedAssignments)
        {
            if (translation == null)
            {
                return null;
            }

            if (StatementIsTerminated(translation, expression))
            {
                return translation;
            }

            translation += ";";

            if ((expression.NodeType != ExpressionType.Assign) || !joinedAssignments.Contains(expression))
            {
                return translation;
            }

            var typeName = GetVariableTypeName((BinaryExpression)expression);

            return typeName + " " + translation;
        }

        private static bool StatementIsTerminated(string translation, Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Block:
                case ExpressionType.Lambda:
                    return true;

                case ExpressionType.Assign:
                    return false;
            }

            if (translation.IsTerminated())
            {
                return true;
            }

            return expression is CommentExpression;
        }

        private static string GetVariableTypeName(BinaryExpression assignment)
        {
            return assignment.Right.NodeType == ExpressionType.Lambda
                ? assignment.Right.Type.GetFriendlyName()
                : "var";
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

        private class JoinedAssignmentVisitor : ExpressionVisitor
        {
            private readonly List<Expression> _assignedVariables;
            private readonly List<Expression> _accessedVariables;
            private readonly List<BinaryExpression> _assignments;

            private JoinedAssignmentVisitor()
            {
                _assignedVariables = new List<Expression>();
                _accessedVariables = new List<Expression>();
                _assignments = new List<BinaryExpression>();
            }

            public static IEnumerable<BinaryExpression> GetAssignments(Expression block)
            {
                var visitor = new JoinedAssignmentVisitor();

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