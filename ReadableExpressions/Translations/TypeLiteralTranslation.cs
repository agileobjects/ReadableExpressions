namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class TypeLiteralTranslation : ITranslation
    {
        private readonly ITranslation _typeNameTranslation;

        public TypeLiteralTranslation(Type type, ITranslationContext context)
        {
            _typeNameTranslation = context.GetTranslationFor(type);
            EstimatedSize = _typeNameTranslation.EstimatedSize + "typeof()".Length;
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public Type Type => typeof(Type);

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteKeywordToTranslation("typeof");
            _typeNameTranslation.WriteInParentheses(buffer);
        }
    }
}