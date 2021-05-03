namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using NetStandardPolyfills;

    internal class DefaultValueTranslation : ITranslation, IPotentialEmptyTranslatable
    {
        private const string _null = "null";
        private readonly bool _typeCanBeNull;
        private readonly ITranslatable _operatorTranslation;

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
                allowNullKeyword =
                    (Type.FullName != null) &&
                    !string.IsNullOrEmpty(Type.GetAssembly().GetLocation());
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

            var typeNameTranslation = context.GetTranslationFor(Type);
            _operatorTranslation = new DefaultOperatorTranslation(typeNameTranslation, context.Settings);
            TranslationSize = _operatorTranslation.TranslationSize;
            FormattingSize = _operatorTranslation.FormattingSize;
        }

        public ExpressionType NodeType => ExpressionType.Default;

        public Type Type { get; }

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public bool IsEmpty { get; }

        public int GetIndentSize()
            => _typeCanBeNull ? 0 : _operatorTranslation?.GetIndentSize() ?? 0;

        public int GetLineCount()
            => _typeCanBeNull ? 1 : _operatorTranslation?.GetLineCount() ?? 1;

        public void WriteTo(TranslationWriter writer)
        {
            if (_typeCanBeNull)
            {
                writer.WriteKeywordToTranslation(_null);
            }

            _operatorTranslation?.WriteTo(writer);
        }
    }
}