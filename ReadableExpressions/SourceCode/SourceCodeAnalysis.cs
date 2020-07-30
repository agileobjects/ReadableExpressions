namespace AgileObjects.ReadableExpressions.SourceCode
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Translations.Reflection;

    internal class SourceCodeAnalysis : ExpressionAnalysis
    {
        private readonly Stack<MethodScope> _methodScopes;
        private List<string> _requiredNamespaces;
        private ClassExpression _currentClass;
        private Dictionary<BlockExpression, MethodExpression> _methodsByConvertedBlock;

        private SourceCodeAnalysis(TranslationSettings settings)
            : base(settings)
        {
            _methodScopes = new Stack<MethodScope>();
            _methodsByConvertedBlock = new Dictionary<BlockExpression, MethodExpression>();
        }

        #region Factory Method

        public static SourceCodeAnalysis For(SourceCodeExpression expression, TranslationSettings settings)
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
            else if (Settings.CollectRequiredNamespaces)
            {
                _requiredNamespaces = Enumerable<string>.EmptyList;
            }

            return base.Finalise();
        }

        #endregion

        private MethodScope CurrentScope => _methodScopes.Peek();

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

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    Visit((ConstantExpression)expression);
                    break;

                case ExpressionType.Default:
                    Visit((DefaultExpression)expression);
                    break;

                case ExpressionType.MemberAccess:
                    Visit((MemberExpression)expression);
                    break;

                case (ExpressionType)SourceCodeExpressionType.SourceCode:
                    Visit(((SourceCodeExpression)expression).Classes);
                    return;

                case (ExpressionType)SourceCodeExpressionType.Class:
                    Visit((ClassExpression)expression);
                    return;

                case (ExpressionType)SourceCodeExpressionType.Method:
                    Visit((MethodExpression)expression);
                    return;

                case (ExpressionType)SourceCodeExpressionType.MethodParameter:
                    Visit((MethodParameterExpression)expression);
                    return;
            }

            base.Visit(expression);
        }

        protected override void Visit(BlockExpression block)
        {
            CurrentScope.Add(block.Variables);
            base.Visit(block);
        }

        private void Visit(ClassExpression @class)
        {
            _currentClass = @class;
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
                    .GetRequiredExplicitGenericArguments(Settings));
            }

            if (methodCall.Method.IsStatic)
            {
                AddNamespaceIfRequired(methodCall.Method.DeclaringType);
            }

            base.Visit(methodCall);
        }

        private void Visit(MethodExpression method)
        {
            _methodScopes.Push(new MethodScope(method));

            AddNamespaceIfRequired(method);

            Visit(method.Parameters);
            Visit(method.Body);

            _methodScopes.Pop().Finalise();
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
            CurrentScope.VariableAccessed(variable);
            base.Visit(variable);
        }

        protected override void Visit(CatchBlock @catch)
        {
            var catchVariable = @catch.Variable;

            if (catchVariable != null)
            {
                AddNamespaceIfRequired(catchVariable);
                CurrentScope.Add(catchVariable);
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
            if (!Settings.CollectRequiredNamespaces ||
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
            private readonly List<ParameterExpression> _inScopeVariables;
            private readonly IList<ParameterExpression> _unscopedVariables;

            public MethodScope(MethodExpression method)
            {
                _method = method;
                _inScopeVariables = new List<ParameterExpression>(method.Definition.Parameters);
                _unscopedVariables = new List<ParameterExpression>();
            }

            public void Add(ParameterExpression inScopeVariable)
                => _inScopeVariables.Add(inScopeVariable);

            public void Add(IEnumerable<ParameterExpression> inScopeVariables)
                => _inScopeVariables.AddRange(inScopeVariables);

            public void VariableAccessed(ParameterExpression variable)
            {
                if (!_inScopeVariables.Contains(variable) &&
                    !_unscopedVariables.Contains(variable))
                {
                    _unscopedVariables.Add(variable);
                }
            }

            public void Finalise()
            {
                if (_unscopedVariables.Any())
                {
                }
            }
        }
    }
}