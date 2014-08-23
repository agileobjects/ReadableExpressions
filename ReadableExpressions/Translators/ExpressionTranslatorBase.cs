namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal abstract class ExpressionTranslatorBase : IExpressionTranslator
    {
        private readonly ExpressionType[] _nodeTypes;

        protected ExpressionTranslatorBase(params ExpressionType[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
        }

        public IEnumerable<ExpressionType> NodeTypes
        {
            get { return _nodeTypes; }
        }

        public abstract string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry);
    }
}