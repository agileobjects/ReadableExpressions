﻿namespace AgileObjects.ReadableExpressions
{
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

    internal class ExpressionScope
    {
        private readonly DeclarationType _defaultDeclarationType;
        private Expression _scopeExpression;
        private ICollection<ParameterExpression> _variables;
        private Dictionary<ParameterExpression, VariableInfo> _variableInfos;
        private List<ExpressionScope> _childScopes;

        public ExpressionScope()
        {
            _childScopes = new List<ExpressionScope>();
        }

        public ExpressionScope(LambdaExpression lambda, ExpressionScope parent) :
            this(parent)
        {
            _defaultDeclarationType = DeclarationType.None;
            Set(lambda);
        }

        public ExpressionScope(BlockExpression block, ExpressionScope parent) :
            this(parent)
        {
            _defaultDeclarationType = DeclarationType.VariableList;
            Set(block);
        }

        public ExpressionScope(object scopeObject, ExpressionScope parent) :
            this(parent)
        {
            Set(scopeObject);
        }

        private ExpressionScope(ExpressionScope parent)
        {
            Parent = parent;
            (parent._childScopes ??= new List<ExpressionScope>()).Add(this);
        }

        public ExpressionScope Parent { get; }

        public object ScopeObject { get; private set; }

        public IEnumerable<ParameterExpression> AllVariables
            => AllVariableInfos.Project(info => info.Variable);

        private IEnumerable<VariableInfo> AllVariableInfos
        {
            get
            {
                if (_variableInfos != null)
                {
                    foreach (var variableInfo in _variableInfos.Values)
                    {
                        yield return variableInfo;
                    }
                }

                if (_childScopes != null)
                {
                    var childScopeVariableInfos = _childScopes
                        .SelectMany(childScope => childScope.AllVariableInfos);

                    foreach (var variableInfo in childScopeVariableInfos)
                    {
                        yield return variableInfo;
                    }
                }
            }
        }

        public BlockExpression GetCurrentBlockOrNull()
        {
            if (_scopeExpression?.NodeType == Block)
            {
                return (BlockExpression)_scopeExpression;
            }

            return Parent?.GetCurrentBlockOrNull();
        }

        public void Set(LambdaExpression lambda)
        {
            ScopeObject = lambda;
            _scopeExpression = lambda;
            _variables = lambda.Parameters;
        }

        public void Set(BlockExpression block)
        {
            ScopeObject = block;
            _scopeExpression = block;
            _variables = block.Variables;
        }

        public void Set(object scopeObject) => ScopeObject = scopeObject;

        public object GetCurrentConstructObjectOrNull()
        {
            return _scopeExpression == null
                ? ScopeObject
                : Parent?.GetCurrentConstructObjectOrNull();
        }

        public ExpressionScope FindScopeFor(BlockExpression block)
        {
            if (_scopeExpression == block)
            {
                return this;
            }

            return _childScopes?
                .Project(childScope => childScope.FindScopeFor(block))
                .Filter(scope => scope != null)
                .FirstOrDefault();
        }

        public bool TryAddFirstAccess(
            ParameterExpression variable,
            BinaryExpression assignment)
        {
            if (!TryAddFirstAccess(variable, out var variableInfo))
            {
                return false;
            }

            variableInfo.AssignmentParentConstruct =
                GetCurrentConstructObjectOrNull();

            DeclareInAssignment(variableInfo, assignment);
            return true;
        }

        public bool TryAddFirstAccess(ParameterExpression variable)
            => TryAddFirstAccess(variable, out _);

        private bool TryAddFirstAccess(
            ParameterExpression variable,
            out VariableInfo variableInfo)
        {
            variableInfo = GetOrAddVariableInfo(variable, out var variableAdded);

            if (variableAdded)
            {
                return true;
            }

            ++variableInfo.ReferenceCount;
            return false;
        }

        public ParameterExpression ReevaluateDeclaration(ParameterExpression variable)
        {
            var variableInfo = GetVariableInfoOrNull(variable);

            if (!IsJoinedAssignment(variableInfo))
            {
                return variable;
            }

            var parentConstruct = variableInfo.AssignmentParentConstruct;

            if (parentConstruct == null || IsForOrIsWithin(parentConstruct))
            {
                return variable;
            }

            // This variable was assigned within a construct but is being accessed 
            // outside of that scope, so the assignment shouldn't be joined:
            DeclareInVariableList(variable);
            return variable;
        }

        private void DeclareInVariableList(ParameterExpression variable)
            => GetVariableInfo(variable).DeclarationType = DeclarationType.VariableList;

        public void AddOutputParameter(ParameterExpression parameter)
        {
            var variableInfo =
                GetOrAddVariableInfo(parameter, out var variableAdded);

            if (variableAdded)
            {
                // Decrement the reference count to ensure this access
                // doesn't count towards whether the variable is used:
                --variableInfo.ReferenceCount;
                variableInfo.DeclarationType = DeclarationType.InlineOutput;
            }
        }

        public void DeclareInAssignment(
            ParameterExpression variable,
            Expression assignment)
        {
            var variableInfo = GetVariableInfo(variable);
            DeclareInAssignment(variableInfo, assignment);
        }

        private static void DeclareInAssignment(
            VariableInfo variableInfo,
            Expression assignment)
        {
            variableInfo.DeclarationType = DeclarationType.JoinedAssignment;
            variableInfo.Assignment = assignment;
        }

        public void AddCatchBlockVariable(ParameterExpression variable)
            => GetOrAddVariableInfo(variable, out _).IsCatchBlockVariable = true;

        public bool IsCatchBlockVariable(ParameterExpression variable)
            => GetVariableInfo(variable).IsCatchBlockVariable;

        private bool IsForOrIsWithin(object scopeObject)
        {
            return ScopeObject == scopeObject ||
                   Parent?.IsForOrIsWithin(scopeObject) == true;
        }

        public bool IsJoinedAssignmentVariable(ParameterExpression variable)
            => IsJoinedAssignment(GetVariableInfoOrNull(variable));

        private static bool IsJoinedAssignment(VariableInfo variableInfo)
            => variableInfo?.DeclarationType == DeclarationType.JoinedAssignment;

        public bool IsJoinedAssignment(BinaryExpression assignment)
        {
            if (assignment.Left.NodeType != Parameter)
            {
                return false;
            }

            var variable = (ParameterExpression)assignment.Left;
            var variableInfo = GetVariableInfoOrNull(variable);

            return
                variableInfo?.Assignment == assignment &&
                variableInfo.DeclarationType == DeclarationType.JoinedAssignment;
        }

        public bool ShouldDeclareInVariableList(
            ParameterExpression variable,
            out bool isUsed)
        {
            var info = GetVariableInfoOrNull(variable);

            if (info == null)
            {
                isUsed = false;
                return true;
            }

            isUsed = info.IsUsed;
            return info.DeclarationType == DeclarationType.VariableList;
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

        public bool IsUsed(ParameterExpression parameter)
        {
            var declaringScope = GetDeclaringScope(parameter);

            foreach (var variableInfo in declaringScope.AllVariableInfos)
            {
                if (VariableComparer.Instance.Equals(variableInfo.Variable, parameter))
                {
                    return variableInfo.IsUsed;
                }
            }

            return false;
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

            variableInfos = declaringScope.EnsureVariableInfos();

        AddNewInfo:
            info = new()
            {
                Variable = variable,
                DeclarationType = declaringScope._defaultDeclarationType
            };

            variableInfos.Add(variable, info);
            variableAdded = true;
            return info;
        }

        private Dictionary<ParameterExpression, VariableInfo> EnsureVariableInfos()
        {
            return _variableInfos =
                new Dictionary<ParameterExpression, VariableInfo>(
                    VariableComparer.Instance);
        }

        private ExpressionScope GetDeclaringScope(ParameterExpression variable)
        {
            var declaringScope = default(ExpressionScope);
            var searchScope = this;

            ExpressionScope rootScope;

            do
            {
                rootScope = searchScope;

                if (searchScope.Declares(variable))
                {
                    declaringScope = searchScope;
                }

                searchScope = searchScope.Parent;
            }
            while (searchScope != null);

            return declaringScope ?? rootScope;
        }

        private bool Declares(ParameterExpression variable)
        {
            if (_variables?.Any() != true)
            {
                return false;
            }

            foreach (var scopeVariable in _variables)
            {
                if (VariableComparer.Instance.Equals(variable, scopeVariable))
                {
                    return true;
                }
            }

            return false;
        }

        #region Helper Classes

        private class VariableInfo
        {
            public ParameterExpression Variable;
            public DeclarationType DeclarationType;
            public Expression Assignment;
            public object AssignmentParentConstruct;
            public bool IsCatchBlockVariable;
            public bool HasBeenDeclared;
            public int ReferenceCount;

            public bool IsUsed => ReferenceCount > 0;
        }

        private enum DeclarationType
        {
            None,
            VariableList,
            InlineOutput,
            JoinedAssignment
        }

        private class VariableComparer : IEqualityComparer<ParameterExpression>
        {
            public static readonly IEqualityComparer<ParameterExpression> Instance =
                new VariableComparer();

            public bool Equals(ParameterExpression x, ParameterExpression y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                return x!.Type == y!.Type && x.Name == y.Name;
            }

            public int GetHashCode(ParameterExpression param)
            {
                var hashCode = param.Type.GetHashCode();

                if (param.Name == null)
                {
                    return hashCode;
                }

                unchecked
                {
                    return (hashCode * 397) ^ param.Name.GetHashCode();
                }
            }
        }

        #endregion
    }
}