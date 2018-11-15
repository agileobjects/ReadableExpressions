using System;

namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class ArrayLengthTranslation : ITranslation
    {
        private const string _length = ".Length";

        private readonly ITranslation _operand;

        public ArrayLengthTranslation(UnaryExpression arrayLength, ITranslationContext context)
        {
            _operand = context.GetTranslationFor(arrayLength.Operand);
            EstimatedSize = _operand.EstimatedSize + LengthPropertyLength;
        }

        public ExpressionType NodeType => ExpressionType.ArrayLength;

        public Type Type => typeof(int);

        public int EstimatedSize { get; }

        private static int LengthPropertyLength => _length.Length;

        public void WriteTo(ITranslationContext context)
        {
            _operand.WriteTo(context);
            context.WriteToTranslation(_length);
        }
    }
}