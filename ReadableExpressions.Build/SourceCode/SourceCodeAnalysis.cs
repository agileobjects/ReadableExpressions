namespace AgileObjects.ReadableExpressions.Build.SourceCode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Translations.Reflection;
    using static System.Linq.Expressions.ExpressionType;

    internal class SourceCodeAnalysis : ExpressionAnalysis
    {
        private readonly SourceCodeTranslationSettings _settings;
        private readonly Stack<Expression> _expressions;
        private List<string> _requiredNamespaces;
        private MethodScope _currentMethodScope;
        private Dictionary<BlockExpression, MethodExpression> _methodsByConvertedBlock;

        private SourceCodeAnalysis(SourceCodeTranslationSettings settings)
            : base(settings)
        {
            _settings = settings;
            _expressions = new Stack<Expression>();
        }

        #region Factory Method

        public static SourceCodeAnalysis For(
            SourceCodeExpression expression,
            SourceCodeTranslationSettings settings)
        {
            var analysis = new SourceCodeAnalysis(settings);

            analysis.Visit(expression);
            analysis.Finalise();

            return analysis;
        }

        protected override ExpressionAnalysis Finalise()
        {
            if (_requiredNamespaces != null)
            {
                _requiredNamespaces.Sort(UsingsComparer.Instance);
            }
            else if (_settings.CollectRequiredNamespaces)
            {
                _requiredNamespaces = Enumerable<string>.EmptyList;
            }

            return base.Finalise();
        }

        #endregion

        public IList<string> RequiredNamespaces => _requiredNamespaces;

        public bool IsMethodBlock(BlockExpression block, out MethodExpression blockMethod)
        {
            if (_methodsByConvertedBlock == null)
            {
                blockMethod = null;
                return false;
            }

            return _methodsByConvertedBlock.TryGetValue(block, out blockMethod);
        }

        protected override void Visit(Expression expression)
        {
            if (expression == null)
            {
                return;
            }

            _expressions.Push(expression);

            switch (expression.NodeType)
            {
                case Block when ExtractToMethod((BlockExpression)expression, out var blockMethod):
                    Visit(blockMethod);
                    goto SkipBaseVisit;

                case Default:
                    Visit((DefaultExpression)expression);
                    break;

                case (ExpressionType)SourceCodeExpressionType.SourceCode:
                    Visit(((SourceCodeExpression)expression).Classes);
                    goto SkipBaseVisit;

                case (ExpressionType)SourceCodeExpressionType.Class:
                    Visit((ClassExpression)expression);
                    goto SkipBaseVisit;

                case (ExpressionType)SourceCodeExpressionType.Method:
                    Visit((MethodExpression)expression);
                    goto SkipBaseVisit;

                case (ExpressionType)SourceCodeExpressionType.MethodParameter:
                    Visit((MethodParameterExpression)expression);
                    goto SkipBaseVisit;
            }

            base.Visit(expression);

            SkipBaseVisit:
            _expressions.Pop();
        }

        private bool ExtractToMethod(BlockExpression block, out MethodExpression blockMethod)
        {
            if (ExtractToMethod(block))
            {
                blockMethod = _currentMethodScope.CreateMethodFor(block);

                (_methodsByConvertedBlock ??= new Dictionary<BlockExpression, MethodExpression>())
                    .Add(block, blockMethod);

                return true;
            }

            blockMethod = null;
            return false;
        }

        private bool ExtractToMethod(BlockExpression block)
        {
            var parentExpression = _expressions.ElementAt(1);

            switch (parentExpression.NodeType)
            {
                case Block:
                case Lambda:
                case Loop:
                case Quote:
                case Try:
                case (ExpressionType)SourceCodeExpressionType.Method:
                    return false;

                case Switch:
                    var @switch = (SwitchExpression)parentExpression;

                    if (block == @switch.DefaultBody ||
                        @switch.Cases.Any(@case => block == @case.Body))
                    {
                        return false;
                    }

                    goto default;

                default:
                    return block.Expressions.Count > 1;
            }
        }

        protected override void Visit(BlockExpression block)
        {
            _currentMethodScope.Add(block.Variables);
            base.Visit(block);
        }

        protected override bool IsAssignmentJoinable(ParameterExpression variable)
        {
            if (_currentMethodScope.IsMethodParameter(variable))
            {
                return false;
            }

            return base.IsAssignmentJoinable(variable);
        }

        private void Visit(ClassExpression @class)
        {
            AddNamespacesIfRequired(@class.Interfaces);
            Visit(@class.Methods);
        }

        protected override void Visit(ConstantExpression constant)
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

        protected override Expression Visit(MemberExpression memberAccess)
        {
            if (memberAccess.Expression == null)
            {
                // Static member access
                AddNamespaceIfRequired(memberAccess.Member.DeclaringType);
            }

            return base.Visit(memberAccess);
        }

        protected override void Visit(MethodCallExpression methodCall)
        {
            if (methodCall.Method.IsGenericMethod)
            {
                AddNamespacesIfRequired(new BclMethodWrapper(methodCall.Method)
                    .GetRequiredExplicitGenericArguments(_settings));
            }

            if (methodCall.Method.IsStatic)
            {
                AddNamespaceIfRequired(methodCall.Method.DeclaringType);
            }

            base.Visit(methodCall);
        }

        private void Visit(MethodExpression method)
        {
            EnterMethodScope(method);

            AddNamespaceIfRequired(method);

            Visit(method.Parameters);
            Visit(method.Body);

            ExitMethodScope();
        }

        private void EnterMethodScope(MethodExpression method)
            => _currentMethodScope = new MethodScope(method, _currentMethodScope, _settings);

        private void ExitMethodScope()
        {
            _currentMethodScope.Finalise();
            _currentMethodScope = _currentMethodScope.Parent;
        }

        private void Visit(MethodParameterExpression methodParameter)
            => AddNamespaceIfRequired(methodParameter);

        protected override void Visit(NewArrayExpression newArray)
        {
            AddNamespaceIfRequired(newArray.Type.GetElementType());
            base.Visit(newArray);
        }

        protected override void Visit(NewExpression newing)
        {
            AddNamespaceIfRequired(newing.Type);
            base.Visit(newing);
        }

        protected override void Visit(ParameterExpression variable)
        {
            _currentMethodScope.VariableAccessed(variable);
            base.Visit(variable);
        }

        protected override void Visit(CatchBlock @catch)
        {
            var catchVariable = @catch.Variable;

            if (catchVariable != null)
            {
                AddNamespaceIfRequired(catchVariable);
                _currentMethodScope.Add(catchVariable);
            }

            base.Visit(@catch);
        }

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

        private class MethodScope
        {
            private readonly MethodExpression _method;
            private readonly SourceCodeTranslationSettings _settings;
            private readonly List<ParameterExpression> _inScopeVariables;
            private readonly IList<ParameterExpression> _unscopedVariables;

            public MethodScope(
                MethodExpression method,
                MethodScope parent,
                SourceCodeTranslationSettings settings)
            {
                _method = method;
                Parent = parent;
                _settings = settings;
                _inScopeVariables = new List<ParameterExpression>(method.Definition.Parameters);
                _unscopedVariables = new List<ParameterExpression>();
            }

            public MethodScope Parent { get; }

            public void Add(ParameterExpression inScopeVariable)
                => _inScopeVariables.Add(inScopeVariable);

            public void Add(IEnumerable<ParameterExpression> inScopeVariables)
                => _inScopeVariables.AddRange(inScopeVariables);

            public void VariableAccessed(ParameterExpression variable)
            {
                if (_inScopeVariables.Contains(variable) ||
                    _unscopedVariables.Contains(variable))
                {
                    return;
                }

                _unscopedVariables.Add(variable);
                Parent?.VariableAccessed(variable);
            }

            public bool IsMethodParameter(ParameterExpression parameter)
            {
                VariableAccessed(parameter);

                return _unscopedVariables.Contains(parameter);
            }

            public MethodExpression CreateMethodFor(Expression block)
            {
                var blockMethod = MethodExpression.For(
                    _method.Parent,
                    block,
                    _settings,
                    isPublic: false);

                _method.Parent.AddMethod(blockMethod);
                return blockMethod;
            }

            public void Finalise()
            {
                if (_unscopedVariables.Any())
                {
                    _method.AddParameters(_unscopedVariables);
                }
            }
        }
    }
}