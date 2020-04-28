namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Formatting;
    using Interfaces;

    internal class CommentTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly string _comment;

        public CommentTranslation(string comment)
        {
            _comment = comment;
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public Type Type => typeof(string);

        public int EstimatedSize => _comment.Length;

        public bool IsTerminated => true;

        public void WriteTo(TranslationBuffer buffer)
            => buffer.WriteToTranslation(_comment, TokenType.Comment);
    }
}
