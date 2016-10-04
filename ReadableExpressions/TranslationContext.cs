namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Translators;
    using Translators.Formatting;

    public class TranslationContext
    {
        private readonly ExpressionAnalysisVisitor _analyzer;
        private readonly Translator _globalTranslator;

        private TranslationContext(ExpressionAnalysisVisitor analyzer, Translator globalTranslator)
        {
            _analyzer = analyzer;
            _globalTranslator = globalTranslator;
        }

        public static TranslationContext For(Expression expression, Translator globalTranslator)
        {
            return new TranslationContext(ExpressionAnalysisVisitor.Analyse(expression), globalTranslator);
        }

        public IEnumerable<ParameterExpression> JoinedAssignmentVariables => _analyzer.AssignedVariables;

        public string Translate(Expression expression)
        {
            return _globalTranslator.Invoke(expression, this);
        }

        internal CodeBlock TranslateCodeBlock(Expression expression)
        {
            return TranslateBlock(expression as BlockExpression) ?? TranslateSingle(expression);
        }

        private CodeBlock TranslateBlock(BlockExpression block)
        {
            if (block == null)
            {
                return null;
            }

            if (block.Expressions.Count == 1)
            {
                return TranslateSingle(block);
            }

            var blockString = Translate(block);
            var blockLines = blockString.SplitToLines();

            return new CodeBlock(block, blockLines);
        }

        private CodeBlock TranslateSingle(Expression body)
        {
            var bodyString = Translate(body).WithoutSurroundingParentheses(body);

            return new CodeBlock(body, bodyString);
        }

        internal ParameterSet TranslateParameters(
            IEnumerable<Expression> parameters,
            IMethodInfo method = null)
        {
            return new ParameterSet(method, parameters, this);
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

        public bool IsPartOfMethodCallChain(Expression methodCall)
        {
            return _analyzer.ChainedMethodCalls.Contains(methodCall);
        }

        private class ExpressionAnalysisVisitor : ExpressionVisitor
        {
            private readonly List<ParameterExpression> _accessedVariables;
            private readonly List<Expression> _assignedAssignments;
            private BlockExpression _currentBlock;

            private ExpressionAnalysisVisitor()
            {
                _accessedVariables = new List<ParameterExpression>();
                AssignedVariables = new List<ParameterExpression>();
                _assignedAssignments = new List<Expression>();
                JoinedAssignments = new List<BinaryExpression>();
                NamedLabelTargets = new List<LabelTarget>();
                ChainedMethodCalls = new List<MethodCallExpression>();
            }

            #region Factory Method

            public static ExpressionAnalysisVisitor Analyse(Expression expression)
            {
                var analyzer = new ExpressionAnalysisVisitor();

                var coreExpression = GetCoreExpression(expression);

                analyzer.Visit(coreExpression);
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

            public ICollection<ParameterExpression> AssignedVariables { get; }

            public ICollection<BinaryExpression> JoinedAssignments { get; }

            public ICollection<LabelTarget> NamedLabelTargets { get; }

            public List<MethodCallExpression> ChainedMethodCalls { get; }

            protected override Expression VisitBlock(BlockExpression block)
            {
                _currentBlock = block;

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
                    ((_currentBlock == null) || _currentBlock.Expressions.Contains(binaryExpression)) &&
                    !AssignedVariables.Contains(binaryExpression.Left) &&
                    !_assignedAssignments.Contains(binaryExpression))
                {
                    var variable = (ParameterExpression)binaryExpression.Left;

                    if (VariableHasNotYetBeenAccessed(variable))
                    {
                        JoinedAssignments.Add(binaryExpression);
                        _accessedVariables.Add(variable);
                        AssignedVariables.Add(variable);
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
                    NamedLabelTargets.Add(@goto.Target);
                }

                return base.VisitGoto(@goto);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if (!ChainedMethodCalls.Contains(methodCall))
                {
                    var methodCallChain = GetChainedMethodCalls(methodCall).ToArray();

                    if (methodCallChain.Length > 2)
                    {
                        ChainedMethodCalls.AddRange(methodCallChain);
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