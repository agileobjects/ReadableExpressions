namespace AgileObjects.ReadableExpressions.Build.SourceCode.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;
    using ReadableExpressions.Translations;
    using ReadableExpressions.Translations.Interfaces;
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
            SourceCodeTranslationSettings settings)
            : this(
                expression,
                SourceCodeAnalysis.For(expression, settings),
                settings)
        {
        }

        private SourceCodeExpressionTranslation(
            Expression expression,
            SourceCodeAnalysis analysis,
            SourceCodeTranslationSettings settings)
            : base(expression, analysis, settings)
        {
            _analysis = analysis;
        }

        #region ISourceCodeTranslationContext Members

        IList<string> ISourceCodeTranslationContext.RequiredNamespaces
            => _analysis.RequiredNamespaces;

        public override ITranslation GetTranslationFor(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            switch (expression.NodeType)
            {
                case Block:
                    var block = (BlockExpression)expression;

                    if (_currentMethod.Body != block &&
                        _analysis.IsMethodBlock(block, out var method))
                    {
                        return MethodCallTranslation.For(method, this);
                    }

                    break;

                case (ExpressionType)SourceCode:
                    return new SourceCodeTranslation((SourceCodeExpression)expression, this);

                case (ExpressionType)Class:
                    return new ClassTranslation((ClassExpression)expression, this);

                case (ExpressionType)Method:
                    return new MethodTranslation(_currentMethod = (MethodExpression)expression, this);
            }

            return base.GetTranslationFor(expression);
        }

        #endregion
    }
}