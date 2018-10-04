namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class BinaryTranslation : ITranslation
    {
        private readonly ITranslation _leftOperand;
        private readonly ITranslation _rightOperand;

        public BinaryTranslation(BinaryExpression binary, ITranslationContext context)
        {
            _leftOperand = context.GetTranslationFor(binary.Left);
            _rightOperand = context.GetTranslationFor(binary.Right);
        }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            _leftOperand.WriteTo(context);
            _rightOperand.WriteTo(context);
        }
    }
}