namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
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

        public IndexAccessTranslation(ITranslation subject, ParameterSetTranslation parameters, Type indexValueType)
        {
            NodeType = ExpressionType.Call;
            Type = indexValueType;
            _subject = subject;
            _parameters = parameters;
            EstimatedSize = GetEstimatedSize();
        }

        public IndexAccessTranslation(IndexExpression indexAccess, ITranslationContext context)
            : this(indexAccess.Object, indexAccess.Arguments, context)
        {
            NodeType = ExpressionType.Index;
            Type = indexAccess.Type;
        }

        public IndexAccessTranslation(BinaryExpression arrayIndexAccess, ITranslationContext context)
            : this(arrayIndexAccess.Left, new[] { arrayIndexAccess.Right }, context)
        {
            NodeType = ExpressionType.ArrayIndex;
            Type = arrayIndexAccess.Type;
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

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            _subject.WriteTo(buffer);
            buffer.WriteToTranslation('[');
            _parameters.WithoutParentheses().WriteTo(buffer);
            buffer.WriteToTranslation(']');
        }
    }
}