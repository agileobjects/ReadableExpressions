namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines a method which translates an <see cref="Expression"/> into a source code string.
    /// </summary>
    /// <param name="expression">The <see cref="Expression"/> to translate.</param>
    /// <param name="context">
    /// The <see cref="TranslationContext"/> for the root <see cref="Expression"/> being translated.
    /// </param>
    /// <returns>A source code translation of the given <paramref name="expression"/>.</returns>
    public delegate string Translator(Expression expression, TranslationContext context);

    internal abstract class ExpressionTranslatorBase : IExpressionTranslator
    {
        private readonly ExpressionType[] _nodeTypes;

        protected ExpressionTranslatorBase(params ExpressionType[] nodeTypes)
        {
            _nodeTypes = nodeTypes;
        }

        public IEnumerable<ExpressionType> NodeTypes => _nodeTypes;

        public abstract string Translate(Expression expression, TranslationContext context);
    }
}