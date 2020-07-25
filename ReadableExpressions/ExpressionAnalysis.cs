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
        private Stack<object> _constructs;
        private ICollection<ParameterExpression> _catchBlockVariables;
        private ICollection<LabelTarget> _namedLabelTargets;
        private List<MethodCallExpression> _chainedMethodCalls;
        private ICollection<GotoExpression> _gotoReturnGotos;
        private Dictionary<Type, ParameterExpression[]> _unnamedVariablesByType;
        private Dictionary<MethodExpression, List<ParameterExpression>> _unscopedVariablesByMethod;
        private Dictionary<BlockExpression, MethodExpression> _methodsByConvertedBlock;
        private bool _isSourceCodeAnalysis;
        private bool _isMultilineBlockContext;
        private ClassExpression _currentClass;
        private Queue<ICollection<ParameterExpression>> _scopedVariables;
        private Stack<List<ParameterExpression>> _unscopedVariables;

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
            else if (_settings.CollectRequiredNamespaces)
            {
                _requiredNamespaces = Enumerable<string>.EmptyList;
            }

            _inlineOutputVariables ??= Enumerable<ParameterExpression>.EmptyArray;
            _joinedAssignmentVariables ??= Enumerable<ParameterExpression>.EmptyArray;
            return this;
        }

        #endregion

        public IList<string> RequiredNamespaces => _requiredNamespaces;

        public ICollection<ParameterExpression> InlineOutputVariables => _inlineOutputVariables;

        public ICollection<ParameterExpression> JoinedAssignmentVariables => _joinedAssignmentVariables;

        public bool IsJoinedAssignment(Expression expression)
        {
            return (expression.NodeType == Assign) &&
                   _joinedAssignments?.Contains((BinaryExpression)expression) == true;
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

        public bool IsMethodBlock(BlockExpression block, out MethodExpression blockMethod)
        {
            if (_methodsByConvertedBlock == null)
            {
                blockMethod = null;
                return false;
            }

            return _methodsByConvertedBlock.TryGetValue(block, out blockMethod);
        }

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
                        _isMultilineBlockContext = false;
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
                        _isMultilineBlockContext = false;
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
                        _isMultilineBlockContext = false;
                        expression = ((LabelExpression)expression).DefaultValue;
                        continue;

                    case Lambda:
                        Visit((LambdaExpression)expression);
                        return;

                    case ListInit:
                        Visit((ListInitExpression)expression);
                        return;

                    case Loop:
                        _isMultilineBlockContext = true;
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
                        _isMultilineBlockContext = false;
                        expression = ((TypeBinaryExpression)expression).Expression;
                        continue;

                    default:
                        switch ((SourceCodeExpressionType)expression.NodeType)
                        {
                            case SourceCodeExpressionType.SourceCode:
                                _isSourceCodeAnalysis = true;
                                Visit(((SourceCodeExpression)expression).Classes);
                                return;

                            case SourceCodeExpressionType.Class:
                                Visit((ClassExpression)expression);
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
            _isMultilineBlockContext = false;

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

            if (_isSourceCodeAnalysis && !IsInScope(variable))
            {
                return false;
            }

            return _joinedAssignmentVariables?.Contains(variable) != true;
        }

        private bool IsFirstAccess(Expression variable)
            => _accessedVariables?.Contains(variable) != true;

        private void AddVariableAccess(ParameterExpression variable)
            => (_accessedVariables ??= new List<ParameterExpression>()).Add(variable);

        private void Visit(BlockExpression block)
        {
            VisitVariableOwner(
                block,
                block.Variables,
                b =>
                {
                    var convertToMethod = ShouldConvertToMethod(b);

                    if (convertToMethod)
                    {
                        CollectUnscopedVariables();
                    }

                    (_blocks ??= new Stack<BlockExpression>()).Push(b);

                    VisitMultilineBlockContext(b.Expressions);
                    Visit(b.Variables);

                    _blocks.Pop();

                    if (!convertToMethod)
                    {
                        return;
                    }

                    Expression methodBody = b;

                    var unscopedVariables = _unscopedVariables.Pop();

                    if (unscopedVariables.Any())
                    {
                        methodBody = methodBody.ToLambdaExpression(unscopedVariables);
                    }

                    var convertedBlockMethod = MethodExpression
                        .For(_currentClass, methodBody, _settings, isPublic: false);

                    _currentClass.AddMethod(convertedBlockMethod);

                    (_methodsByConvertedBlock ??= new Dictionary<BlockExpression, MethodExpression>())
                        .Add(b, convertedBlockMethod);
                });
        }

        private void CollectUnscopedVariables()
        {
            (_unscopedVariables ??= new Stack<List<ParameterExpression>>())
                .Push(new List<ParameterExpression>());
        }

        private bool ShouldConvertToMethod(BlockExpression block)
        {
            return
                _settings.CollectInlineBlocks &&
               !_isMultilineBlockContext &&
                (block.Expressions.Count != 1);
        }

        private void Visit(ClassExpression @class)
        {
            _currentClass = @class;
            Visit(@class.Methods);
        }

        private void Visit(ConditionalExpression conditional)
        {
            _isMultilineBlockContext = false;

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
            _isMultilineBlockContext = false;

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
            _isMultilineBlockContext = false;

            Visit(index.Object);
            Visit(index.Arguments);
        }

        private void Visit(InvocationExpression invocation)
        {
            _isMultilineBlockContext = false;

            Visit(invocation.Arguments);
            Visit(invocation.Expression);
        }

        private void Visit(LambdaExpression lambda)
        {
            Visit(lambda.Parameters);
            VisitMultilineBlockContext(lambda.Body);
        }

        private void Visit(ListInitExpression init)
        {
            Visit(init.NewExpression);
            Visit(init.Initializers);
        }

        private void Visit(MemberExpression memberAccess)
        {
            _isMultilineBlockContext = false;

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
            _isMultilineBlockContext = false;

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
            VisitVariableOwner(
                method,
                method.Definition.Parameters,
                m =>
                {
                    AddNamespaceIfRequired(method);
                    CollectUnscopedVariables();

                    Visit(method.Parameters);
                    VisitMultilineBlockContext(method.Body);

                    var unscopedVariables = _unscopedVariables.Pop();

                    if (unscopedVariables.Any())
                    {
                        (_unscopedVariablesByMethod ??= new Dictionary<MethodExpression, List<ParameterExpression>>())
                            .Add(method, unscopedVariables);
                    }
                });
        }

        private void Visit(MethodParameterExpression methodParameter)
            => AddNamespaceIfRequired(methodParameter);

        private void Visit(NewExpression newing)
        {
            _isMultilineBlockContext = false;

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

        private void Visit(NewArrayExpression newArray)
        {
            _isMultilineBlockContext = false;

            Visit(newArray.Expressions);
        }

        private void Visit(ParameterExpression variable)
        {
            if (variable == null)
            {
                return;
            }

            if (IsFirstAccess(variable))
            {
                AddVariableAccess(variable);
            }

            AddVariableAccessInScope(variable);

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

        private void AddVariableAccessInScope(ParameterExpression variable)
        {
            if (!_isSourceCodeAnalysis || IsInScope(variable))
            {
                return;
            }

            foreach (var variableSet in _unscopedVariables)
            {
                if (!variableSet.Contains(variable))
                {
                    variableSet.Add(variable);
                }
            }
        }

        private bool IsInScope(ParameterExpression variable)
        {
            return _scopedVariables?
                .Any(variablesCollection => variablesCollection.Contains(variable)) == true;
        }

        private void Visit(SwitchExpression @switch)
        {
            _isMultilineBlockContext = false;

            Visit(@switch.SwitchValue);

            for (int i = 0, n = @switch.Cases.Count; i < n; ++i)
            {
                Visit(@switch.Cases[i]);
            }

            VisitMultilineBlockContext(@switch.DefaultBody);
        }

        private void Visit(SwitchCase @case)
        {
            VisitConstruct(@case, c =>
            {
                Visit(c.TestValues);
                VisitMultilineBlockContext(c.Body);
            });
        }

        private void Visit(TryExpression @try)
        {
            VisitConstruct(@try, t =>
            {
                VisitMultilineBlockContext(t.Body);

                for (int i = 0, n = t.Handlers.Count; i < n; ++i)
                {
                    Visit(t.Handlers[i]);
                }

                VisitMultilineBlockContext(t.Finally);
                VisitMultilineBlockContext(t.Fault);
            });
        }

        private void Visit(CatchBlock @catch)
        {
            var catchVariable = @catch.Variable;

            ICollection<ParameterExpression> variables;

            if (catchVariable != null)
            {
                AddNamespaceIfRequired(catchVariable);

                (_catchBlockVariables ??= new List<ParameterExpression>())
                    .Add(catchVariable);

                variables = new[] { catchVariable };
            }
            else
            {
                variables = Enumerable<ParameterExpression>.EmptyArray;
            }

            VisitVariableOwner(
                @catch,
                variables,
                c =>
                {
                    VisitConstruct(c, cc =>
                    {
                        Visit(cc.Variable);
                        VisitMultilineBlockContext(cc.Filter);
                        VisitMultilineBlockContext(cc.Body);
                    });
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

        private void VisitVariableOwner<TExpression>(
            TExpression expression,
            ICollection<ParameterExpression> variables,
            Action<TExpression> visitAction)
        {
            if (!_isSourceCodeAnalysis || !variables.Any())
            {
                visitAction.Invoke(expression);
                return;
            }

            AddScopedVariables(variables);

            visitAction.Invoke(expression);

            RemoveScopedVariables();
        }

        private void AddScopedVariables(ICollection<ParameterExpression> inScopeVariables)
        {
            (_scopedVariables ??= new Queue<ICollection<ParameterExpression>>())
                .Enqueue(inScopeVariables);
        }

        private void RemoveScopedVariables() => _scopedVariables.Dequeue();

        private void VisitConstruct<TExpression>(TExpression expression, Action<TExpression> visitAction)
        {
            (_constructs ??= new Stack<object>()).Push(expression);

            visitAction.Invoke(expression);

            _constructs.Pop();
        }

        private void VisitMultilineBlockContext<TExpression>(IList<TExpression> expressions)
            where TExpression : Expression
        {
            _isMultilineBlockContext = true;
            Visit(expressions);
            _isMultilineBlockContext = false;
        }

        private void VisitMultilineBlockContext(Expression expression)
        {
            _isMultilineBlockContext = true;
            Visit(expression);
            _isMultilineBlockContext = false;
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