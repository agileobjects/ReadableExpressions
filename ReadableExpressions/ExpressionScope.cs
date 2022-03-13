namespace AgileObjects.ReadableExpressions
{
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
        private Dictionary<ParameterExpression, VariableInfo> _variableInfos;
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
                if (_variableInfos != null)
                {
                    foreach (var variable in _variableInfos.Keys)
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

        public bool TryAddFirstAccess(ParameterExpression variable)
        {
            GetOrAddVariableInfo(variable, out var variableAdded);
            return variableAdded;
        }

        public void DeclareInVariableList(ParameterExpression variable) 
            => GetVariableInfo(variable).DeclarationType = DeclarationType.VariableList;

        public void DeclareInOutputParameterUse(ParameterExpression parameter)
        {
            var variableInfo =
                GetOrAddVariableInfo(parameter, out var variableAdded);

            if (variableAdded)
            {
                variableInfo.DeclarationType = DeclarationType.InlineOutput;
            }
        }

        public void DeclareInAssignment(ParameterExpression variable) 
            => GetVariableInfo(variable).DeclarationType = DeclarationType.JoinedAssignment;

        public void AddCatchBlockVariable(ParameterExpression variable)
            => GetOrAddVariableInfo(variable, out _).IsCatchBlockVariable = true;

        public bool IsCatchBlockVariable(ParameterExpression variable)
            => GetVariableInfo(variable).IsCatchBlockVariable;

        public bool IsJoinedAssignmentVariable(ParameterExpression variable)
        {
            return GetVariableInfoOrNull(variable)?
                .DeclarationType == DeclarationType.JoinedAssignment;
        }

        public bool ShouldDeclareInVariableList(ParameterExpression variable)
        {
            var info = GetVariableInfoOrNull(variable);

            return info == null ||
                   info.DeclarationType == DeclarationType.VariableList;
        }

        public bool ShouldDeclareInOutputParameterUse(ParameterExpression variable)
        {
            var variableInfo = GetVariableInfo(variable);

            if (variableInfo.HasBeenDeclared ||
                variableInfo.DeclarationType != DeclarationType.InlineOutput)
            {
                return false;
            }

            variableInfo.HasBeenDeclared = true;
            return true;
        }

        private VariableInfo GetVariableInfo(ParameterExpression variable)
            => GetDeclaringScope(variable)._variableInfos[variable];

        private VariableInfo GetVariableInfoOrNull(ParameterExpression variable)
        {
            var variableInfos = GetDeclaringScope(variable)._variableInfos;

            if (variableInfos != null &&
                variableInfos.TryGetValue(variable, out var info))
            {
                return info;
            }

            return null;
        }

        private VariableInfo GetOrAddVariableInfo(
            ParameterExpression variable,
            out bool variableAdded)
        {
            var declaringScope = GetDeclaringScope(variable);
            var variableInfos = declaringScope._variableInfos;

            VariableInfo info;

            if (variableInfos != null)
            {
                if (variableInfos.TryGetValue(variable, out info))
                {
                    variableAdded = false;
                    return info;
                }

                goto AddNewInfo;
            }

            variableInfos = declaringScope._variableInfos =
                new Dictionary<ParameterExpression, VariableInfo>();

        AddNewInfo:
            variableAdded = true;
            info = new();
            variableInfos.Add(variable, info);
            return info;
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

        #region Helper Class

        private class VariableInfo
        {
            public DeclarationType DeclarationType;
            public bool IsCatchBlockVariable;
            public bool HasBeenDeclared;
        }

        private enum DeclarationType
        {
            VariableList,
            InlineOutput,
            JoinedAssignment
        }

        #endregion
    }
}