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
            var analysis = new ExpressionAnalysis(settings);
            analysis.Analyse(expression);

            return analysis;
        }

        #endregion

        /// <summary>
        /// Analyses the given <paramref name="expression"/>, setting <see cref="ResultExpression"/>
        /// to the Expression returned from <see cref="VisitAndConvert(Expression)"/>, then calls
        /// <see cref="Finalise"/>.
        /// </summary>
        /// <param name="expression">The Expression to analyse.</param>
        protected virtual void Analyse(Expression expression)
        {
            switch (expression.NodeType)
            {
                case DebugInfo:
                case Default:
                case Parameter:
                case RuntimeVariables:
                    ResultExpression = expression;
                    break;

                default:
                    ResultExpression = VisitAndConvert(expression);
                    break;
            }

            Finalise();
        }

        /// <summary>
        /// Gets the Expression which was the final result of the analysis.
        /// </summary>
        public Expression ResultExpression { get; private set; }

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
        /// Visits the given <paramref name="expression"/>, returning a converted Expression if
        /// appropriate.
        /// </summary>
        /// <param name="expression">The Expression to visit.</param>
        /// <returns>
        /// An updated version of the given <paramref name="expression"/>, or the given Expression
        /// if no updates are required.
        /// </returns>
        protected virtual Expression VisitAndConvert(Expression expression)
        {
            while (true)
            {
                if (expression == null)
                {
                    return null;
                }

                if (expression is ICustomAnalysableExpression customExpression)
                {
                    return (Expression)VisitAndConvert(customExpression);
                }

                switch (expression.NodeType)
                {
                    case Constant when expression.IsComment():
                        return VisitAndConvert((CommentExpression)expression);

                    case Constant:
                        return VisitAndConvert((ConstantExpression)expression);

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
                        return VisitAndConvert((UnaryExpression)expression);

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
                        return VisitAndConvert((BinaryExpression)expression);

                    case Block:
                        return VisitAndConvert((BlockExpression)expression);

                    case Call:
                        return VisitAndConvert((MethodCallExpression)expression);

                    case Conditional:
                        return VisitAndConvert((ConditionalExpression)expression);

                    case Dynamic:
                        return VisitAndConvert((DynamicExpression)expression);

                    case Goto:
                        return VisitAndConvert((GotoExpression)expression);

                    case Index:
                        return VisitAndConvert((IndexExpression)expression);

                    case Invoke:
                        return VisitAndConvert((InvocationExpression)expression);

                    case Label:
                        return VisitAndConvert((LabelExpression)expression);

                    case Lambda:
                        return VisitAndConvert((LambdaExpression)expression);

                    case ListInit:
                        return VisitAndConvert((ListInitExpression)expression);

                    case Loop:
                        return VisitAndConvert((LoopExpression)expression);

                    case MemberAccess:
                        return VisitAndConvert((MemberExpression)expression);

                    case MemberInit:
                        return VisitAndConvert((MemberInitExpression)expression);

                    case New:
                        return VisitAndConvert((NewExpression)expression);

                    case NewArrayInit:
                    case NewArrayBounds:
                        return VisitAndConvert((NewArrayExpression)expression);

                    case Parameter:
                        return VisitAndConvert((ParameterExpression)expression);

                    case RuntimeVariables:
                        return VisitAndConvert((RuntimeVariablesExpression)expression);

                    case Switch:
                        return VisitAndConvert((SwitchExpression)expression);

                    case Try:
                        return VisitAndConvert((TryExpression)expression);

                    case TypeEqual:
                    case TypeIs:
                        return VisitAndConvert((TypeBinaryExpression)expression);

                    default:
                        return expression;
                }
            }
        }

        /// <summary>
        /// Visits the given <paramref name="binary"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="binary">The BinaryExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="binary"/>, or the given BinaryExpression
        /// if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(BinaryExpression binary)
        {
            var isConstructAssignment = false;
            var isJoinedAssignment = false;

            if (IsJoinableVariableAssignment(binary, out var variable))
            {
                if (IsFirstAccess(variable))
                {
                    if (_constructs?.Any() == true)
                    {
                        (_constructsByAssignment ??= new Dictionary<BinaryExpression, object>())
                            .Add(binary, _constructs.Peek());

                        isConstructAssignment = true;
                    }

                    (_joinedAssignments ??= new List<BinaryExpression>()).Add(binary);
                    (_joinedAssignmentVariables ??= new List<ParameterExpression>()).Add(variable);

                    isJoinedAssignment = true;

                    AddVariableAccess(variable);
                }

                AddAssignedAssignmentIfAppropriate(binary.Right);
            }

            var updatedBinary = binary.Update(
                VisitAndConvert(binary.Left),
                VisitAndConvert(binary.Conversion),
                VisitAndConvert(binary.Right));

            if (updatedBinary == binary)
            {
                return updatedBinary;
            }

            if (isConstructAssignment)
            {
                _constructsByAssignment.Add(updatedBinary, _constructsByAssignment[binary]);
                _constructsByAssignment.Remove(binary);
            }

            if (isJoinedAssignment)
            {
                _joinedAssignments.Add(updatedBinary);
                _joinedAssignments.Remove(binary);
            }

            return updatedBinary;
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
        /// Visits the given <paramref name="block"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="block">The BlockExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="block"/>, or the given BlockExpression
        /// if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(BlockExpression block)
        {
            (_blocks ??= new Stack<BlockExpression>()).Push(block);

            var expressions = VisitAndConvert(block.Expressions);
            var variables = VisitAndConvert(block.Variables);

            _blocks.Pop();

            return block.Update(variables, expressions);
        }

        /// <summary>
        /// Visits and returns the Expressions contained in the given <paramref name="customExpression"/>.
        /// </summary>
        /// <typeparam name="TExpression">The type of <see cref="ICustomAnalysableExpression"/> to visit.</typeparam>
        /// <param name="customExpression">The <see cref="ICustomAnalysableExpression"/> to visit.</param>
        /// <returns>The given <paramref name="customExpression"/>.</returns>
        protected virtual TExpression VisitAndConvert<TExpression>(TExpression customExpression)
            where TExpression : ICustomAnalysableExpression
        {
            foreach (var expr in customExpression.Expressions)
            {
                VisitAndConvert(expr);
            }

            return customExpression;
        }

        /// <summary>
        /// Visits the given <paramref name="conditional"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="conditional">The ConditionalExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="conditional"/>, or the given
        /// ConditionalExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(ConditionalExpression conditional)
        {
            return VisitConstruct(conditional, cnd => cnd.Update(
                VisitAndConvert(cnd.Test),
                VisitAndConvert(cnd.IfTrue),
                VisitAndConvert(cnd.IfFalse)));
        }

        /// <summary>
        /// Visits the given <paramref name="comment"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="comment">The CommentExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="comment"/>, or the given
        /// CommentExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(CommentExpression comment)
            => comment;

        /// <summary>
        /// Visits the given <paramref name="constant"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="constant">The ConstantExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="constant"/>, or the given
        /// ConstantExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(ConstantExpression constant)
            => constant;

        /// <summary>
        /// Visits the given <paramref name="dynamic"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="dynamic">The DynamicExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="dynamic"/>, or the given
        /// DynamicExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(DynamicExpression dynamic)
            => dynamic.Update(VisitAndConvert(dynamic.Arguments));

        /// <summary>
        /// Visits the given <paramref name="goto"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="goto">The GotoExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="goto"/>, or the given GotoExpression
        /// if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(GotoExpression @goto)
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
            return @goto.Update(@goto.Target, VisitAndConvert(@goto.Value));
        }

        /// <summary>
        /// Visits the given <paramref name="index"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="index">The IndexExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="index"/>, or the given IndexExpression
        /// if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(IndexExpression index)
        {
            return VisitAndConvert(
                index,
                index.Object,
                index.Arguments,
                (idx, exp, args) => Expression
                    .MakeIndex(exp, idx.Indexer, args));
        }

        /// <summary>
        /// Visits the given <paramref name="invocation"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="invocation">The InvocationExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="invocation"/>, or the given
        /// InvocationExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(InvocationExpression invocation)
        {
            return VisitAndConvert(
                invocation,
                invocation.Expression,
                invocation.Arguments,
                (inv, exp, args) => Expression.Invoke(exp, args));
        }

        /// <summary>
        /// Visits the given <paramref name="label"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="label">The LabelExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="label"/>, or the given
        /// LabelExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(LabelExpression label)
            => label.Update(label.Target, VisitAndConvert(label.DefaultValue));

        private LambdaExpression VisitAndConvert(LambdaExpression lambda)
        {
            if (lambda == null)
            {
                return null;
            }

            var parameters = VisitAndConvert(lambda.Parameters);
            var body = VisitAndConvert(lambda.Body);

            if (parameters == lambda.Parameters && body == lambda.Body)
            {
                return lambda;
            }

            return lambda.Update(body, parameters);
        }

        /// <summary>
        /// Visits the given <paramref name="listInit"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="listInit">The ListInitExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="listInit"/>, or the given
        /// ListInitExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(ListInitExpression listInit)
        {
            var updatedNewing = VisitAndConvert(listInit.NewExpression);

            if (updatedNewing.NodeType != New)
            {
                return updatedNewing;
            }

            return listInit.Update(
                (NewExpression)updatedNewing,
                VisitAndConvert(listInit.Initializers));
        }

        /// <summary>
        /// Visits the given <paramref name="loop"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="loop">The LoopExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="loop"/>, or the given
        /// LoopExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(LoopExpression loop)
            => loop.Update(loop.BreakLabel, loop.ContinueLabel, VisitAndConvert(loop.Body));

        /// <summary>
        /// Visits the given <paramref name="memberAccess"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="memberAccess">The MemberExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="memberAccess"/>, or the given
        /// CommentExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(MemberExpression memberAccess)
            => memberAccess.Update(VisitAndConvert(memberAccess.Expression));

        /// <summary>
        /// Visits the given <paramref name="methodCall"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="methodCall">The MethodCallExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="methodCall"/>, or the given
        /// MethodCallExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(MethodCallExpression methodCall)
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

            return VisitAndConvert(
                methodCall,
                methodCall.Object,
                methodCall.Arguments,
                (mc, s, args) => Expression.Call(s, mc.Method, args));
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
        /// Visits the given <paramref name="newing"/>, returning a replacement NewExpression if
        /// appropriate.
        /// </summary>
        /// <param name="newing">The NewExpression to visit.</param>
        /// <returns>
        /// A NewExpression to replace the given <paramref name="newing"/>, or the given
        /// NewExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(NewExpression newing)
            => newing.Update(VisitAndConvert(newing.Arguments));

        private IEnumerable<ElementInit> VisitAndConvert(IList<ElementInit> elementInits)
        {
            var initCount = elementInits.Count;
            var updatedInits = new ElementInit[initCount];
            var initsUpdated = false;

            for (var i = 0; i < initCount; ++i)
            {
                var init = elementInits[i];
                var arguments = init.Arguments;
                var updatedArguments = VisitAndConvert(arguments);

                if (updatedArguments != arguments)
                {
                    initsUpdated = true;
                    init = Expression.ElementInit(init.AddMethod, updatedArguments);
                }

                updatedInits[i] = init;
            }

            return initsUpdated ? updatedInits : elementInits;
        }

        /// <summary>
        /// Visits the given <paramref name="memberInit"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="memberInit">The MemberInitExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="memberInit"/>, or the given
        /// MemberInitExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(MemberInitExpression memberInit)
        {
            var updatedNewing = VisitAndConvert(memberInit.NewExpression);

            if (updatedNewing.NodeType != New)
            {
                return updatedNewing;
            }

            return memberInit.Update(
                (NewExpression)updatedNewing,
                VisitAndConvert(memberInit.Bindings));
        }

        private IEnumerable<MemberBinding> VisitAndConvert(IList<MemberBinding> bindings)
        {
            var bindingCount = bindings.Count;
            var updatedBindings = new MemberBinding[bindingCount];
            var bindingsUpdated = false;

            for (var i = 0; i < bindingCount; ++i)
            {
                var binding = bindings[i];
                var updatedBinding = VisitAndConvert(binding);
                updatedBindings[i] = updatedBinding;

                if (updatedBinding != binding)
                {
                    bindingsUpdated = true;
                }
            }

            return bindingsUpdated ? updatedBindings : bindings;
        }

        private MemberBinding VisitAndConvert(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitAndConvert((MemberAssignment)binding);

                case MemberBindingType.MemberBinding:
                    return VisitAndConvert((MemberMemberBinding)binding);

                case MemberBindingType.ListBinding:
                    return VisitAndConvert((MemberListBinding)binding);

                default:
                    throw new NotSupportedException("Unable to analyze bindings of type " + binding.BindingType);
            }
        }

        private MemberBinding VisitAndConvert(MemberAssignment assignment)
            => assignment.Update(VisitAndConvert(assignment.Expression));

        private MemberBinding VisitAndConvert(MemberMemberBinding binding)
            => binding.Update(VisitAndConvert(binding.Bindings));

        private MemberBinding VisitAndConvert(MemberListBinding listBinding)
            => listBinding.Update(VisitAndConvert(listBinding.Initializers));

        /// <summary>
        /// Visits the given <paramref name="newArray"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="newArray">The NewArrayExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="newArray"/>, or the given
        /// NewArrayExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(NewArrayExpression newArray)
            => newArray.Update(VisitAndConvert(newArray.Expressions));

        /// <summary>
        /// Visits the given <paramref name="variable"/>, returning a replacement ParameterExpression
        /// if appropriate.
        /// </summary>
        /// <param name="variable">The ParameterExpression to visit.</param>
        /// <returns>
        /// A ParameterExpression to replace the given <paramref name="variable"/>, or the given
        /// ParameterExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(ParameterExpression variable)
            => Visit(variable);

        private ParameterExpression Visit(ParameterExpression variable)
        {
            if (variable == null)
            {
                return null;
            }

            if (IsFirstAccess(variable))
            {
                AddVariableAccess(variable);
            }

            if (_joinedAssignmentVariables?.Contains(variable) != true)
            {
                return variable;
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
                return variable;
            }

            // This variable was assigned within a construct but is being accessed 
            // outside of that scope, so the assignment shouldn't be joined:
            _joinedAssignmentVariables.Remove(variable);
            _joinedAssignments.Remove(joinedAssignmentData.Assignment);
            _constructsByAssignment.Remove(joinedAssignmentData.Assignment);

            return variable;
        }

        /// <summary>
        /// Visits the given <paramref name="runtimeVariables"/>, returning a replacement Expression
        /// if appropriate.
        /// </summary>
        /// <param name="runtimeVariables">The RuntimeVariablesExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="runtimeVariables"/>, or the given
        /// RuntimeVariablesExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(RuntimeVariablesExpression runtimeVariables)
            => runtimeVariables.Update(VisitAndConvert(runtimeVariables.Variables));

        /// <summary>
        /// Visits the given <paramref name="switch"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="switch">The SwitchExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="switch"/>, or the given
        /// SwitchExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(SwitchExpression @switch)
        {
            var updatedValue = VisitAndConvert(@switch.SwitchValue);

            var caseCount = @switch.Cases.Count;
            var updatedCases = new SwitchCase[caseCount];

            for (var i = 0; i < caseCount; ++i)
            {
                updatedCases[i] = VisitAndConvert(@switch.Cases[i]);
            }

            var updatedDefault = VisitAndConvert(@switch.DefaultBody);

            return @switch.Update(updatedValue, updatedCases, updatedDefault);
        }

        private SwitchCase VisitAndConvert(SwitchCase @case)
        {
            return VisitConstruct(@case, c => c.Update(
                VisitAndConvert(c.TestValues),
                VisitAndConvert(c.Body)));
        }

        /// <summary>
        /// Visits the given <paramref name="try"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="try">The TryExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="try"/>, or the given TryExpression
        /// if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(TryExpression @try)
        {
            return VisitConstruct(@try, t =>
            {
                var updatedBody = VisitAndConvert(t.Body);

                var handlerCount = t.Handlers.Count;
                var updatedHandlers = new CatchBlock[handlerCount];

                for (var i = 0; i < handlerCount; ++i)
                {
                    updatedHandlers[i] = VisitAndConvert(t.Handlers[i]);
                }

                var updatedFinally = VisitAndConvert(t.Finally);
                var updatedFault = VisitAndConvert(t.Fault);

                return t.Update(
                    updatedBody,
                    updatedHandlers,
                    updatedFinally,
                    updatedFault);
            });
        }

        /// <summary>
        /// Visits the given <paramref name="catch"/>, returning a replacement CatchBlock if
        /// appropriate.
        /// </summary>
        /// <param name="catch">The CatchBlock to visit.</param>
        /// <returns>
        /// A CatchBlock to replace the given <paramref name="catch"/>, or the given CatchBlock if
        /// no replacement is required.
        /// </returns>
        protected virtual CatchBlock VisitAndConvert(CatchBlock @catch)
        {
            var catchVariable = @catch.Variable;

            if (catchVariable != null)
            {
                (_catchBlockVariables ??= new List<ParameterExpression>())
                    .Add(catchVariable);
            }

            return VisitConstruct(@catch, c => c.Update(
                Visit(c.Variable),
                VisitAndConvert(c.Filter),
                VisitAndConvert(c.Body)));
        }

        /// <summary>
        /// Visits the given <paramref name="typing"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="typing">The TypeBinaryExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="typing"/>, or the given
        /// TypeBinaryExpression if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(TypeBinaryExpression typing)
            => typing.Update(VisitAndConvert(typing.Expression));

        /// <summary>
        /// Visits the given <paramref name="unary"/>, returning a replacement Expression if
        /// appropriate.
        /// </summary>
        /// <param name="unary">The UnaryExpression to visit.</param>
        /// <returns>
        /// An Expression to replace the given <paramref name="unary"/>, or the given UnaryExpression
        /// if no replacement is required.
        /// </returns>
        protected virtual Expression VisitAndConvert(UnaryExpression unary)
            => unary.Update(VisitAndConvert(unary.Operand));

        /// <summary>
        /// Visits the given <paramref name="parameters"/>, returning a replacement ParameterExpression
        /// collection if appropriate.
        /// </summary>
        /// <param name="parameters">The ParameterExpressions to visit.</param>
        /// <returns>
        /// A ParameterExpression collection to replace the given <paramref name="parameters"/>, or
        /// the given collection if no replacement is required.
        /// </returns>
        protected IList<ParameterExpression> VisitAndConvert(IList<ParameterExpression> parameters)
            => VisitAndConvert(parameters, Visit);

        /// <summary>
        /// Visits the given <paramref name="expressions"/>, returning a replacement Expression
        /// collection if appropriate.
        /// </summary>
        /// <param name="expressions">The Expressions to visit.</param>
        /// <returns>
        /// An Expression collection to replace the given <paramref name="expressions"/>, or the
        /// given collection if no replacement is required.
        /// </returns>
        protected IList<Expression> VisitAndConvert(IList<Expression> expressions)
            => VisitAndConvert(expressions, VisitAndConvert);

        /// <summary>
        /// Visits the given <paramref name="expressions"/>, returning a replacement
        /// <typeparamref name="TExpression"/> collection if appropriate.
        /// </summary>
        /// <typeparam name="TExpression">The type of Expression contained in the collection.</typeparam>
        /// <param name="expressions">The <typeparamref name="TExpression"/>s to visit.</param>
        /// <param name="visitor">A Func with which to process each <typeparamref name="TExpression"/>.</param>
        /// <returns>
        /// A <typeparamref name="TExpression"/> collection to replace the given
        /// <paramref name="expressions"/>, or the given collection if no replacement is required.
        /// </returns>
        protected static IList<TExpression> VisitAndConvert<TExpression>(
            IList<TExpression> expressions,
            Func<TExpression, TExpression> visitor)
            where TExpression : Expression
        {
            var expressionCount = expressions.Count;

            switch (expressionCount)
            {
                case 0:
                    return expressions;

                case 1:
                    {
                        var expression = expressions[0];
                        var updatedExpression = visitor.Invoke(expression);

                        return expression != updatedExpression
                            ? new[] { updatedExpression } : expressions;
                    }

                default:
                    var updatedExpressions = new TExpression[expressionCount];
                    var expressionsUpdated = false;

                    for (var i = 0; ;)
                    {
                        var expression = expressions[i];
                        var updatedExpression = visitor.Invoke(expression);
                        updatedExpressions[i] = updatedExpression;

                        if (expression != updatedExpression)
                        {
                            expressionsUpdated = true;
                        }

                        ++i;

                        if (i == expressionCount)
                        {
                            return expressionsUpdated
                                ? updatedExpressions : expressions;
                        }
                    }
            }
        }

        /// <summary>
        /// Visits the given <paramref name="subject"/> and <paramref name="expressions"/>, and uses
        /// the <paramref name="expressionFactory"/> to create a replacement
        /// <typeparamref name="TExpression"/> if appropriate.
        /// </summary>
        /// <typeparam name="TExpression">The type of Expression to visit.</typeparam>
        /// <param name="expression">The <typeparamref name="TExpression"/> to visit.</param>
        /// <param name="subject">The <paramref name="expression"/>'s subject.</param>
        /// <param name="expressions">The <paramref name="expression"/>'s expressions.</param>
        /// <param name="expressionFactory">
        /// A Func with which to create a new <typeparamref name="TExpression"/> instance.
        /// </param>
        /// <returns>
        /// An Expression to replace the given <paramref name="expression"/>, or the given
        /// <paramref name="expression"/> if no replacement is required.
        /// </returns>
        protected Expression VisitAndConvert<TExpression>(
            TExpression expression,
            Expression subject,
            IList<Expression> expressions,
            Func<TExpression, Expression, IList<Expression>, Expression> expressionFactory)
            where TExpression : Expression
        {
            var updatedSubject = VisitAndConvert(subject);
            var updatedArguments = VisitAndConvert(expressions);

            if (updatedSubject == subject && updatedArguments == expressions)
            {
                return expression;
            }

            return expressionFactory.Invoke(expression, subject, updatedArguments);
        }

        private TConstruct VisitConstruct<TConstruct>(
            TConstruct expression,
            Func<TConstruct, TConstruct> visitor)
        {
            (_constructs ??= new Stack<object>()).Push(expression);

            var updatedConstruct = visitor.Invoke(expression);

            _constructs.Pop();

            return updatedConstruct;
        }

        private void AddAssignedAssignmentIfAppropriate(Expression assignedValue)
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

        /// <summary>
        /// Merges the results of the given <paramref name="otherAnalysis"/> with this
        /// <see cref="ExpressionAnalysis"/>.
        /// </summary>
        /// <param name="otherAnalysis">The <see cref="ExpressionAnalysis"/> to merge into this one.</param>
        protected void Merge(ExpressionAnalysis otherAnalysis)
        {
            if (otherAnalysis._inlineOutputVariables != null)
            {
                Merge(
                    otherAnalysis._inlineOutputVariables,
                   _inlineOutputVariables ??= new List<ParameterExpression>());
            }

            if (otherAnalysis._joinedAssignmentVariables != null)
            {
                Merge(
                    otherAnalysis._joinedAssignmentVariables,
                   _joinedAssignmentVariables ??= new List<ParameterExpression>());
            }

            if (otherAnalysis._joinedAssignments != null)
            {
                Merge(
                    otherAnalysis._joinedAssignments,
                   _joinedAssignments ??= new List<BinaryExpression>());
            }
        }

        private void Merge<TExpression>(
            IEnumerable<TExpression> sourceExpressions,
            ICollection<TExpression> targetExpressions)
        {
            _inlineOutputVariables ??= new List<ParameterExpression>();

            foreach (var expression in sourceExpressions)
            {
                targetExpressions.Add(expression);
            }
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
    }
}