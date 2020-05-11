namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class ArrayLengthTranslation : ITranslation
    {
        private const string _length = ".Length";
        private static readonly int _lengthPropertyLength = _length.Length;

        private readonly ITranslation _operand;

        public ArrayLengthTranslation(UnaryExpression arrayLength, ITranslationContext context)
        {
            _operand = context.GetTranslationFor(arrayLength.Operand);
            TranslationSize = _operand.TranslationSize + _lengthPropertyLength;
        }

        public ExpressionType NodeType => ExpressionType.ArrayLength;

        public Type Type => typeof(int);

        public int TranslationSize { get; }

        public int FormattingSize => _operand.FormattingSize;

        public int GetLineCount() => _operand.GetLineCount();

        public void WriteTo(TranslationBuffer buffer)
        {
            _operand.WriteTo(buffer);
            buffer.WriteToTranslation(_length);
        }
    }
}