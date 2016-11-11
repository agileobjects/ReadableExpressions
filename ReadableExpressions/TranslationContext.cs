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
        public IEnumerable<ParameterExpression> JoinedAssignmentVariables => _analyzer.JoinedAssignedVariables;

        /// <summary>
        /// Translates the given <paramref name="expression"/> to readable source code.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to translate.</param>
        /// <returns>A source code translation of the given <paramref name="expression"/>.</returns>
        public string Translate(Expression expression)
        {
            return _globalTranslator.Invoke(expression, this);
        }

        internal string TranslateAsCodeBlock(Expression expression)
        {
            return TranslateCodeBlock(expression).WithCurlyBracesIfMultiStatement();
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
            private readonly Dictionary<BinaryExpression, object> _constructsByAssignment;
            private readonly List<ParameterExpression> _accessedVariables;
            private readonly List<Expression> _assignedAssignments;
            private readonly Stack<object> _constructs;

            private ExpressionAnalysisVisitor()
            {
                _constructsByAssignment = new Dictionary<BinaryExpression, object>();
                _accessedVariables = new List<ParameterExpression>();
                JoinedAssignedVariables = new List<ParameterExpression>();
                JoinedAssignments = new List<BinaryExpression>();
                _assignedAssignments = new List<Expression>();
                NamedLabelTargets = new List<LabelTarget>();
                ChainedMethodCalls = new List<MethodCallExpression>();
                _constructs = new Stack<object>();
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

            public ICollection<ParameterExpression> JoinedAssignedVariables { get; }

            public ICollection<BinaryExpression> JoinedAssignments { get; }

            public ICollection<LabelTarget> NamedLabelTargets { get; }

            public List<MethodCallExpression> ChainedMethodCalls { get; }

            protected override Expression VisitParameter(ParameterExpression variable)
            {
                if (VariableHasNotYetBeenAccessed(variable))
                {
                    _accessedVariables.Add(variable);
                }

                if (!JoinedAssignedVariables.Contains(variable))
                {
                    return base.VisitParameter(variable);
                }

                var joinedAssignmentData = _constructsByAssignment
                    .Where(kvp => kvp.Key.Left == variable)
                    .Select(kvp => new
                    {
                        Assignment = kvp.Key,
                        Construct = kvp.Value
                    })
                    .FirstOrDefault();

                if ((joinedAssignmentData == null) || _constructs.Contains(joinedAssignmentData.Construct))
                {
                    return base.VisitParameter(variable);
                }

                // This variable was assigned within a construct but is being accessed 
                // outside of that scope, so the assignment shouldn't be joined:
                JoinedAssignedVariables.Remove(variable);
                JoinedAssignments.Remove(joinedAssignmentData.Assignment);
                _constructsByAssignment.Remove(joinedAssignmentData.Assignment);

                return base.VisitParameter(variable);
            }

            protected override Expression VisitBinary(BinaryExpression binary)
            {
                if ((binary.NodeType == ExpressionType.Assign) &&
                    (binary.Left.NodeType == ExpressionType.Parameter) &&
                    !JoinedAssignedVariables.Contains(binary.Left) &&
                    !_assignedAssignments.Contains(binary))
                {
                    var variable = (ParameterExpression)binary.Left;

                    if (VariableHasNotYetBeenAccessed(variable))
                    {
                        if (_constructs.Any())
                        {
                            _constructsByAssignment.Add(binary, _constructs.Peek());
                        }

                        JoinedAssignments.Add(binary);
                        _accessedVariables.Add(variable);
                        JoinedAssignedVariables.Add(variable);
                    }

                    AddAssignmentIfAppropriate(binary.Right);
                }

                return base.VisitBinary(binary);
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

            #region Construct

            protected override CatchBlock VisitCatchBlock(CatchBlock @catch)
            {
                return VisitConstruct(@catch, base.VisitCatchBlock);
            }

            protected override Expression VisitConditional(ConditionalExpression conditional)
            {
                return VisitConstruct(conditional, base.VisitConditional);
            }

            protected override Expression VisitTry(TryExpression @try)
            {
                return VisitConstruct(@try, base.VisitTry);
            }

            protected override SwitchCase VisitSwitchCase(SwitchCase @case)
            {
                return VisitConstruct(@case, base.VisitSwitchCase);
            }

            private TResult VisitConstruct<TArg, TResult>(TArg expression, Func<TArg, TResult> baseMethod)
            {
                _constructs.Push(expression);

                var result = baseMethod.Invoke(expression);

                _constructs.Pop();

                return result;
            }

            #endregion
        }
    }
}