namespace AgileObjects.ReadableExpressions.SourceCode.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;
    using SourceCode;
    using ReadableExpressions.Translations;
    using ReadableExpressions.Translations.Interfaces;
    using ReadableExpressions.Translations.SourceCode;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif
    using static SourceCodeExpressionType;

    internal class SourceCodeExpressionTranslation :
        ExpressionTranslation,
        ISourceCodeTranslationContext
    {
        private readonly SourceCodeAnalysis _analysis;
        private MethodExpression _currentMethod;

        public SourceCodeExpressionTranslation(
            SourceCodeExpression expression,
            TranslationSettings settings)
            : this(
                expression,
                SourceCodeAnalysis.For(expression, settings),
                settings)
        {
        }

        private SourceCodeExpressionTranslation(
            Expression expression,
            SourceCodeAnalysis analysis,
            TranslationSettings settings)
            : base(expression, analysis, settings)
        {
            _analysis = analysis;
        }

        #region ISourceCodeTranslationContext Members

        IList<string> ISourceCodeTranslationContext.RequiredNamespaces
            => _analysis.RequiredNamespaces;

        IList<ParameterExpression> ISourceCodeTranslationContext.GetUnscopedVariablesFor(
            MethodExpression method)
        {
            return _analysis.UnscopedVariablesByMethod.TryGetValue(method, out var variables)
                ? variables : Enumerable<ParameterExpression>.EmptyList;
        }

        public override ITranslation GetTranslationFor(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.NodeType == Block)
            {
                var block = (BlockExpression)expression;

                if (_currentMethod.Body != block &&
                    _analysis.IsMethodBlock(block, out var method))
                {
                    return MethodCallTranslation.For(method, this);
                }
            }

            switch ((SourceCodeExpressionType)expression.NodeType)
            {
                case SourceCode:
                    return new SourceCodeTranslation((SourceCodeExpression)expression, this);

                case Class:
                    return new ClassTranslation((ClassExpression)expression, this);

                case Method:
                    return new MethodTranslation(_currentMethod = (MethodExpression)expression, this);
            }

            return base.GetTranslationFor(expression);
        }

        #endregion
    }
}