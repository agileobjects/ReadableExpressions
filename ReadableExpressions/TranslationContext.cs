namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    public class TranslationContext
    {
        private readonly ExpressionAnalysisVisitor _analyzer;

        private TranslationContext(ExpressionAnalysisVisitor analyzer)
        {
            _analyzer = analyzer;
        }

        public static TranslationContext For(Expression expression)
        {
            return new TranslationContext(ExpressionAnalysisVisitor.Analyse(expression));
        }

        public IEnumerable<ParameterExpression> JoinedAssignmentVariables => _analyzer.AssignedVariables;

        public bool IsNotJoinedAssignment(Expression expression)
        {
            return (expression.NodeType != ExpressionType.Assign) ||
                !_analyzer.JoinedAssignments.Contains(expression);
        }

        public bool IsReferencedByGoto(LabelTarget labelTarget)
        {
            return _analyzer.NamedLabelTargets.Contains(labelTarget);
        }

        public bool IsPartOfMethodCallChain(Expression methodCall)
        {
            return _analyzer.ChainedMethodCalls.Contains(methodCall);
        }

        private class ExpressionAnalysisVisitor : ExpressionVisitor
        {
            private readonly List<ParameterExpression> _accessedVariables;
            private readonly List<ParameterExpression> _assignedVariables;
            private readonly List<Expression> _assignedAssignments;
            private readonly List<BinaryExpression> _joinedAssignments;
            private readonly List<LabelTarget> _namedLabelTargets;
            private readonly List<MethodCallExpression> _chainedMethodCalls;

            private ExpressionAnalysisVisitor()
            {
                _accessedVariables = new List<ParameterExpression>();
                _assignedVariables = new List<ParameterExpression>();
                _assignedAssignments = new List<Expression>();
                _joinedAssignments = new List<BinaryExpression>();
                _namedLabelTargets = new List<LabelTarget>();
                _chainedMethodCalls = new List<MethodCallExpression>();
            }

            #region Factory Method

            public static ExpressionAnalysisVisitor Analyse(Expression expression)
            {
                var analyzer = new ExpressionAnalysisVisitor();

                var coreExpression = GetCoreExpression(expression);

                switch (coreExpression.NodeType)
                {
                    case ExpressionType.Block:
                    case ExpressionType.Call:
                    case ExpressionType.Conditional:
                        analyzer.Visit(coreExpression);
                        break;
                }

                return analyzer;
            }

            private static Expression GetCoreExpression(Expression expression)
            {
                var coreExpression = expression;

                while (true)
                {
                    if (coreExpression.NodeType == ExpressionType.Lambda)
                    {
                        coreExpression = ((LambdaExpression)coreExpression).Body;
                        continue;
                    }

                    var unary = coreExpression as UnaryExpression;

                    if (unary == null)
                    {
                        break;
                    }

                    coreExpression = unary.Operand;
                }

                return coreExpression;
            }

            #endregion

            public IEnumerable<ParameterExpression> AssignedVariables => _assignedVariables;

            public IEnumerable<BinaryExpression> JoinedAssignments => _joinedAssignments;

            public IEnumerable<LabelTarget> NamedLabelTargets => _namedLabelTargets;

            public IEnumerable<MethodCallExpression> ChainedMethodCalls => _chainedMethodCalls;

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
                    !_assignedVariables.Contains(binaryExpression.Left) &&
                    !_assignedAssignments.Contains(binaryExpression))
                {
                    var variable = (ParameterExpression)binaryExpression.Left;

                    if (VariableHasNotYetBeenAccessed(variable))
                    {
                        _joinedAssignments.Add(binaryExpression);
                        _accessedVariables.Add(variable);
                        _assignedVariables.Add(variable);
                    }

                    AddAssignmentIfAppropriate(binaryExpression.Right);
                }

                return base.VisitBinary(binaryExpression);
            }

            private bool VariableHasNotYetBeenAccessed(Expression variable)
            {
                return !_accessedVariables.Contains(variable);
            }

            private void AddAssignmentIfAppropriate(Expression assignedValue)
            {
                while (true)
                {
                    switch (assignedValue.NodeType)
                    {
                        case ExpressionType.Block:
                            assignedValue = ((BlockExpression)assignedValue).Result;
                            continue;

                        case ExpressionType.Convert:
                        case ExpressionType.ConvertChecked:
                            assignedValue = ((UnaryExpression)assignedValue).Operand;
                            continue;

                        case ExpressionType.Assign:
                            _assignedAssignments.Add(assignedValue);
                            break;
                    }
                    break;
                }
            }

            protected override Expression VisitGoto(GotoExpression @goto)
            {
                if (@goto.Kind == GotoExpressionKind.Goto)
                {
                    _namedLabelTargets.Add(@goto.Target);
                }

                return base.VisitGoto(@goto);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if (!_chainedMethodCalls.Contains(methodCall))
                {
                    var methodCallChain = GetChainedMethodCalls(methodCall).ToArray();

                    if (methodCallChain.Length > 2)
                    {
                        _chainedMethodCalls.AddRange(methodCallChain);
                    }
                }

                return base.VisitMethodCall(methodCall);
            }

            private static IEnumerable<MethodCallExpression> GetChainedMethodCalls(
                MethodCallExpression methodCall)
            {
                while (methodCall != null)
                {
                    yield return methodCall;

                    methodCall = methodCall.GetSubject() as MethodCallExpression;
                }
            }
        }
    }
}