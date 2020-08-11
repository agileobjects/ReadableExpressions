namespace AgileObjects.ReadableExpressions
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static System.Environment;
    using static System.StringSplitOptions;
    using static ReadableExpressionConstants;

    /// <summary>
    /// Represents a source code comment.
    /// </summary>
    public class CommentExpression : Expression
    {
        private static readonly string[] _newLines = { NewLine };
        private const string _commentString = "// ";

        internal CommentExpression(string comment)
        {
            TextLines = comment.Trim().Split(_newLines, None);

            Comment =
                _commentString +
                string.Join(NewLine + _commentString, TextLines);
        }

        /// <summary>
        /// Gets the ExpressionType value (1004) indicating the type of this
        /// <see cref="CommentExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType => (ExpressionType)ExpressionTypeComment;

        /// <summary>
        /// Gets the type of this <see cref="CommentExpression"/> - typeof(string).
        /// </summary>
        public override Type Type => typeof(string);

        /// <summary>
        /// Gets the double-slash-commented text.
        /// </summary>
        public string Comment { get; }

        internal string[] TextLines { get; }

        /// <summary>
        /// Gets a string representation of this <see cref="CommentExpression"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="CommentExpression"/>.</returns>
        public override string ToString() => Comment;
    }
}