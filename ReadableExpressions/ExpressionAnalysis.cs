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
    using NetStandardPolyfills;
    using SourceCode;
    using Translations;
    using Translations.Reflection;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class ExpressionAnalysis
    {
        private readonly TranslationSettings _settings;
        private Dictionary<BinaryExpression, object> _constructsByAssignment;
        private List<string> _requiredNamespaces;
        private ICollection<ParameterExpression> _accessedVariables;
        private IList<ParameterExpression> _inlineOutputVariables;
        private IList<ParameterExpression> _joinedAssignmentVariables;
        private ICollection<BinaryExpression> _joinedAssignments;
        private ICollection<Expression> _assignedAssignments;
        private Stack<BlockExpression> _blocks;
        private List<ParameterExpression> _blockVariables;
        private Stack<object> _constructs;
        private ICollection<ParameterExpression> _catchBlockVariables;
        private ICollection<LabelTarget> _namedLabelTargets;
        private List<MethodCallExpression> _chainedMethodCalls;
        private ICollection<GotoExpression> _gotoReturnGotos;
        private Dictionary<Type, ParameterExpression[]> _unnamedVariablesByType;
        private Dictionary<MethodExpression, List<ParameterExpression>> _unscopedVariablesByMethod;

        private ExpressionAnalysis(TranslationSettings settings)
        {
            _settings = settings;
        }

        #region Factory Method

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

        private ExpressionAnalysis Finalise()
        {
            if (_requiredNamespaces != null)
            {
                _requiredNamespaces.Sort(UsingsComparer.Instance);
            }
            else
            {
                _requiredNamespaces = Enumerable<string>.EmptyList;
            }

            _inlineOutputVariables ??= Enumerable<ParameterExpression>.EmptyArray;
            _joinedAssignmentVariables ??= Enumerable<ParameterExpression>.EmptyArray;
            return this;
        }

        #endregion

        public IList<string> RequiredNamespaces => _requiredNamespaces;

        public ICollection<BlockExpression> InlineBlocks { get; private set; }

        public ICollection<ParameterExpression> InlineOutputVariables => _inlineOutputVariables;

        public ICollection<ParameterExpression> JoinedAssignmentVariables => _joinedAssignmentVariables;

        public bool IsNotJoinedAssignment(Expression expression)
        {
            return (expression.NodeType != Assign) ||
                   _joinedAssignments?.Contains((BinaryExpression)expression) != true;
        }

        public bool IsCatchBlockVariable(Expression variable)
        {
            return (variable.NodeType == Parameter) &&
                  (_catchBlockVariables?.Contains((ParameterExpression)variable) == true);
        }

        public bool IsReferencedByGoto(LabelTarget labelTarget)
            => _namedLabelTargets?.Contains(labelTarget) == true;

        public bool GoesToReturnLabel(GotoExpression @goto)
            => _gotoReturnGotos?.Contains(@goto) == true;

        public bool IsPartOfMethodCallChain(MethodCallExpression methodCall)
            => _chainedMethodCalls?.Contains(methodCall) == true;

        public Dictionary<Type, ParameterExpression[]> UnnamedVariablesByType
            => _unnamedVariablesByType ??= _accessedVariables?
                .Where(variable => InternalStringExtensions.IsNullOrWhiteSpace(variable.Name))
                .GroupBy(variable => variable.Type)
                .ToDictionary(grp => grp.Key, grp => grp.ToArray()) ??
                 EmptyDictionary<Type, ParameterExpression[]>.Instance;

        public Dictionary<MethodExpression, List<ParameterExpression>> UnscopedVariablesByMethod
            => _unscopedVariablesByMethod ??= EmptyDictionary<MethodExpression, List<ParameterExpression>>.Instance;

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
                    case Constant:
                        Visit((ConstantExpression)expression);
                        return;

                    case DebugInfo:
                    case Extension:
                        return;

                    case Default:
                        Visit((DefaultExpression)expression);
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
                        Visit((MemberExpression)expression);
                        return;

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
                        switch ((SourceCodeExpressionType)expression.NodeType)
                        {
                            case SourceCodeExpressionType.SourceCode:
                                Visit(((SourceCodeExpression)expression).Classes);
                                return;

                            case SourceCodeExpressionType.Class:
                                Visit(((ClassExpression)expression).Methods);
                                return;

                            case SourceCodeExpressionType.Method:
                                Visit((MethodExpression)expression);
                                return;

                            case SourceCodeExpressionType.MethodParameter:
                                Visit((MethodParameterExpression)expression);
                                return;
                        }

                        return;
                }
            }
        }

        private void Visit(BinaryExpression binary)
        {
            if ((binary.NodeType == Assign) &&
                (binary.Left.NodeType == Parameter) &&
               (_joinedAssignmentVariables?.Contains(binary.Left) != true) &&
               (_assignedAssignments?.Contains(binary) != true))
            {
                var variable = (ParameterExpression)binary.Left;

                if (VariableHasNotYetBeenAccessed(variable))
                {
                    if (_constructs?.Any() == true)
                    {
                        (_constructsByAssignment ??= new Dictionary<BinaryExpression, object>())
                            .Add(binary, _constructs.Peek());
                    }

                    (_joinedAssignments ??= new List<BinaryExpression>()).Add(binary);
                    (_accessedVariables ??= new List<ParameterExpression>()).Add(variable);
                    (_joinedAssignmentVariables ??= new List<ParameterExpression>()).Add(variable);
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

        private bool VariableHasNotYetBeenAccessed(Expression variable)
            => _accessedVariables?.Contains(variable) != true;

        private void Visit(BlockExpression block)
        {
            AddInlineBlockIfRequired(block);

            if (_blocks == null)
            {
                _blocks = new Stack<BlockExpression>();
                _blockVariables = new List<ParameterExpression>();
            }

            _blockVariables.AddRange(block.Variables);
            _blocks.Push(block);

            Visit(block.Expressions);
            Visit(block.Variables);

            _blocks.Pop();
        }

        private void AddInlineBlockIfRequired(BlockExpression block)
        {
            if (ShouldAddInlineBlock(block))
            {
                (InlineBlocks ??= new List<BlockExpression>()).Add(block);
            }
        }

        private bool ShouldAddInlineBlock(BlockExpression block)
        {
            if (!_settings.CollectInlineBlocks ||
                (_constructs == null) ||
                (block.Expressions.Count == 1))
            {
                return false;
            }

            var containingConstruct = _constructs.Peek();

            switch (containingConstruct)
            {
                case Expression expression:
                    switch (expression.NodeType)
                    {
                        case Conditional:
                            var conditional = (ConditionalExpression)expression;

                            if (conditional.Test == block)
                            {
                                return true;
                            }

                            return
                                 conditional.IsTernary() &&
                                (conditional.IfTrue == block ||
                                 conditional.IfFalse == block);
                    }

                    break;
            }

            return false;
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

        private void Visit(ConstantExpression constant)
        {
            if (constant.Type.IsEnum())
            {
                AddNamespaceIfRequired(constant);
                return;
            }

            if (constant.Type.IsAssignableTo(typeof(Type)))
            {
                AddNamespaceIfRequired((Type)constant.Value);
            }
        }

        private void Visit(DefaultExpression @default)
            => AddNamespaceIfRequired(@default);

        private void AddNamespacesIfRequired(IEnumerable<Type> accessedTypes)
        {
            foreach (var type in accessedTypes)
            {
                AddNamespaceIfRequired(type);
            }
        }

        private void AddNamespaceIfRequired(Expression expression)
            => AddNamespaceIfRequired(expression.Type);

        private void AddNamespaceIfRequired(Type accessedType)
        {
            if (!_settings.CollectRequiredNamespaces ||
               (accessedType == typeof(void)) ||
               (accessedType == typeof(string)) ||
               (accessedType == typeof(object)) ||
                accessedType.IsPrimitive())
            {
                return;
            }

            if (accessedType.IsGenericType())
            {
                AddNamespacesIfRequired(accessedType.GetGenericTypeArguments());
            }

            var @namespace = accessedType.Namespace;

            if (@namespace == null)
            {
                return;
            }

            _requiredNamespaces ??= new List<string>();

            if (!_requiredNamespaces.Contains(@namespace))
            {
                _requiredNamespaces.Add(@namespace);
            }
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

        private void Visit(MemberExpression memberAccess)
        {
            if (memberAccess.Expression != null)
            {
                Visit(memberAccess.Expression);
                return;
            }

            // Static member access
            AddNamespaceIfRequired(memberAccess.Member.DeclaringType);
        }

        private void Visit(MethodCallExpression methodCall)
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
                        VariableHasNotYetBeenAccessed(argument))
                    {
                        (_inlineOutputVariables ??= new List<ParameterExpression>())
                            .Add((ParameterExpression)argument);
                    }
                }
            }

            if (methodCall.Method.IsGenericMethod)
            {
                AddNamespacesIfRequired(new BclMethodWrapper(methodCall.Method)
                    .GetRequiredExplicitGenericArguments(_settings));
            }

            if (methodCall.Method.IsStatic)
            {
                AddNamespaceIfRequired(methodCall.Method.DeclaringType);
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

        private void Visit(MethodExpression method)
        {
            AddNamespaceIfRequired(method);

            Visit(method.Parameters);
            Visit(method.Body);

            if (_accessedVariables == null)
            {
                return;
            }

            var unscopedVariables = _accessedVariables
                .Except(method.Parameters.ProjectToArray(p => p.ParameterExpression));

            if (_blockVariables?.Any() == true)
            {
                unscopedVariables = unscopedVariables.Except(_blockVariables);
            }

            if (_catchBlockVariables?.Any() == true)
            {
                unscopedVariables = unscopedVariables.Except(_catchBlockVariables);
            }

            (_unscopedVariablesByMethod ??= new Dictionary<MethodExpression, List<ParameterExpression>>())
                .Add(method, unscopedVariables.ToList());
        }

        private void Visit(MethodParameterExpression methodParameter)
            => AddNamespaceIfRequired(methodParameter);

        private void Visit(NewExpression newing)
        {
            AddNamespaceIfRequired(newing.Type);
            Visit(newing.Arguments);
        }

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
            if (variable == null)
            {
                return;
            }

            if (VariableHasNotYetBeenAccessed(variable))
            {
                (_accessedVariables ??= new List<ParameterExpression>()).Add(variable);
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

        private void Visit(CatchBlock @catch)
        {
            var catchVariable = @catch.Variable;

            if (catchVariable != null)
            {
                (_catchBlockVariables ??= new List<ParameterExpression>()).Add(catchVariable);

                AddNamespaceIfRequired(catchVariable);
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

        private void Visit<TExpression>(IList<TExpression> expressions)
            where TExpression : Expression
        {
            for (int i = 0, n = expressions.Count; i < n; ++i)
            {
                Visit(expressions[i]);
            }
        }

        private void VisitConstruct<TExpression>(TExpression expression, Action<TExpression> baseMethod)
        {
            (_constructs ??= new Stack<object>()).Push(expression);

            baseMethod.Invoke(expression);

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