namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Translators;
    using Translators.Formatting;

    /// <summary>
    /// Contains information about an <see cref="Expression"/> being translated.
    /// </summary>
    public class TranslationContext
    {
        private readonly ExpressionAnalysisVisitor _analyzer;
        private readonly Translator _globalTranslator;

        private TranslationContext(ExpressionAnalysisVisitor analyzer, Translator globalTranslator)
        {
            _analyzer = analyzer;
            _globalTranslator = globalTranslator;
        }

        /// <summary>
        /// Creates a <see cref="TranslationContext"/> containing information about the given
        /// <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="Expression"/> for which to create the <see cref="TranslationContext"/>.
        /// </param>
        /// <param name="globalTranslator">
        /// A global <see cref="Translator"/> delegate with which to perform translations.
        /// </param>
        /// <returns>A <see cref="TranslationContext"/> for the given<paramref name="expression"/>.</returns>
        public static TranslationContext For(Expression expression, Translator globalTranslator)
        {
            return new TranslationContext(ExpressionAnalysisVisitor.Analyse(expression), globalTranslator);
        }

        /// <summary>
        /// Gets the variables in the translated <see cref="Expression"/> which should be declared in the
        /// same statement in which they are assigned.
        /// </summary>
        public IEnumerable<ParameterExpression> JoinedAssignmentVariables => _analyzer.AssignedVariables;

        /// <summary>
        /// Translates the given <paramref name="expression"/> to readable source code.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to translate.</param>
        /// <returns>A source code translation of the given <paramref name="expression"/>.</returns>
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

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="expression"/> represents an assignment 
        /// where the assigned variable is declared as part of the assignment statement.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="expression"/> represents an assignment where the assigned variable 
        /// is declared as part of the assignment statement, otherwise false.
        /// </returns>
        public bool IsNotJoinedAssignment(Expression expression)
        {
            return (expression.NodeType != ExpressionType.Assign) ||
                !_analyzer.JoinedAssignments.Contains(expression);
        }

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="labelTarget"/> is referenced by a
        /// <see cref="GotoExpression"/>.
        /// </summary>
        /// <param name="labelTarget">The <see cref="LabelTarget"/> to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="labelTarget"/> is referenced by a <see cref="GotoExpression"/>,
        /// otherwise false.
        /// </returns>
        public bool IsReferencedByGoto(LabelTarget labelTarget)
        {
            return _analyzer.NamedLabelTargets.Contains(labelTarget);
        }

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="methodCall"/> is part of a chain
        /// of multiple method calls.
        /// </summary>
        /// <param name="methodCall">The <see cref="Expression"/> to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="methodCall"/> is part of a chain of multiple method calls,
        /// otherwise false.
        /// </returns>
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

                    if (methodCallChain.Length > 1)
                    {
                        if (methodCallChain.Length > 2)
                        {
                            ChainedMethodCalls.AddRange(methodCallChain);
                        }
                        else if (methodCallChain[0].ToString().Contains(" ... "))
                        {
                            // Expression.ToString() replaces multiple lines with ' ... ';
                            // potential fragile, but works unless MS change it:
                            ChainedMethodCalls.AddRange(methodCallChain);
                        }
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