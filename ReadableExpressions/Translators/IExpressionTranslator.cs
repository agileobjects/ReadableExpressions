namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IExpressionTranslator
    {
        IEnumerable<ExpressionType> NodeTypes
        {
            get;
        }

        string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry);
    }
}