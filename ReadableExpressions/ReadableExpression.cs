namespace AgileObjects.ReadableExpressions
{
    public class ReadableExpression
    {
        public static CommentExpression Comment(string text)
        {
            return new CommentExpression(text);
        }
    }
}
