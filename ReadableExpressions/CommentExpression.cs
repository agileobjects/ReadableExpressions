namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Linq.Expressions;

    public class CommentExpression : Expression
    {
        internal CommentExpression(string text)
        {
            Comment = "// " + text;
        }

        public static bool IsComment(string codeLine)
        {
            return codeLine.StartsWith("// ", StringComparison.Ordinal);
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(string);

        public string Comment { get; }

        public override string ToString()
        {
            return Comment;
        }
    }
}