namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ArrayLengthTranslation : ITranslation
    {
        private const string _length = ".Length";

        private readonly ITranslationContext _context;
        private readonly ITranslation _operand;

        public ArrayLengthTranslation(UnaryExpression arrayLength, ITranslationContext context)
        {
            _context = context;
            _operand = context.GetTranslationFor(arrayLength.Operand);
            EstimatedSize = _operand.EstimatedSize + LengthPropertyLength;
        }

        public int EstimatedSize { get; }

        private static int LengthPropertyLength => _length.Length;

        public void WriteToTranslation()
        {
            _operand.WriteToTranslation();
            _context.WriteToTranslation(_length);
        }
    }
}