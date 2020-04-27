namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class FixedValuesTranslation : ITranslation
    {
        private readonly IEnumerable<Action<TranslationBuffer>> _valueWriters;

        public FixedValuesTranslation(
            ExpressionType nodeType,
            Type type,
            int estimatedSize,
            params Action<TranslationBuffer>[] valueWriters)
        {
            NodeType = nodeType;
            Type = type;
            EstimatedSize = estimatedSize;
            _valueWriters = valueWriters;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            foreach (var valueWriter in _valueWriters)
            {
                valueWriter.Invoke(buffer);
            }
        }
    }
}