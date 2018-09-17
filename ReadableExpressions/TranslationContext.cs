namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using BinaryExpression = Microsoft.Scripting.Ast.BinaryExpression;
    using BlockExpression = Microsoft.Scripting.Ast.BlockExpression;
    using CatchBlock = Microsoft.Scripting.Ast.CatchBlock;
    using ConditionalExpression = Microsoft.Scripting.Ast.ConditionalExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using ExpressionVisitor = Microsoft.Scripting.Ast.ExpressionVisitor;
    using GotoExpression = Microsoft.Scripting.Ast.GotoExpression;
    using GotoExpressionKind = Microsoft.Scripting.Ast.GotoExpressionKind;
    using LabelExpression = Microsoft.Scripting.Ast.LabelExpression;
    using LabelTarget = Microsoft.Scripting.Ast.LabelTarget;
    using LambdaExpression = Microsoft.Scripting.Ast.LambdaExpression;
    using MethodCallExpression = Microsoft.Scripting.Ast.MethodCallExpression;
    using ParameterExpression = Microsoft.Scripting.Ast.ParameterExpression;
    using SwitchCase = Microsoft.Scripting.Ast.SwitchCase;
    using TryExpression = Microsoft.Scripting.Ast.TryExpression;
    using UnaryExpression = Microsoft.Scripting.Ast.UnaryExpression;
