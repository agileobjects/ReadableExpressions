namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal abstract class ExpressionTranslatorBase : IExpressionTranslator
    {
        private readonly ExpressionType[] _nodeTypes;

        protected ExpressionTranslatorBase(
            IExpressionTranslatorRegistry registry,
            params ExpressionType[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
            Registry = registry;
        }

        protected IExpressionTranslatorRegistry Registry { get; }

        public IEnumerable<ExpressionType> NodeTypes => _nodeTypes;

        public abstract string Translate(Expression expression);
    }
}