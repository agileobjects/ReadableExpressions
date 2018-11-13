namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class ThrowTranslation : ITranslation
    {
        private const string _throw = "throw";
        private readonly ITranslation _thrownItemTranslation;

        public ThrowTranslation(UnaryExpression throwExpression, ITranslationContext context)
        {
            // ReSharper disable HeuristicUnreachableCode
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // unary.Operand is null when using Expression.Rethrow():
            if (throwExpression.Operand == null)
            {
                EstimatedSize = _throw.Length;
                return;
            }

            _thrownItemTranslation = context.GetTranslationFor(throwExpression.Operand);
            // ReSharper restore HeuristicUnreachableCode
        }

        public ExpressionType NodeType => ExpressionType.Throw;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation(_throw);

            if (_thrownItemTranslation != null)
            {
                context.WriteSpaceToTranslation();
                _thrownItemTranslation.WriteTo(context);
            }
        }
    }
}