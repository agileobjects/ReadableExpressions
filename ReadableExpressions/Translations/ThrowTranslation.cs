namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
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
            Type = throwExpression.Type;
            TranslationSize = _throw.Length;
            FormattingSize = context.GetKeywordFormattingSize();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // unary.Operand is null when using Expression.Rethrow():
            if ((throwExpression.Operand == null) || context.IsCatchBlockVariable(throwExpression.Operand))
            {
                return;
            }

            _thrownItemTranslation = context.GetTranslationFor(throwExpression.Operand);
            TranslationSize = _thrownItemTranslation.TranslationSize;
            FormattingSize += _thrownItemTranslation.FormattingSize;
        }

        public ExpressionType NodeType => ExpressionType.Throw;

        public Type Type { get; }

        public int TranslationSize { get; }
        
        public int FormattingSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteKeywordToTranslation(_throw);

            if (_thrownItemTranslation == null)
            {
                return;
            }

            buffer.WriteSpaceToTranslation();
            _thrownItemTranslation.WriteTo(buffer);
        }
    }
}