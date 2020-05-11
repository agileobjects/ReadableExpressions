namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal class DefaultValueTranslation : ITranslation, IPotentialEmptyTranslatable
    {
        private const string _default = "default";
        private const string _null = "null";
        private readonly bool _typeCanBeNull;
        private readonly ITranslation _typeNameTranslation;

        public DefaultValueTranslation(
            Expression defaultExpression,
            ITranslationContext context,
            bool allowNullKeyword = true)
        {
            Type = defaultExpression.Type;

            if (Type == typeof(void))
            {
                IsEmpty = true;
                return;
            }

            if (allowNullKeyword)
            {
                _typeCanBeNull = Type.CanBeNull();

                if (_typeCanBeNull)
                {
                    TranslationSize = _null.Length;
                    return;
                }
            }

            _typeNameTranslation = context.GetTranslationFor(Type);
            TranslationSize = _default.Length + _typeNameTranslation.TranslationSize + "()".Length;
            FormattingSize = context.GetKeywordFormattingSize() + _typeNameTranslation.FormattingSize;
        }

        public ExpressionType NodeType => ExpressionType.Default;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsEmpty { get; }

        public int GetLineCount()
            => _typeCanBeNull ? 1 : _typeNameTranslation?.GetLineCount() ?? 1;

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_typeCanBeNull)
            {
                buffer.WriteKeywordToTranslation(_null);
            }

            if (_typeNameTranslation == null)
            {
                // Translation of default(void):
                return;
            }

            buffer.WriteKeywordToTranslation(_default);
            _typeNameTranslation.WriteInParentheses(buffer);
        }
    }
}