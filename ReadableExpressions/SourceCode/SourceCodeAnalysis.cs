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
        private List<string> _requiredNamespaces;
        private ClassExpression _currentClass;
        private Dictionary<BlockExpression, MethodExpression> _methodsByConvertedBlock;

        private SourceCodeAnalysis(TranslationSettings settings)
            : base(settings)
        {
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

                default:
                    switch ((SourceCodeExpressionType)expression.NodeType)
                    {
                        case SourceCodeExpressionType.SourceCode:
                            Visit(((SourceCodeExpression)expression).Classes);
                            break;

                        case SourceCodeExpressionType.Class:
                            Visit((ClassExpression)expression);
                            break;

                        case SourceCodeExpressionType.Method:
                            Visit((MethodExpression)expression);
                            break;

                        case SourceCodeExpressionType.MethodParameter:
                            Visit((MethodParameterExpression)expression);
                            break;
                    }

                    return;
            }

            base.Visit(expression);
        }

        private void Visit(ClassExpression @class)
        {
            _currentClass = @class;
            Visit(@class.Methods);
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

        private void Visit(MemberExpression memberAccess)
        {
            if (memberAccess.Expression == null)
            {
                // Static member access
                AddNamespaceIfRequired(memberAccess.Member.DeclaringType);
            }
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

        protected override void Visit(CatchBlock @catch)
        {
            var catchVariable = @catch.Variable;

            if (catchVariable != null)
            {
                AddNamespaceIfRequired(catchVariable);
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
    }
}