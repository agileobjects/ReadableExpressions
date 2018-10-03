namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using System.Collections.ObjectModel;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Translators;
    using Translators.Formatting;

    /// <summary>
    /// Contains information about an <see cref="Expression"/> being translated.
    /// </summary>
    public class TranslationContext
    {
        private readonly TranslationAnalyzer _analyzer;
        private readonly Func<Expression, TranslationContext, string> _globalTranslator;

        private TranslationContext(
            TranslationAnalyzer analyzer,
            Func<Expression, TranslationContext, string> globalTranslator,
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
        /// <param name="globalTranslator">A global translation Func with which to perform translations.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A <see cref="TranslationContext"/> for the given<paramref name="expression"/>.</returns>
        public static TranslationContext For(
            Expression expression,
            Func<Expression, TranslationContext, string> globalTranslator,
            Func<TranslationSettings, TranslationSettings> configuration = null)
        {
            var analyzer = TranslationAnalyzer.Analyse(expression);
            var settings = GetTranslationSettings(configuration);

            return new TranslationContext(analyzer, globalTranslator, settings);
        }

        internal static TranslationSettings GetTranslationSettings(
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
            return (expression.NodeType == ExpressionType.Block)
                ? TranslateBlock((BlockExpression)expression)
                : TranslateSingle(expression);
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

        internal class TranslationAnalyzer
        {
            private readonly Dictionary<BinaryExpression, object> _constructsByAssignment;
            private readonly List<ParameterExpression> _accessedVariables;
            private readonly List<Expression> _assignedAssignments;
            private readonly Stack<BlockExpression> _blocks;
            private readonly Stack<object> _constructs;
            private ICollection<LabelTarget> _namedLabelTargets;
            private ICollection<GotoExpression> _gotoReturnGotos;
            private Dictionary<Type, ParameterExpression[]> _unnamedVariablesByType;

            private TranslationAnalyzer()
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

            public static TranslationAnalyzer Analyse(Expression expression)
            {
                var analyzer = new TranslationAnalyzer();

                analyzer.Visit(expression);

                return analyzer;
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

            private void Visit(Expression expression)
            {
                while (true)
                {
                    if (expression == null)
                    {
                        return;
                    }

                    switch (expression.NodeType)
                    {
                        case ExpressionType.Constant:
                        case ExpressionType.DebugInfo:
                        case ExpressionType.Default:
                        case ExpressionType.Extension:
                            return;

                        case ExpressionType.ArrayLength:
                        case ExpressionType.Convert:
                        case ExpressionType.ConvertChecked:
                        case ExpressionType.Decrement:
                        case ExpressionType.Increment:
                        case ExpressionType.IsFalse:
                        case ExpressionType.IsTrue:
                        case ExpressionType.Negate:
                        case ExpressionType.NegateChecked:
                        case ExpressionType.Not:
                        case ExpressionType.OnesComplement:
                        case ExpressionType.PostDecrementAssign:
                        case ExpressionType.PostIncrementAssign:
                        case ExpressionType.PreDecrementAssign:
                        case ExpressionType.PreIncrementAssign:
                        case ExpressionType.Quote:
                        case ExpressionType.Throw:
                        case ExpressionType.TypeAs:
                        case ExpressionType.UnaryPlus:
                        case ExpressionType.Unbox:
                            expression = ((UnaryExpression)expression).Operand;
                            continue;

                        case ExpressionType.Add:
                        case ExpressionType.AddAssign:
                        case ExpressionType.AddAssignChecked:
                        case ExpressionType.AddChecked:
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                        case ExpressionType.AndAssign:
                        case ExpressionType.ArrayIndex:
                        case ExpressionType.Assign:
                        case ExpressionType.Coalesce:
                        case ExpressionType.Divide:
                        case ExpressionType.DivideAssign:
                        case ExpressionType.Equal:
                        case ExpressionType.ExclusiveOr:
                        case ExpressionType.ExclusiveOrAssign:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.LeftShift:
                        case ExpressionType.LeftShiftAssign:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.ModuloAssign:
                        case ExpressionType.Multiply:
                        case ExpressionType.MultiplyAssign:
                        case ExpressionType.MultiplyAssignChecked:
                        case ExpressionType.MultiplyChecked:
                        case ExpressionType.Modulo:
                        case ExpressionType.NotEqual:
                        case ExpressionType.Or:
                        case ExpressionType.OrAssign:
                        case ExpressionType.OrElse:
                        case ExpressionType.Power:
                        case ExpressionType.PowerAssign:
                        case ExpressionType.RightShift:
                        case ExpressionType.RightShiftAssign:
                        case ExpressionType.Subtract:
                        case ExpressionType.SubtractAssign:
                        case ExpressionType.SubtractAssignChecked:
                        case ExpressionType.SubtractChecked:
                            Visit((BinaryExpression)expression);
                            return;

                        case ExpressionType.Block:
                            Visit((BlockExpression)expression);
                            return;

                        case ExpressionType.Call:
                            Visit((MethodCallExpression)expression);
                            return;

                        case ExpressionType.Conditional:
                            Visit((ConditionalExpression)expression);
                            return;

                        case ExpressionType.Dynamic:
                            Visit(((DynamicExpression)expression).Arguments);
                            return;

                        case ExpressionType.Goto:
                            Visit((GotoExpression)expression);
                            return;

                        case ExpressionType.Index:
                            Visit((IndexExpression)expression);
                            return;

                        case ExpressionType.Invoke:
                            Visit((InvocationExpression)expression);
                            return;

                        case ExpressionType.Label:
                            expression = ((LabelExpression)expression).DefaultValue;
                            continue;

                        case ExpressionType.Lambda:
                            expression = ((LambdaExpression)expression).Body;
                            continue;

                        case ExpressionType.ListInit:
                            Visit((ListInitExpression)expression);
                            return;

                        case ExpressionType.Loop:
                            expression = ((LoopExpression)expression).Body;
                            continue;

                        case ExpressionType.MemberAccess:
                            expression = ((MemberExpression)expression).Expression;
                            continue;

                        case ExpressionType.MemberInit:
                            Visit((MemberInitExpression)expression);
                            return;

                        case ExpressionType.New:
                            Visit((NewExpression)expression);
                            return;

                        case ExpressionType.NewArrayInit:
                        case ExpressionType.NewArrayBounds:
                            Visit((NewArrayExpression)expression);
                            return;

                        case ExpressionType.Parameter:
                            Visit((ParameterExpression)expression);
                            return;

                        case ExpressionType.RuntimeVariables:
                            Visit(((RuntimeVariablesExpression)expression).Variables);
                            return;

                        case ExpressionType.Switch:
                            Visit((SwitchExpression)expression);
                            return;

                        case ExpressionType.Try:
                            Visit((TryExpression)expression);
                            return;

                        case ExpressionType.TypeEqual:
                        case ExpressionType.TypeIs:
                            expression = ((TypeBinaryExpression)expression).Expression;
                            continue;

                        default:
                            return;
                    }
                }
            }

            private void Visit(BinaryExpression binary)
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

                Visit(binary.Left);
                Visit(binary.Conversion);
                Visit(binary.Right);
            }

            private void Visit(BlockExpression block)
            {
                _blocks.Push(block);

                Visit(block.Expressions);
                Visit(block.Variables);

                _blocks.Pop();
            }

            private bool VariableHasNotYetBeenAccessed(Expression variable)
                => !_accessedVariables.Contains(variable);

            private void Visit(MethodCallExpression methodCall)
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

                Visit(methodCall.Object);
                Visit(methodCall.Arguments);
            }

            private static IEnumerable<MethodCallExpression> GetChainedMethodCalls(MethodCallExpression methodCall)
            {
                while (methodCall != null)
                {
                    yield return methodCall;

                    methodCall = methodCall.GetSubject() as MethodCallExpression;
                }
            }

            private void Visit(ConditionalExpression conditional)
            {
                VisitConstruct(conditional, c =>
                {
                    Visit(c.Test);
                    Visit(c.IfTrue);
                    Visit(c.IfFalse);
                });
            }

            private void Visit(GotoExpression @goto)
            {
                if (@goto.Kind != GotoExpressionKind.Goto)
                {
                    goto VisitValue;
                }

                var currentBlockFinalExpression = _blocks.Peek()?.Expressions.Last();

                if (currentBlockFinalExpression?.NodeType == ExpressionType.Label)
                {
                    var returnLabel = (LabelExpression)currentBlockFinalExpression;

                    if (@goto.Target == returnLabel.Target)
                    {
                        GotoReturnGotos.Add(@goto);
                        goto VisitValue;
                    }
                }

                NamedLabelTargets.Add(@goto.Target);

                VisitValue:
                Visit(@goto.Value);
            }

            private void Visit(IndexExpression index)
            {
                Visit(index.Object);
                Visit(index.Arguments);
            }

            private void Visit(InvocationExpression invocation)
            {
                Visit(invocation.Arguments);
                Visit(invocation.Expression);
            }

            private void Visit(LambdaExpression lambda)
            {
                if (lambda != null)
                {
                    Visit(lambda.Body);
                }
            }

            private void Visit(ListInitExpression init)
            {
                Visit(init.NewExpression);
                Visit(init.Initializers);
            }

            private void Visit(NewExpression newing) => Visit(newing.Arguments);

            private void Visit(IList<ElementInit> elementInits)
            {
                for (int i = 0, n = elementInits.Count; i < n; ++i)
                {
                    Visit(elementInits[i].Arguments);
                }
            }

            private void Visit(MemberInitExpression memberInit)
            {
                Visit(memberInit.NewExpression);
                Visit(memberInit.Bindings);
            }

            private void Visit(IList<MemberBinding> original)
            {
                for (int i = 0, n = original.Count; i < n; ++i)
                {
                    Visit(original[i]);
                }
            }

            private void Visit(MemberBinding binding)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        Visit(((MemberAssignment)binding).Expression);
                        return;

                    case MemberBindingType.MemberBinding:
                        Visit(((MemberMemberBinding)binding).Bindings);
                        return;

                    case MemberBindingType.ListBinding:
                        Visit(((MemberListBinding)binding).Initializers);
                        return;

                    default:
                        throw new NotSupportedException("Unable to analyze bindings of type " + binding.BindingType);
                }
            }

            private void Visit(NewArrayExpression na) => Visit(na.Expressions);

            private void Visit(ParameterExpression variable)
            {
                if (VariableHasNotYetBeenAccessed(variable))
                {
                    _accessedVariables.Add(variable);
                }

                if (!JoinedAssignedVariables.Contains(variable))
                {
                    return;
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
                    return;
                }

                // This variable was assigned within a construct but is being accessed 
                // outside of that scope, so the assignment shouldn't be joined:
                JoinedAssignedVariables.Remove(variable);
                JoinedAssignments.Remove(joinedAssignmentData.Assignment);
                _constructsByAssignment.Remove(joinedAssignmentData.Assignment);
            }

            private void Visit(SwitchExpression @switch)
            {
                Visit(@switch.SwitchValue);

                for (int i = 0, n = @switch.Cases.Count; i < n; ++i)
                {
                    Visit(@switch.Cases[i]);
                }

                Visit(@switch.DefaultBody);
            }

            private void Visit(SwitchCase @case)
            {
                VisitConstruct(@case, c =>
                {
                    Visit(c.TestValues);
                    Visit(c.Body);
                });
            }

            private void Visit(TryExpression @try)
            {
                VisitConstruct(@try, t =>
                {
                    Visit(t.Body);

                    for (int i = 0, n = t.Handlers.Count; i < n; ++i)
                    {
                        Visit(t.Handlers[i]);
                    }

                    Visit(t.Finally);
                    Visit(t.Fault);
                });
            }

            private void Visit(CatchBlock @catch)
            {
                VisitConstruct(@catch, c =>
                {
                    Visit(c.Variable);
                    Visit(c.Filter);
                    Visit(c.Body);
                });
            }

            private void Visit(IList<ParameterExpression> parameters)
            {
                for (int i = 0, n = parameters.Count; i < n; ++i)
                {
                    Visit(parameters[i]);
                }
            }

            private void Visit(IList<Expression> expressions)
            {
                for (int i = 0, n = expressions.Count; i < n; ++i)
                {
                    Visit(expressions[i]);
                }
            }

            private void VisitConstruct<TExpression>(TExpression expression, Action<TExpression> baseMethod)
            {
                _constructs.Push(expression);

                baseMethod.Invoke(expression);

                _constructs.Pop();
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
        }

        #endregion
    }
}