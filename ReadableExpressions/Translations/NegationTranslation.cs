using System;

namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class NegationTranslation : ITranslation
    {
        private const char _bang = '!';
        private readonly char _operator;
        private readonly ITranslation _negatedValue;

        public NegationTranslation(UnaryExpression negation, ITranslationContext context)
            : this(
                negation.NodeType,
               (negation.NodeType == ExpressionType.Not) ? _bang : '-',
                context.GetTranslationFor(negation.Operand))
        {
        }

        private NegationTranslation(
            ExpressionType negationType,
            char @operator,
            ITranslation negatedValue)
        {
            NodeType = negationType;
            _operator = @operator;
            _negatedValue = negatedValue;
            EstimatedSize = negatedValue.EstimatedSize + 3;
        }

        public static ITranslation ForNot(ITranslation negatedValue)
            => new NegationTranslation(ExpressionType.Not, _bang, negatedValue);

        public ExpressionType NodeType { get; }

        public Type Type => typeof(bool);

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            context.WriteToTranslation(_operator);
            _negatedValue.WriteInParenthesesIfRequired(context);
        }
    }
}