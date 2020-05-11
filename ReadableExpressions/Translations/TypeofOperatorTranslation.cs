namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class TypeofOperatorTranslation : ITranslation
    {
        private readonly ITranslation _typeNameTranslation;

        public TypeofOperatorTranslation(Type type, ITranslationContext context)
        {
            _typeNameTranslation = context.GetTranslationFor(type);
            TranslationSize = _typeNameTranslation.TranslationSize + "typeof()".Length;
            FormattingSize = _typeNameTranslation.FormattingSize + context.GetKeywordFormattingSize();
        }

        public ExpressionType NodeType => ExpressionType.Constant;

        public Type Type => typeof(Type);

        public int TranslationSize { get; }

        public int FormattingSize { get; }

        public int GetLineCount() => _typeNameTranslation.GetLineCount();

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteKeywordToTranslation("typeof");
            _typeNameTranslation.WriteInParentheses(buffer);
        }
    }
}