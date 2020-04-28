namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class NegationTranslation : ITranslation
    {
        private const char _bang = '!';
        private readonly ITranslationContext _context;
        private readonly char _operator;
        private readonly ITranslation _negatedValue;

        public NegationTranslation(UnaryExpression negation, ITranslationContext context)
            : this(
                negation.NodeType,
               (negation.NodeType == ExpressionType.Not) ? _bang : '-',
                context.GetTranslationFor(negation.Operand),
                context)
        {
        }

        private NegationTranslation(
            ExpressionType negationType,
            char @operator,
            ITranslation negatedValue,
            ITranslationContext context)
        {
            _context = context;
            NodeType = negationType;
            _operator = @operator;
            _negatedValue = negatedValue;
            TranslationSize = negatedValue.TranslationSize + 3;
        }

        public static ITranslation ForNot(ITranslation negatedValue, ITranslationContext context)
            => new NegationTranslation(ExpressionType.Not, _bang, negatedValue, context);

        public ExpressionType NodeType { get; }

        public Type Type => typeof(bool);

        public int TranslationSize { get; }

        public int FormattingSize => _negatedValue.FormattingSize;

        public void WriteTo(TranslationBuffer buffer)
        {
            buffer.WriteToTranslation(_operator);
            _negatedValue.WriteInParenthesesIfRequired(buffer, _context);
        }
    }
}