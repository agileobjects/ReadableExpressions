namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
    using Interfaces;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class IndexAccessTranslation : ITranslation
    {
        private readonly ITranslation _subject;
        private readonly ParameterSetTranslation _parameters;

        public IndexAccessTranslation(ITranslation subject, ParameterSetTranslation parameters)
        {
            NodeType = ExpressionType.Call;
            _subject = subject;
            _parameters = parameters;
            EstimatedSize = GetEstimatedSize();
        }

        public IndexAccessTranslation(IndexExpression indexAccess, ITranslationContext context)
            : this(indexAccess.Object, indexAccess.Arguments, context)
        {
            NodeType = ExpressionType.Index;
        }

        public IndexAccessTranslation(BinaryExpression arrayIndexAccess, ITranslationContext context)
            : this(arrayIndexAccess.Left, new[] { arrayIndexAccess.Right }, context)
        {
            NodeType = ExpressionType.ArrayIndex;
        }

        private IndexAccessTranslation(
            Expression subject,
            ICollection<Expression> arguments,
            ITranslationContext context)
        {
            _subject = context.GetTranslationFor(subject);
            _parameters = new ParameterSetTranslation(arguments, context);
            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize() => _subject.EstimatedSize + _parameters.EstimatedSize + 2;

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _subject.WriteTo(context);
            context.WriteToTranslation('[');
            _parameters.WithoutParentheses().WriteTo(context);
            context.WriteToTranslation(']');
        }
    }
}