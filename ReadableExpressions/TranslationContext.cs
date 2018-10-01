using AgileObjects.ReadableExpressions.Translations;

namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using Expr = System.Linq.Expressions;
    using ExpressionType = System.Linq.Expressions.ExpressionType;
#else
    using System.Collections.ObjectModel;
    using Expr = Microsoft.Scripting.Ast;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
#endif
    using Extensions;
    using Translators;
    using Translators.Formatting;

    /// <summary>
    /// Contains information about an <see cref="Expr.Expression"/> being translated.
    /// </summary>
    public class TranslationContext
    {
        private readonly TranslationAnalyzer _analyzer;
        private readonly Func<Expr.Expression, TranslationContext, string> _globalTranslator;

        private TranslationContext(
            TranslationAnalyzer analyzer,
            Func<Expr.Expression, TranslationContext, string> globalTranslator,
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
        /// The <see cref="Expr.Expression"/> for which to create the <see cref="TranslationContext"/>.
        /// </param>
        /// <param name="globalTranslator">A global translation Func with which to perform translations.</param>
        /// <param name="configuration">The configuration to use for the translation, if required.</param>
        /// <returns>A <see cref="TranslationContext"/> for the given<paramref name="expression"/>.</returns>
        public static TranslationContext For(
            Expr.Expression expression,
            Func<Expr.Expression, TranslationContext, string> globalTranslator,
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
        /// Gets the variables in the translated <see cref="Expr.Expression"/> which should be declared in the
        /// same statement in which they are assigned.
        /// </summary>
        public IEnumerable<Expr.ParameterExpression> JoinedAssignmentVariables => _analyzer.JoinedAssignedVariables;

        /// <summary>
        /// Configuration for translation in this context
        /// </summary>
        public TranslationSettings Settings { get; }

        /// <summary>
        /// Translates the given <paramref name="expression"/> to readable source code.
        /// </summary>
        /// <param name="expression">The <see cref="Expr.Expression"/> to translate.</param>
        /// <returns>A source code translation of the given <paramref name="expression"/>.</returns>
        public string Translate(Expr.Expression expression) => _globalTranslator.Invoke(expression, this);

        internal string TranslateAsCodeBlock(Expr.Expression expression)
        {
            return TranslateCodeBlock(expression).WithCurlyBracesIfMultiStatement();
        }

        internal CodeBlock TranslateCodeBlock(Expr.Expression expression)
        {
            return (expression.NodeType == ExpressionType.Block)
                ? TranslateBlock((Expr.BlockExpression)expression)
                : TranslateSingle(expression);
        }

        private CodeBlock TranslateBlock(Expr.BlockExpression block)
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

        private CodeBlock TranslateSingle(Expr.Expression body)
        {
            var bodyString = Translate(body).WithoutSurroundingParentheses(body);

            return new CodeBlock(body, bodyString);
        }

#if NET35
        internal ParameterSet TranslateParameters<TExpression>(
            ReadOnlyCollection<TExpression> parameters,
            IMethodInfo method = null)
            where TExpression : Expr.Expression
        {
            return new ParameterSet(method, parameters.Cast<Expr.Expression>().ToArray(), this);
        }
#endif
        internal ParameterSet TranslateParameters(
            IEnumerable<Expr.Expression> parameters,
            IMethodInfo method = null)
        {
            return new ParameterSet(method, parameters, this);
        }

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="expression"/> represents an assignment 
        /// where the assigned variable is declared as part of the assignment statement.
        /// </summary>
        /// <param name="expression">The <see cref="Expr.Expression"/> to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="expression"/> represents an assignment where the assigned variable 
        /// is declared as part of the assignment statement, otherwise false.
        /// </returns>
        public bool IsNotJoinedAssignment(Expr.Expression expression)
        {
            return (expression.NodeType != ExpressionType.Assign) ||
                  !_analyzer.JoinedAssignments.Contains(expression);
        }

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="labelTarget"/> is referenced by a
        /// <see cref="Expr.GotoExpression"/>.
        /// </summary>
        /// <param name="labelTarget">The <see cref="Expr.LabelTarget"/> to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="labelTarget"/> is referenced by a <see cref="Expr.GotoExpression"/>,
        /// otherwise false.
        /// </returns>
        public bool IsReferencedByGoto(Expr.LabelTarget labelTarget)
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
        public bool GoesToReturnLabel(Expr.GotoExpression @goto)
            => _analyzer.GotoReturnGotos.Contains(@goto);

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="methodCall"/> is part of a chain
        /// of multiple method calls.
        /// </summary>
        /// <param name="methodCall">The <see cref="Expr.Expression"/> to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="methodCall"/> is part of a chain of multiple method calls,
        /// otherwise false.
        /// </returns>
        public bool IsPartOfMethodCallChain(Expr.Expression methodCall)
            => _analyzer.ChainedMethodCalls.Contains(methodCall);

        /// <summary>
        /// Gets the 1-based index of the given <paramref name="variable"/> in the set of unnamed,
        /// accessed variables of its Type.
        /// </summary>
        /// <param name="variable">The variable for which to get the 1-based index.</param>
        /// <returns>The 1-based index of the given <paramref name="variable"/>.</returns>
        public int? GetUnnamedVariableNumber(Expr.ParameterExpression variable)
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
            private readonly TranslationTree _translationTree;
            private readonly Dictionary<Expr.BinaryExpression, object> _constructsByAssignment;
            private readonly List<Expr.ParameterExpression> _accessedVariables;
            private readonly List<Expr.Expression> _assignedAssignments;
            private readonly Stack<Expr.BlockExpression> _blocks;
            private readonly Stack<object> _constructs;
            private ICollection<Expr.LabelTarget> _namedLabelTargets;
            private ICollection<Expr.GotoExpression> _gotoReturnGotos;
            private Dictionary<Type, Expr.ParameterExpression[]> _unnamedVariablesByType;

            private TranslationAnalyzer()
            {
                _translationTree = new TranslationTree();
                _constructsByAssignment = new Dictionary<Expr.BinaryExpression, object>();
                _accessedVariables = new List<Expr.ParameterExpression>();
                JoinedAssignedVariables = new List<Expr.ParameterExpression>();
                JoinedAssignments = new List<Expr.BinaryExpression>();
                _assignedAssignments = new List<Expr.Expression>();
                ChainedMethodCalls = new List<Expr.MethodCallExpression>();
                _blocks = new Stack<Expr.BlockExpression>();
                _constructs = new Stack<object>();
            }

            #region Factory Method

            public static TranslationAnalyzer Analyse(Expr.Expression expression)
            {
                var analyzer = new TranslationAnalyzer();

                analyzer.Visit(expression);

                return analyzer;
            }

            #endregion

            public ICollection<Expr.ParameterExpression> JoinedAssignedVariables { get; }

            public ICollection<Expr.BinaryExpression> JoinedAssignments { get; }

            public ICollection<Expr.LabelTarget> NamedLabelTargets
                => _namedLabelTargets ?? (_namedLabelTargets = new List<Expr.LabelTarget>());

            public ICollection<Expr.GotoExpression> GotoReturnGotos
                => _gotoReturnGotos ?? (_gotoReturnGotos = new List<Expr.GotoExpression>());

            public List<Expr.MethodCallExpression> ChainedMethodCalls { get; }

            public Dictionary<Type, Expr.ParameterExpression[]> UnnamedVariablesByType
                => _unnamedVariablesByType ??
                   (_unnamedVariablesByType = _accessedVariables
                       .Where(variable => variable.Name.IsNullOrWhiteSpace())
                       .GroupBy(variable => variable.Type)
                       .ToDictionary(grp => grp.Key, grp => grp.ToArray()));

            private void Visit(Expr.Expression expression)
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
                            expression = ((Expr.UnaryExpression)expression).Operand;
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
                            Visit((Expr.BinaryExpression)expression);
                            return;

                        case ExpressionType.Block:
                            Visit((Expr.BlockExpression)expression);
                            return;

                        case ExpressionType.Call:
                            Visit((Expr.MethodCallExpression)expression);
                            return;

                        case ExpressionType.Conditional:
                            Visit((Expr.ConditionalExpression)expression);
                            return;

                        case ExpressionType.Dynamic:
                            Visit(((Expr.DynamicExpression)expression).Arguments);
                            return;

                        case ExpressionType.Goto:
                            Visit((Expr.GotoExpression)expression);
                            return;

                        case ExpressionType.Index:
                            Visit((Expr.IndexExpression)expression);
                            return;

                        case ExpressionType.Invoke:
                            Visit((Expr.InvocationExpression)expression);
                            return;

                        case ExpressionType.Label:
                            expression = ((Expr.LabelExpression)expression).DefaultValue;
                            continue;

                        case ExpressionType.Lambda:
                            expression = ((Expr.LambdaExpression)expression).Body;
                            continue;

                        case ExpressionType.ListInit:
                            Visit((Expr.ListInitExpression)expression);
                            return;

                        case ExpressionType.Loop:
                            expression = ((Expr.LoopExpression)expression).Body;
                            continue;

                        case ExpressionType.MemberAccess:
                            expression = ((Expr.MemberExpression)expression).Expression;
                            continue;

                        case ExpressionType.MemberInit:
                            Visit((Expr.MemberInitExpression)expression);
                            return;

                        case ExpressionType.New:
                            Visit((Expr.NewExpression)expression);
                            return;

                        case ExpressionType.NewArrayInit:
                        case ExpressionType.NewArrayBounds:
                            Visit((Expr.NewArrayExpression)expression);
                            return;

                        case ExpressionType.Parameter:
                            Visit((Expr.ParameterExpression)expression);
                            return;

                        case ExpressionType.RuntimeVariables:
                            Visit(((Expr.RuntimeVariablesExpression)expression).Variables);
                            return;

                        case ExpressionType.Switch:
                            Visit((Expr.SwitchExpression)expression);
                            return;

                        case ExpressionType.Try:
                            Visit((Expr.TryExpression)expression);
                            return;

                        case ExpressionType.TypeEqual:
                        case ExpressionType.TypeIs:
                            expression = ((Expr.TypeBinaryExpression)expression).Expression;
                            continue;

                        default:
                            return;
                    }
                }
            }

            private void Visit(Expr.BinaryExpression binary)
            {
                if ((binary.NodeType == ExpressionType.Assign) &&
                    (binary.Left.NodeType == ExpressionType.Parameter) &&
                    !JoinedAssignedVariables.Contains(binary.Left) &&
                    !_assignedAssignments.Contains(binary))
                {
                    var variable = (Expr.ParameterExpression)binary.Left;

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

            private void Visit(Expr.BlockExpression block)
            {
                _blocks.Push(block);

                Visit(block.Expressions);
                Visit(block.Variables);

                _blocks.Pop();
            }

            private bool VariableHasNotYetBeenAccessed(Expr.Expression variable)
                => !_accessedVariables.Contains(variable);

            private void Visit(Expr.MethodCallExpression methodCall)
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

            private static IEnumerable<Expr.MethodCallExpression> GetChainedMethodCalls(Expr.MethodCallExpression methodCall)
            {
                while (methodCall != null)
                {
                    yield return methodCall;

                    methodCall = methodCall.GetSubject() as Expr.MethodCallExpression;
                }
            }

            private void Visit(Expr.ConditionalExpression conditional)
            {
                VisitConstruct(conditional, c =>
                {
                    Visit(c.Test);
                    Visit(c.IfTrue);
                    Visit(c.IfFalse);
                });
            }

            private void Visit(Expr.GotoExpression @goto)
            {
                if (@goto.Kind != Expr.GotoExpressionKind.Goto)
                {
                    goto VisitValue;
                }

                var currentBlockFinalExpression = _blocks.Peek()?.Expressions.Last();

                if (currentBlockFinalExpression?.NodeType == ExpressionType.Label)
                {
                    var returnLabel = (Expr.LabelExpression)currentBlockFinalExpression;

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

            private void Visit(Expr.IndexExpression index)
            {
                Visit(index.Object);
                Visit(index.Arguments);
            }

            private void Visit(Expr.InvocationExpression invocation)
            {
                Visit(invocation.Arguments);
                Visit(invocation.Expression);
            }

            private void Visit(Expr.LambdaExpression lambda)
            {
                _translationTree.Add(lambda);
                Visit(lambda.Body);
            }

            private void Visit(Expr.ListInitExpression init)
            {
                Visit(init.NewExpression);
                Visit(init.Initializers);
            }

            private void Visit(Expr.NewExpression newing) => Visit(newing.Arguments);

            private void Visit(IList<Expr.ElementInit> elementInits)
            {
                for (int i = 0, n = elementInits.Count; i < n; ++i)
                {
                    Visit(elementInits[i].Arguments);
                }
            }

            private void Visit(Expr.MemberInitExpression memberInit)
            {
                Visit(memberInit.NewExpression);
                Visit(memberInit.Bindings);
            }

            private void Visit(IList<Expr.MemberBinding> original)
            {
                for (int i = 0, n = original.Count; i < n; ++i)
                {
                    Visit(original[i]);
                }
            }

            private void Visit(Expr.MemberBinding binding)
            {
                switch (binding.BindingType)
                {
                    case Expr.MemberBindingType.Assignment:
                        Visit(((Expr.MemberAssignment)binding).Expression);
                        return;

                    case Expr.MemberBindingType.MemberBinding:
                        Visit(((Expr.MemberMemberBinding)binding).Bindings);
                        return;

                    case Expr.MemberBindingType.ListBinding:
                        Visit(((Expr.MemberListBinding)binding).Initializers);
                        return;

                    default:
                        throw new NotSupportedException("Unable to analyze bindings of type " + binding.BindingType);
                }
            }

            private void Visit(Expr.NewArrayExpression na) => Visit(na.Expressions);

            private void Visit(Expr.ParameterExpression variable)
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

            private void Visit(Expr.SwitchExpression @switch)
            {
                Visit(@switch.SwitchValue);

                for (int i = 0, n = @switch.Cases.Count; i < n; ++i)
                {
                    Visit(@switch.Cases[i]);
                }

                Visit(@switch.DefaultBody);
            }

            private void Visit(Expr.SwitchCase @case)
            {
                VisitConstruct(@case, c =>
                {
                    Visit(c.TestValues);
                    Visit(c.Body);
                });
            }

            private void Visit(Expr.TryExpression @try)
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

            private void Visit(Expr.CatchBlock @catch)
            {
                VisitConstruct(@catch, c =>
                {
                    Visit(c.Variable);
                    Visit(c.Filter);
                    Visit(c.Body);
                });
            }

            private void Visit(IList<Expr.ParameterExpression> parameters)
            {
                for (int i = 0, n = parameters.Count; i < n; ++i)
                {
                    Visit(parameters[i]);
                }
            }

            private void Visit(IList<Expr.Expression> expressions)
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

            private void AddAssignmentIfAppropriate(Expr.Expression assignedValue)
            {
                while (true)
                {
                    switch (assignedValue.NodeType)
                    {
                        case ExpressionType.Block:
                            assignedValue = ((Expr.BlockExpression)assignedValue).Result;
                            continue;

                        case ExpressionType.Convert:
                        case ExpressionType.ConvertChecked:
                            assignedValue = ((Expr.UnaryExpression)assignedValue).Operand;
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