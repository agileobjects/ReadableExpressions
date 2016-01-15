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

        public override ExpressionType NodeType => ExpressionType.Constant;

        public override Type Type => typeof(string);

        public string Comment { get; }

        public override string ToString()
        {
            return Comment;
        }
    }
}