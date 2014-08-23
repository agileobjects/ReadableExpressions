namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Linq.Expressions;

    public interface IExpressionTranslatorRegistry
    {
        string Translate(Expression expression);
    }
}
