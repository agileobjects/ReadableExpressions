namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class TranslationContext
    {
        private ExpressionAnalysisVisitor _analyzer;

        private ExpressionAnalysisVisitor Analyzer => _analyzer ?? (_analyzer = new ExpressionAnalysisVisitor());

        public IEnumerable<ParameterExpression> JoinedAssignmentVariables => _analyzer.AssignedVariables;

        public void Process(BlockExpression block)
        {
            Analyzer.Process(block);
        }

        public bool IsNotJoinedAssignment(Expression expression)
        {
            return (expression.NodeType != ExpressionType.Assign) ||
                !_analyzer.JoinedAssignments.Contains(expression);
        }

        public bool IsReferencedByGoto(LabelTarget labelTarget)
        {
            return _analyzer.NamedLabelTargets.Contains(labelTarget);
        }

        private class ExpressionAnalysisVisitor : ExpressionVisitor
        {
            private readonly List<BlockExpression> _processedBlocks;
            private readonly List<ParameterExpression> _accessedVariables;
            private readonly List<ParameterExpression> _assignedVariables;
            private readonly List<BinaryExpression> _joinedAssignments;
            private readonly List<LabelTarget> _namedLabelTargets;

            public ExpressionAnalysisVisitor()
            {
                _processedBlocks = new List<BlockExpression>();
                _accessedVariables = new List<ParameterExpression>();
                _assignedVariables = new List<ParameterExpression>();
                _joinedAssignments = new List<BinaryExpression>();
                _namedLabelTargets = new List<LabelTarget>();
            }

            public IEnumerable<ParameterExpression> AssignedVariables => _assignedVariables;

            public IEnumerable<BinaryExpression> JoinedAssignments => _joinedAssignments;

            public IEnumerable<LabelTarget> NamedLabelTargets => _namedLabelTargets;

            public void Process(Expression block)
            {
                if (!_processedBlocks.Contains(block))
                {
                    Visit(block);
                }
            }

            protected override Expression VisitBlock(BlockExpression block)
            {
                _processedBlocks.Add(block);

                return base.VisitBlock(block);
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
                    var variable = (ParameterExpression)binaryExpression.Left;

                    if (VariableHasNotYetBeenAccessed(variable))
                    {
                        _joinedAssignments.Add(binaryExpression);
                        _assignedVariables.Add(variable);
                    }
                }

                return base.VisitBinary(binaryExpression);
            }

            private bool VariableHasNotYetBeenAccessed(Expression variable)
            {
                return !_accessedVariables.Contains(variable);
            }

            protected override Expression VisitGoto(GotoExpression @goto)
            {
                if (@goto.Kind == GotoExpressionKind.Goto)
                {
                    _namedLabelTargets.Add(@goto.Target);
                }

                return base.VisitGoto(@goto);
            }
        }
    }
}