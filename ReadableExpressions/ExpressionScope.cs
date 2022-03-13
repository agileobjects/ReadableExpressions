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

    internal class ExpressionScope
    {
        private BlockExpression _block;
        private ICollection<ParameterExpression> _accessedVariables;
        private IList<ParameterExpression> _joinedAssignmentVariables;
        private IList<ParameterExpression> _inlineOutputVariables;
        private ICollection<ParameterExpression> _catchBlockVariables;
        private List<ExpressionScope> _childScopes;

        public ExpressionScope()
        {
            _childScopes = new List<ExpressionScope>();
        }

        public ExpressionScope(BlockExpression block, ExpressionScope parent)
            : this((object)block, parent)
        {
            _block = block;
        }

        public ExpressionScope(object scopeObject, ExpressionScope parent)
        {
            ScopeObject = scopeObject;
            Parent = parent;
            (parent._childScopes ??= new List<ExpressionScope>()).Add(this);
        }

        public ExpressionScope Parent { get; }

        public object ScopeObject { get; private set; }

        public IEnumerable<ParameterExpression> AllVariables
        {
            get
            {
                if (_accessedVariables != null)
                {
                    foreach (var variable in _accessedVariables)
                    {
                        yield return variable;
                    }
                }

                if (_childScopes != null)
                {
                    var childScopeVariables = _childScopes
                        .SelectMany(childScope => childScope.AllVariables);

                    foreach (var variable in childScopeVariables)
                    {
                        yield return variable;
                    }
                }
            }
        }

        public BlockExpression GetCurrentBlockOrNull()
            => _block ?? Parent?.GetCurrentBlockOrNull();

        public void Set(BlockExpression block) => _block = block;

        public object GetCurrentConstructObjectOrNull()
        {
            return _block == null
                ? ScopeObject
                : Parent?.GetCurrentConstructObjectOrNull();
        }

        public void Set(object scopeObject) => ScopeObject = scopeObject;

        public bool Contains(object scopeObject)
        {
            return ScopeObject == scopeObject ||
                   Parent?.Contains(scopeObject) == true;
        }

        public ExpressionScope FindScopeFor(BlockExpression block)
        {
            if (_block == block)
            {
                return this;
            }

            return _childScopes?
                .Project(childScope => childScope.FindScopeFor(block))
                .Filter(scope => scope != null)
                .FirstOrDefault();
        }

        public bool HasBeenAccessed(ParameterExpression variable)
        {
            var declaringScope = GetDeclaringScope(variable);

            return declaringScope.HasBeenAdded(
                scope => scope._accessedVariables,
                variable,
                out _);
        }

        public void AddVariableAccess(ParameterExpression variable)
            => (_accessedVariables ??= new List<ParameterExpression>()).Add(variable);

        public bool HasBeenJoinAssigned(ParameterExpression variable)
            => HasBeenJoinAssigned(variable, out _);

        private bool HasBeenJoinAssigned(
            ParameterExpression variable,
            out ICollection<ParameterExpression> variables)
        {
            var declaringScope = GetDeclaringScope(variable);

            return declaringScope.HasBeenAdded(
                scope => scope._joinedAssignmentVariables,
                variable,
                out variables);
        }

        public void AddJoinedAssignmentVariable(ParameterExpression variable)
            => (_joinedAssignmentVariables ??= new List<ParameterExpression>()).Add(variable);

        public void RemoveJoinedAssignmentVariable(ParameterExpression variable)
        {
            if (HasBeenJoinAssigned(variable, out var variables))
            {
                variables.Remove(variable);
            }
        }

        public bool IsInlineOutputParameter(ParameterExpression variable)
        {
            var declaringScope = GetDeclaringScope(variable);

            return declaringScope.HasBeenAdded(
                scope => scope._inlineOutputVariables,
                variable,
                out _);
        }

        public void AddInlineOutputVariable(ParameterExpression parameter)
        {
            (_inlineOutputVariables ??= new List<ParameterExpression>())
                .Add(parameter);
        }

        public void AddCatchBlockVariable(ParameterExpression variable)
        {
            (_catchBlockVariables ??= new List<ParameterExpression>())
                .Add(variable);
        }

        public bool IsCatchBlockVariable(ParameterExpression variable)
        {
            var declaringScope = GetDeclaringScope(variable);

            return declaringScope.HasBeenAdded(
                scope => scope._catchBlockVariables,
                variable,
                out _);
        }

        private ExpressionScope GetDeclaringScope(ParameterExpression variable)
        {
            if (Parent == null ||
                _block?.Variables.Contains(variable) == true)
            {
                return this;
            }

            return Parent.GetDeclaringScope(variable);
        }

        private bool HasBeenAdded(
            Func<ExpressionScope, ICollection<ParameterExpression>> variablesGetter,
            ParameterExpression variable,
            out ICollection<ParameterExpression> variables)
        {
            variables = variablesGetter.Invoke(this);

            if (variables?.Contains(variable) == true)
            {
                return true;
            }

            if (_childScopes?.Any() != true)
            {
                return false;
            }

            foreach (var childScope in _childScopes)
            {
                if (childScope.HasBeenAdded(
                        variablesGetter,
                        variable,
                        out variables))
                {
                    return true;
                }
            }

            return false;
        }
    }
}