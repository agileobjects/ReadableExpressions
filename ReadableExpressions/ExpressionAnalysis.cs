namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    /// <summary>
    /// Contains information about an analysed Expression.
    /// </summary>
    public class ExpressionAnalysis
    {
        private readonly TranslationSettings _settings;
        private Dictionary<BinaryExpression, object> _constructsByAssignment;
        private ICollection<ParameterExpression> _accessedVariables;
        private IList<ParameterExpression> _inlineOutputVariables;
        private IList<ParameterExpression> _joinedAssignmentVariables;
        private ICollection<BinaryExpression> _joinedAssignments;
        private ICollection<Expression> _assignedAssignments;
        private Stack<BlockExpression> _blocks;
        private Stack<object> _constructs;
        private ICollection<ParameterExpression> _catchBlockVariables;
        private ICollection<LabelTarget> _namedLabelTargets;
        private List<MethodCallExpression> _chainedMethodCalls;
        private ICollection<GotoExpression> _gotoReturnGotos;
        private Dictionary<Type, ParameterExpression[]> _unnamedVariablesByType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionAnalysis"/> class.
        /// </summary>
        /// <param name="settings">
        /// The <see cref="TranslationSettings"/> being used in the current Expression translation.
        /// </param>
        protected ExpressionAnalysis(TranslationSettings settings)
        {
            _settings = settings;
        }

        #region Factory Method

        /// <summary>
        /// Create an <see cref="ExpressionAnalysis"/> for the given <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The Expression to analyse.</param>
        /// <param name="settings">
        /// The <see cref="TranslationSettings"/> for the current Expression translation.
        /// </param>
        /// <returns>The <see cref="ExpressionAnalysis"/>.</returns>
        public static ExpressionAnalysis For(Expression expression, TranslationSettings settings)
        {
            switch (expression.NodeType)
            {
                case DebugInfo:
                case Default:
                case Extension:
                case Parameter:
                case RuntimeVariables:
                    return new ExpressionAnalysis(settings).Finalise();
            }

            var analysis = new ExpressionAnalysis(settings);

            analysis.Visit(expression);
            analysis.Finalise();

            return analysis;
        }

        /// <summary>
        /// Finalises this <see cref="ExpressionAnalysis"/>, ensuring any required members are
        /// initialised.
        /// </summary>
        /// <returns>This finalised <see cref="ExpressionAnalysis"/>.</returns>
        protected virtual ExpressionAnalysis Finalise()
        {
            _inlineOutputVariables ??= Enumerable<ParameterExpression>.EmptyArray;
            _joinedAssignmentVariables ??= Enumerable<ParameterExpression>.EmptyArray;
            return this;
        }

        #endregion

        /// <summary>
        /// Gets the variables in the translated Expression which are first used as an output
        /// parameter argument.
        /// </summary>
        public ICollection<ParameterExpression> InlineOutputVariables => _inlineOutputVariables;

        /// <summary>
        /// Gets the variables which can be declared as part of their initial assignment statement.
        /// </summary>
        public ICollection<ParameterExpression> JoinedAssignmentVariables => _joinedAssignmentVariables;

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="expression"/> represents an
        /// assignment where the assigned variable is declared as part of the assignment statement.
        /// </summary>
        /// <param name="expression">The Expression to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="expression"/> represents an assignment where the assigned
        /// variable is declared as part of the assignment statement, otherwise false.
        /// </returns>
        public bool IsJoinedAssignment(Expression expression)
        {
            return (expression.NodeType == Assign) &&
                   _joinedAssignments?.Contains((BinaryExpression)expression) == true;
        }

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="variable"/> is the Exception
        /// variable in a Catch block.
        /// </summary>
        /// <param name="variable">The Expression for which to make the determination.</param>
        /// <returns>
        /// True if the given <paramref name="variable"/> is the Exception variable in a Catch block,
        /// otherwise false.
        /// </returns>
        public bool IsCatchBlockVariable(Expression variable)
        {
            return (variable.NodeType == Parameter) &&
                  (_catchBlockVariables?.Contains((ParameterExpression)variable) == true);
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
            => _namedLabelTargets?.Contains(labelTarget) == true;

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
            => _gotoReturnGotos?.Contains(@goto) == true;

        /// <summary>
        /// Returns a value indicating whether the given <paramref name="methodCall"/> is part of a chain
        /// of multiple method calls.
        /// </summary>
        /// <param name="methodCall">The Expression to evaluate.</param>
        /// <returns>
        /// True if the given <paramref name="methodCall"/> is part of a chain of multiple method calls,
        /// otherwise false.
        /// </returns>
        public bool IsPartOfMethodCallChain(MethodCallExpression methodCall)
            => _chainedMethodCalls?.Contains(methodCall) == true;

        /// <summary>
        /// Gets a Dictionary of ParameterExpression variables, keyed by their Type.
        /// </summary>
        public Dictionary<Type, ParameterExpression[]> UnnamedVariablesByType
            => _unnamedVariablesByType ??= _accessedVariables?
                .Where(variable => InternalStringExtensions.IsNullOrWhiteSpace(variable.Name))
                .GroupBy(variable => variable.Type)
                .ToDictionary(grp => grp.Key, grp => grp.ToArray()) ??
                 EmptyDictionary<Type, ParameterExpression[]>.Instance;

        /// <summary>
        /// Visits the given <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The Expression to visit.</param>
        protected virtual void Visit(Expression expression)
        {
            while (true)
            {
                if (expression == null)
                {
                    return;
                }

                switch (expression.NodeType)
                {
                    case Constant:
                        Visit((ConstantExpression)expression);
                        return;

                    case ArrayLength:
                    case ExpressionType.Convert:
                    case ConvertChecked:
                    case Decrement:
                    case Increment:
                    case IsFalse:
                    case IsTrue:
                    case Negate:
                    case NegateChecked:
                    case Not:
                    case OnesComplement:
                    case PostDecrementAssign:
                    case PostIncrementAssign:
                    case PreDecrementAssign:
                    case PreIncrementAssign:
                    case Quote:
                    case Throw:
                    case TypeAs:
                    case UnaryPlus:
                    case Unbox:
                        expression = ((UnaryExpression)expression).Operand;
                        continue;

                    case Add:
                    case AddAssign:
                    case AddAssignChecked:
                    case AddChecked:
                    case And:
                    case AndAlso:
                    case AndAssign:
                    case ArrayIndex:
                    case Assign:
                    case Coalesce:
                    case Divide:
                    case DivideAssign:
                    case Equal:
                    case ExclusiveOr:
                    case ExclusiveOrAssign:
                    case GreaterThan:
                    case GreaterThanOrEqual:
                    case LeftShift:
                    case LeftShiftAssign:
                    case LessThan:
                    case LessThanOrEqual:
                    case ModuloAssign:
                    case Multiply:
                    case MultiplyAssign:
                    case MultiplyAssignChecked:
                    case MultiplyChecked:
                    case Modulo:
                    case NotEqual:
                    case Or:
                    case OrAssign:
                    case OrElse:
                    case Power:
                    case PowerAssign:
                    case RightShift:
                    case RightShiftAssign:
                    case Subtract:
                    case SubtractAssign:
                    case SubtractAssignChecked:
                    case SubtractChecked:
                        Visit((BinaryExpression)expression);
                        return;

                    case Block:
                        Visit((BlockExpression)expression);
                        return;

                    case Call:
                        Visit((MethodCallExpression)expression);
                        return;

                    case Conditional:
                        Visit((ConditionalExpression)expression);
                        return;

                    case Dynamic:
                        Visit(((DynamicExpression)expression).Arguments);
                        return;

                    case Goto:
                        Visit((GotoExpression)expression);
                        return;

                    case Index:
                        Visit((IndexExpression)expression);
                        return;

                    case Invoke:
                        Visit((InvocationExpression)expression);
                        return;

                    case Label:
                        expression = ((LabelExpression)expression).DefaultValue;
                        continue;

                    case Lambda:
                        Visit((LambdaExpression)expression);
                        return;

                    case ListInit:
                        Visit((ListInitExpression)expression);
                        return;

                    case Loop:
                        expression = ((LoopExpression)expression).Body;
                        continue;

                    case MemberAccess:
                        expression = Visit((MemberExpression)expression);
                        continue;

                    case MemberInit:
                        Visit((MemberInitExpression)expression);
                        return;

                    case New:
                        Visit((NewExpression)expression);
                        return;

                    case NewArrayInit:
                    case NewArrayBounds:
                        Visit((NewArrayExpression)expression);
                        return;

                    case Parameter:
                        Visit((ParameterExpression)expression);
                        return;

                    case RuntimeVariables:
                        Visit(((RuntimeVariablesExpression)expression).Variables);
                        return;

                    case Switch:
                        Visit((SwitchExpression)expression);
                        return;

                    case Try:
                        Visit((TryExpression)expression);
                        return;

                    case TypeEqual:
                    case TypeIs:
                        expression = ((TypeBinaryExpression)expression).Expression;
                        continue;

                    default:
                        return;
                }
            }
        }

        private void Visit(BinaryExpression binary)
        {
            if (IsJoinableVariableAssignment(binary, out var variable))
            {
                if (IsFirstAccess(variable))
                {
                    if (_constructs?.Any() == true)
                    {
                        (_constructsByAssignment ??= new Dictionary<BinaryExpression, object>())
                            .Add(binary, _constructs.Peek());
                    }

                    (_joinedAssignments ??= new List<BinaryExpression>()).Add(binary);
                    (_joinedAssignmentVariables ??= new List<ParameterExpression>()).Add(variable);

                    AddVariableAccess(variable);
                }

                AddAssignmentIfAppropriate(binary.Right);
            }

            Visit(binary.Left);

            if (binary.Conversion != null)
            {
                Visit(binary.Conversion.Body);
            }

            Visit(binary.Right);
        }

        private bool IsJoinableVariableAssignment(
            BinaryExpression binary,
            out ParameterExpression variable)
        {
            if (binary.NodeType != Assign ||
                binary.Left.NodeType != Parameter ||
               _assignedAssignments?.Contains(binary) == true)
            {
                variable = null;
                return false;
            }

            variable = (ParameterExpression)binary.Left;

            return IsAssignmentJoinable(variable);
        }

        /// <summary>
        /// Gets a value indicating whether the assignment of the given <paramref name="variable"/>
        /// can be joined with its declaration.
        /// </summary>
        /// <param name="variable">The variable for which to make the determination.</param>
        /// <returns>
        /// True if the assignment of the given <paramref name="variable"/> can be joined with its
        /// declaration, otherwise false.
        /// </returns>
        protected virtual bool IsAssignmentJoinable(ParameterExpression variable)
            => _joinedAssignmentVariables?.Contains(variable) != true;

        private bool IsFirstAccess(Expression variable)
            => _accessedVariables?.Contains(variable) != true;

        private void AddVariableAccess(ParameterExpression variable)
            => (_accessedVariables ??= new List<ParameterExpression>()).Add(variable);

        /// <summary>
        /// Visits the given <paramref name="block"/>.
        /// </summary>
        /// <param name="block">The BlockExpression to visit.</param>
        protected virtual void Visit(BlockExpression block)
        {
            (_blocks ??= new Stack<BlockExpression>()).Push(block);

            Visit(block.Expressions);
            Visit(block.Variables);

            _blocks.Pop();
        }

        private void Visit(ConditionalExpression conditional)
        {
            VisitConstruct(conditional, cnd =>
            {
                Visit(cnd.Test);
                Visit(cnd.IfTrue);
                Visit(cnd.IfFalse);
            });
        }

        /// <summary>
        /// Visits the given <paramref name="constant"/>.
        /// </summary>
        /// <param name="constant">The ConstantExpression to visit.</param>
        protected virtual void Visit(ConstantExpression constant)
        {
        }

        private void Visit(GotoExpression @goto)
        {
            if (@goto.Kind != GotoExpressionKind.Goto)
            {
                goto VisitValue;
            }

            var currentBlockFinalExpression = _blocks?.Peek()?.Expressions.Last();

            if (currentBlockFinalExpression?.NodeType == Label)
            {
                var returnLabel = (LabelExpression)currentBlockFinalExpression;

                if (@goto.Target == returnLabel.Target)
                {
                    (_gotoReturnGotos ??= new List<GotoExpression>()).Add(@goto);
                    goto VisitValue;
                }
            }

            (_namedLabelTargets ??= new List<LabelTarget>()).Add(@goto.Target);

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
            Visit(lambda.Parameters);
            Visit(lambda.Body);
        }

        private void Visit(ListInitExpression init)
        {
            Visit(init.NewExpression);
            Visit(init.Initializers);
        }

        /// <summary>
        /// Visits the given <paramref name="memberAccess"/>.
        /// </summary>
        /// <param name="memberAccess">The MemberExpression to visit.</param>
        /// <returns>The Expression on which to continue analysis.</returns>
        protected virtual Expression Visit(MemberExpression memberAccess)
            => memberAccess.Expression;

        /// <summary>
        /// Visits the given <paramref name="methodCall"/>.
        /// </summary>
        /// <param name="methodCall">The MethodCallExpression to visit.</param>
        protected virtual void Visit(MethodCallExpression methodCall)
        {
            if (_chainedMethodCalls?.Contains(methodCall) != true)
            {
                var methodCallChain = GetChainedMethodCalls(methodCall).ToArray();

                if (methodCallChain.Length > 1)
                {
                    _chainedMethodCalls ??= new List<MethodCallExpression>();

                    if (methodCallChain.Length > 2)
                    {
                        _chainedMethodCalls.AddRange(methodCallChain);
                    }
                    else if (methodCallChain[0].ToString().Contains(" ... "))
                    {
                        // Expression.ToString() replaces multiple lines with ' ... ';
                        // potentially fragile, but works unless MS change it:
                        _chainedMethodCalls.AddRange(methodCallChain);
                    }
                }
            }

            if (_settings.DeclareOutParamsInline)
            {
                for (int i = 0, l = methodCall.Arguments.Count; i < l; ++i)
                {
                    var argument = methodCall.Arguments[i];

                    if ((argument.NodeType == Parameter) &&
                        IsFirstAccess(argument))
                    {
                        (_inlineOutputVariables ??= new List<ParameterExpression>())
                            .Add((ParameterExpression)argument);
                    }
                }
            }

            Visit(methodCall.Object);
            Visit(methodCall.Arguments);
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

        /// <summary>
        /// Visits the given <paramref name="newing"/>.
        /// </summary>
        /// <param name="newing">The NewExpression to visit.</param>
        protected virtual void Visit(NewExpression newing) => Visit(newing.Arguments);

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

        /// <summary>
        /// Visits the given <paramref name="newArray"/>.
        /// </summary>
        /// <param name="newArray">The NewArrayExpression to visit.</param>
        protected virtual void Visit(NewArrayExpression newArray)
            => Visit(newArray.Expressions);

        /// <summary>
        /// Visits the given <paramref name="variable"/>.
        /// </summary>
        /// <param name="variable">The ParameterExpression to visit.</param>
        protected virtual void Visit(ParameterExpression variable)
        {
            if (variable == null)
            {
                return;
            }

            if (IsFirstAccess(variable))
            {
                AddVariableAccess(variable);
            }

            if (_joinedAssignmentVariables?.Contains(variable) != true)
            {
                return;
            }

            var joinedAssignmentData = _constructsByAssignment?
                .Filter(kvp => kvp.Key.Left == variable)
                .Project(kvp => new
                {
                    Assignment = kvp.Key,
                    Construct = kvp.Value
                })
                .FirstOrDefault();

            if ((joinedAssignmentData == null) ||
               (_constructs?.Contains(joinedAssignmentData.Construct) == true))
            {
                return;
            }

            // This variable was assigned within a construct but is being accessed 
            // outside of that scope, so the assignment shouldn't be joined:
            _joinedAssignmentVariables.Remove(variable);
            _joinedAssignments.Remove(joinedAssignmentData.Assignment);
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

        /// <summary>
        /// Visits the given <paramref name="catch"/>.
        /// </summary>
        /// <param name="catch">The CatchBlock to visit.</param>
        protected virtual void Visit(CatchBlock @catch)
        {
            var catchVariable = @catch.Variable;

            if (catchVariable != null)
            {
                (_catchBlockVariables ??= new List<ParameterExpression>())
                    .Add(catchVariable);
            }

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

        /// <summary>
        /// Visits the given <paramref name="expressions"/>.
        /// </summary>
        /// <param name="expressions">The Expressions to visit.</param>
        protected void Visit<TExpression>(IList<TExpression> expressions)
            where TExpression : Expression
        {
            var expressionCount = expressions.Count;

            switch (expressionCount)
            {
                case 0:
                    return;

                case 1:
                    Visit(expressions[0]);
                    return;

                default:
                    for (var i = 0; ;)
                    {
                        Visit(expressions[i]);

                        ++i;

                        if (i == expressionCount)
                        {
                            return;
                        }
                    }
            }
        }

        private void VisitConstruct<TExpression>(TExpression expression, Action<TExpression> visitAction)
        {
            (_constructs ??= new Stack<object>()).Push(expression);

            visitAction.Invoke(expression);

            _constructs.Pop();
        }

        private void AddAssignmentIfAppropriate(Expression assignedValue)
        {
            while (true)
            {
                switch (assignedValue.NodeType)
                {
                    case Block:
                        assignedValue = ((BlockExpression)assignedValue).Result;
                        continue;

                    case ExpressionType.Convert:
                    case ConvertChecked:
                        assignedValue = ((UnaryExpression)assignedValue).Operand;
                        continue;

                    case Assign:
                        (_assignedAssignments ??= new List<Expression>()).Add(assignedValue);
                        break;
                }
                break;
            }
        }
    }
}