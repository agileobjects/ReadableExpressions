namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Formatting;
    using Interfaces;

    internal class CommentTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly string _comment;

        public CommentTranslation(CommentExpression comment, ITranslationContext context)
        {
            _comment = comment.Comment;
            FormattingSize = context.GetFormattingSize(TokenType.Comment);
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public Type Type => typeof(string);

        public int TranslationSize => _comment.Length;

        public int FormattingSize { get; }

        public bool IsTerminated => true;

        public int GetIndentSize() => 0;

        public int GetLineCount() => _comment.GetLineCount();

        public void WriteTo(TranslationWriter writer)
            => writer.WriteToTranslation(_comment, TokenType.Comment);
    }
}
