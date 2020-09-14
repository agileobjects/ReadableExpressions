namespace AgileObjects.ReadableExpressions
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    /// <summary>
    /// Represents a double-slash-commented text comment.
    /// </summary>
    public class CommentExpression : Expression
    {
        internal CommentExpression(string commentText)
        {
            Comment = new Comment(commentText);
        }

        /// <summary>
        /// Converts the given <paramref name="comment"/> to a ConstantExpression of type
        /// <see cref="Comment"/>, containing the <see cref="Comment"/> object.
        /// </summary>
        /// <param name="comment">The <see cref="CommentExpression"/> to convert.</param>
        public static implicit operator ConstantExpression(CommentExpression comment)
            => Constant(comment.Comment, typeof(Comment));

        /// <summary>
        /// Gets the ExpressionType value indicating that this <see cref="CommentExpression"/> should
        /// be treated as a Constant.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Constant;

        /// <summary>
        /// Gets the type of this <see cref="CommentExpression"/> - typeof(<see cref="Comment"/>).
        /// </summary>
        public override Type Type => typeof(Comment);

        /// <summary>
        /// Gets the <see cref="Comment"/> containing the commented text.
        /// </summary>
        public Comment Comment { get; set; }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor) => this;

        /// <summary>
        /// Gets a string representation of this <see cref="CommentExpression"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="CommentExpression"/>.</returns>
        public override string ToString() => Comment.ToString();
    }
}