namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Linq.Expressions;

    public class CommentExpression : Expression
    {
        private const string CommentString = "// ";

        internal CommentExpression(string text)
        {
            Comment = CommentString + text
                .Trim()
                .Replace(Environment.NewLine, Environment.NewLine + CommentString);
        }

        public static bool IsComment(string codeLine)
        {
            return codeLine.TrimStart().StartsWith(CommentString, StringComparison.Ordinal);
        }

        public override ExpressionType NodeType => ExpressionType.Constant;

        public override Type Type => typeof(string);

        public override bool CanReduce => false;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // Using an instance of this Type in an ExpressionVisitor throws 
            // an ArgumentException from Expression.VisitChildren with the 
            // message 'Must be reducible node' - despite CanReduce returning 
            // false. Easiest way to deal with that is to override VisitChildren 
            // to do nothing:
            return this;
        }

        public string Comment { get; }

        public override string ToString()
        {
            return Comment;
        }
    }
}