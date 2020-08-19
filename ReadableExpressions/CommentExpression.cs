namespace AgileObjects.ReadableExpressions
{
    using System;
    using System.Collections.Generic;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static System.Environment;

    /// <summary>
    /// Represents a source code comment.
    /// </summary>
    public class CommentExpression : Expression
    {
        /// <summary>
        /// Gets the numeric value used for a <see cref="CommentExpression"/>'s ExpressionType.
        /// </summary>
        public const ExpressionType ExpressionType = (ExpressionType)1004;

        private const string _commentString = "// ";

        internal CommentExpression(string comment)
        {
            var textLines = comment.Trim().SplitToLines();

            Comment =
                _commentString +
                string.Join(NewLine + _commentString, textLines);

            TextLines = textLines;
        }

        /// <summary>
        /// Gets the ExpressionType value (1004) indicating the type of this
        /// <see cref="CommentExpression"/> as an ExpressionType.
        /// </summary>
        public override ExpressionType NodeType => ExpressionType;

        /// <summary>
        /// Gets the type of this <see cref="CommentExpression"/> - typeof(string).
        /// </summary>
        public override Type Type => typeof(string);

        /// <summary>
        /// Gets the double-slash-commented text.
        /// </summary>
        public string Comment { get; }

        /// <summary>
        /// Gets the lines of the comment, which are the <see cref="Comment"/> string split at
        /// newlines.
        /// </summary>
        public IEnumerable<string> TextLines { get; }

        /// <summary>
        /// Gets a string representation of this <see cref="CommentExpression"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="CommentExpression"/>.</returns>
        public override string ToString() => Comment;
    }
}