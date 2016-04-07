namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class TranslationContext
    {
        private IEnumerable<ParameterExpression> _joinedAssignmentVariables;
        private IEnumerable<BinaryExpression> _joinedAssignments;

        public IEnumerable<ParameterExpression> JoinedAssignmentVariables
        {
            get
            {
                return _joinedAssignmentVariables ??
                    (_joinedAssignmentVariables = _joinedAssignments?
                        .Select(assignment => assignment.Left)
                        .Cast<ParameterExpression>()
                        .ToArray()
                    ?? Enumerable.Empty<ParameterExpression>());
            }
        }

        public void Process(BlockExpression block)
        {
            if (_joinedAssignments != null)
            {
                return;
            }

            _joinedAssignments = JoinedAssignmentVisitor.GetAssignments(block);
        }

        public bool IsNotJoinedAssignment(Expression expression)
        {
            return (expression.NodeType != ExpressionType.Assign) || !_joinedAssignments.Contains(expression);
        }

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
    }
}