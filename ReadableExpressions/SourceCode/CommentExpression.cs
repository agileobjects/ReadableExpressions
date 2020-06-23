namespace AgileObjects.ReadableExpressions.SourceCode
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System;
    using static System.Environment;
    using static System.StringSplitOptions;

    /// <summary>
    /// Represents a source code comment.
    /// </summary>
    public class CommentExpression : Expression
    {
        private static readonly string[] _newLines = { NewLine };
        private const string _commentString = "// ";

        internal CommentExpression(string comment)
        {
            Comment =
                _commentString +
                string.Join(NewLine + _commentString, comment.Trim().Split(_newLines, None));
        }

        /// <summary>
        /// Gets the <see cref="SourceCodeExpressionType"/> value (1004) indicating the type of this
        /// <see cref="CommentExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType
            => (ExpressionType)SourceCodeExpressionType.Comment;

        /// <summary>
        /// Gets the type of this <see cref="CommentExpression"/> - typeof(string).
        /// </summary>
        public override Type Type => typeof(string);

        /// <summary>
        /// Gets the double-slash-commented text.
        /// </summary>
        public string Comment { get; }

        /// <summary>
        /// Gets a string representation of this <see cref="CommentExpression"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="CommentExpression"/>.</returns>
        public override string ToString() => Comment;
    }
}