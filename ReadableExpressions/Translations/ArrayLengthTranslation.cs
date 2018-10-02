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

        private readonly ITranslation _operand;

        public ArrayLengthTranslation(UnaryExpression arrayLength, ITranslationContext context)
        {
            _operand = context.GetTranslationFor(arrayLength.Operand);
            context.Allocate(EstimatedSize = _operand.EstimatedSize + _length.Length);
        }

        public int EstimatedSize { get; }

        public void Translate()
        {
            throw new System.NotImplementedException();
        }
    }
}