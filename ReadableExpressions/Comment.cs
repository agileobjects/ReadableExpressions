namespace AgileObjects.ReadableExpressions
{
    using System.Collections.Generic;
    using Extensions;
    using static System.Environment;

    /// <summary>
    /// Represents a double-slash-commented text comment.
    /// </summary>
    public class Comment
    {
        private const string _commentString = "// ";

        internal Comment(string comment)
        {
            var textLines = comment.Trim().SplitToLines();

            Text =
                _commentString +
                string.Join(NewLine + _commentString, textLines);

            TextLines = textLines;
        }

        /// <summary>
        /// Gets the double-slash-commented text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the lines of the comment, which are the <see cref="Text"/> string split at newlines.
        /// </summary>
        public IEnumerable<string> TextLines { get; }

        /// <summary>
        /// Gets a string representation of this <see cref="Comment"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="Comment"/>.</returns>
        public override string ToString() => Text;
    }
}