#endif
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

        private TranslationContext(
            ExpressionAnalysisVisitor analyzer,
            Translator globalTranslator,
            TranslationSettings settings)
        {
            _analyzer = analyzer;
            _globalTranslator = globalTranslator;
            Settings = settings;
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
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A <see cref="TranslationContext"/> for the given<paramref name="expression"/>.</returns>
        public static TranslationContext For(
            Expression expression,
            Translator globalTranslator,
            Func<TranslationSettings, TranslationSettings> configuration = null)
        {
            var analyzer = ExpressionAnalysisVisitor.Analyse(expression);
            var settings = GetTranslationSettings(configuration);

            return new TranslationContext(analyzer, globalTranslator, settings);
        }

        private static TranslationSettings GetTranslationSettings(
            Func<TranslationSettings, TranslationSettings> configuration)
        {
            return configuration?.Invoke(new TranslationSettings()) ?? TranslationSettings.Default;
        }

        /// <summary>
        /// Gets the variables in the translated <see cref="Expression"/> which should be declared in the
        /// same statement in which they are assigned.
        /// </summary>
        public IEnumerable<ParameterExpression> JoinedAssignmentVariables => _analyzer.JoinedAssignedVariables;

        /// <summary>
        /// Configuration for translation in this context
        /// </summary>
        public TranslationSettings Settings { get; }

        /// <summary>
        /// Translates the given <paramref name="expression"/> to readable source code.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to translate.</param>
        /// <returns>A source code translation of the given <paramref name="expression"/>.</returns>
        public string Translate(Expression expression) => _globalTranslator.Invoke(expression, this);

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

#if NET35
        internal ParameterSet TranslateParameters<TExpression>(
            ReadOnlyCollection<TExpression> parameters,
            IMethodInfo method = null)
            where TExpression : Expression
        {
            return new ParameterSet(method, parameters.Cast<Expression>().ToArray(), this);
        }
#endif

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
            => _analyzer.NamedLabelTargets.Contains(labelTarget);

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="goto"/> goes to the 
        /// final statement in a block, and so should be rendered as a return statement.
        /// </summary>
        /// <param name="goto">The GotoExpression for which to make the determination.</param>
        /// <returns>
        /// True if the given <paramref name="goto"/> goes to the final statement in a block,
        /// otherwise false.
        /// </returns>
        public bool GoesToReturnLabel(GotoExpression @goto)
            => _analyzer.GotoReturnGotos.Contains(@goto);

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
            => _analyzer.ChainedMethodCalls.Contains(methodCall);

        /// <summary>
        /// Gets the 1-based index of the given <paramref name="variable"/> in the set of unnamed,
        /// accessed variables of its Type.
        /// </summary>
        /// <param name="variable">The variable for which to get the 1-based index.</param>
        /// <returns>The 1-based index of the given <paramref name="variable"/>.</returns>
        public int? GetUnnamedVariableNumber(ParameterExpression variable)
        {
            var variablesOfType = _analyzer.UnnamedVariablesByType[variable.Type];

            if (variablesOfType.Length == 1)
            {
                return null;
            }

            return Array.IndexOf(variablesOfType, variable, 0) + 1;
        }

        #region Helper Class

        private class ExpressionAnalysisVisitor : ExpressionVisitor
        {
            private readonly Dictionary<BinaryExpression, object> _constructsByAssignment;
            private readonly List<ParameterExpression> _accessedVariables;
            private readonly List<Expression> _assignedAssignments;
            private readonly Stack<BlockExpression> _blocks;
            private readonly Stack<object> _constructs;
            private ICollection<LabelTarget> _namedLabelTargets;
            private ICollection<GotoExpression> _gotoReturnGotos;
            private Dictionary<Type, ParameterExpression[]> _unnamedVariablesByType;

            private ExpressionAnalysisVisitor()
            {
                _constructsByAssignment = new Dictionary<BinaryExpression, object>();
                _accessedVariables = new List<ParameterExpression>();
                JoinedAssignedVariables = new List<ParameterExpression>();
                JoinedAssignments = new List<BinaryExpression>();
                _assignedAssignments = new List<Expression>();
                ChainedMethodCalls = new List<MethodCallExpression>();
                _blocks = new Stack<BlockExpression>();
                _constructs = new Stack<object>();
            }

            #region Factory Method

            public static ExpressionAnalysisVisitor Analyse(Expression expression)
            {
                var analyzer = new ExpressionAnalysisVisitor();

                var coreExpression = GetCoreExpression(expression);

                if ((expression.NodeType != ExpressionType.Extension) || expression.CanReduce)
                {
                    analyzer.Visit(coreExpression);
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

            public ICollection<ParameterExpression> JoinedAssignedVariables { get; }

            public ICollection<BinaryExpression> JoinedAssignments { get; }

            public ICollection<LabelTarget> NamedLabelTargets
                => _namedLabelTargets ?? (_namedLabelTargets = new List<LabelTarget>());

            public ICollection<GotoExpression> GotoReturnGotos
                => _gotoReturnGotos ?? (_gotoReturnGotos = new List<GotoExpression>());

            public List<MethodCallExpression> ChainedMethodCalls { get; }

            public Dictionary<Type, ParameterExpression[]> UnnamedVariablesByType
                => _unnamedVariablesByType ??
                  (_unnamedVariablesByType = _accessedVariables
                      .Where(variable => variable.Name.IsNullOrWhiteSpace())
                      .GroupBy(variable => variable.Type)
                      .ToDictionary(grp => grp.Key, grp => grp.ToArray()));

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
                    .Filter(kvp => kvp.Key.Left == variable)
                    .Project(kvp => new
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

            protected override Expression VisitBlock(BlockExpression block)
            {
                _blocks.Push(block);

                var result = base.VisitBlock(block);

                _blocks.Pop();

                return result;
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
                => !_accessedVariables.Contains(variable);

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
                if (@goto.Kind != GotoExpressionKind.Goto)
                {
                    return base.VisitGoto(@goto);
                }

                var currentBlockFinalExpression = _blocks.Peek()?.Expressions.Last();

                if (currentBlockFinalExpression?.NodeType == ExpressionType.Label)
                {
                    var returnLabel = (LabelExpression)currentBlockFinalExpression;

                    if (@goto.Target == returnLabel.Target)
                    {
                        GotoReturnGotos.Add(@goto);

                        return base.VisitGoto(@goto);
                    }
                }

                NamedLabelTargets.Add(@goto.Target);

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

        #endregion
    }
}