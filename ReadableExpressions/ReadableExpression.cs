namespace AgileObjects.ReadableExpressions
{
    using System.Linq.Expressions;

    public class ReadableExpression
    {
        public static ConstantExpression Comment(string text)
        {
            return Expression.Constant(text.AsComment());
        }
    }
}
