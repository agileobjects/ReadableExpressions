namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class ModifiedTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly ITranslatable _baseTranslatable;

        public ModifiedTranslation(ITranslation baseTranslation, ExpressionType nodeType)
            : this(baseTranslation, nodeType, baseTranslation.Type)
        {
        }

        public ModifiedTranslation(
            ITranslatable baseTranslatable,
            ExpressionType nodeType,
            Type type)
        {
            _baseTranslatable = baseTranslatable;
            NodeType = nodeType;
            Type = type;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize => _baseTranslatable.EstimatedSize;

        public bool IsTerminated => _baseTranslatable.IsTerminated();

        public void WriteTo(TranslationBuffer buffer) => _baseTranslatable.WriteTo(buffer);
    }
}