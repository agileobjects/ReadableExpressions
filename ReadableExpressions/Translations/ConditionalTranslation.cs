namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConditionalTranslation : ITranslation
    {
        private readonly bool _hasNoElseCondition;
        private readonly ITranslation _testTranslation;
        private readonly ITranslation _ifTrueTranslation;
        private readonly ITranslation _ifFalseTranslation;
        private readonly Action<ITranslationContext> _translationWriter;

        public ConditionalTranslation(ConditionalExpression conditional, ITranslationContext context)
        {
            _testTranslation = new ConditionTranslation(conditional.Test, context);
            _hasNoElseCondition = HasNoElseCondition(conditional);
            _ifTrueTranslation = context.GetTranslationFor(conditional.IfTrue);

            if (_hasNoElseCondition)
            {
                _translationWriter = WriteIfStatement;
                goto EstimateSize;
            }

            _ifFalseTranslation = context.GetTranslationFor(conditional.IfFalse);

            if (IsTernary(conditional))
            {
                _translationWriter = WriteTernary;
            }
            else if (conditional.IfTrue.IsReturnable())
            {
                _ifTrueTranslation = new CodeBlockTranslation(_ifTrueTranslation).WithBraces();
                _translationWriter = WriteShortCircuitingIf;
            }

            EstimateSize:
            EstimatedSize = GetEstimatedSize();
        }

        private static bool HasNoElseCondition(ConditionalExpression conditional)
        {
            return (conditional.IfFalse.NodeType == ExpressionType.Default) &&
                   (conditional.Type == typeof(void));
        }

        private static bool IsTernary(Expression conditional) => conditional.Type != typeof(void);

        private int GetEstimatedSize()
        {
            var estimatedSize = _testTranslation.EstimatedSize + _ifTrueTranslation.EstimatedSize;

            if (_hasNoElseCondition == false)
            {
                estimatedSize += _ifFalseTranslation.EstimatedSize;
            }

            // +10 for parentheses, ternary symbols, etc:
            return estimatedSize + 10;
        }

        public ExpressionType NodeType => ExpressionType.Conditional;

        public int EstimatedSize { get; }

        private void WriteIfStatement(ITranslationContext context)
        {
            context.WriteToTranslation("if ");
            _testTranslation.WriteInParentheses(context);
            context.WriteNewLineToTranslation();
            context.WriteToTranslation('{');
            context.WriteNewLineToTranslation();
            context.Indent();
            _ifTrueTranslation.WriteTo(context);
            context.WriteNewLineToTranslation();
            context.Unindent();
            context.WriteToTranslation('}');
        }

        private void WriteTernary(ITranslationContext context)
        {
            _testTranslation.WriteInParenthesesIfRequired(context);
            context.WriteToTranslation(" ? ");
            _ifTrueTranslation.WriteTo(context);
            context.WriteToTranslation(" : ");
            _ifFalseTranslation.WriteTo(context);
        }

        private void WriteShortCircuitingIf(ITranslationContext context)
        {
            context.WriteToTranslation("if ");
            _testTranslation.WriteInParentheses(context);
            _ifTrueTranslation.WriteTo(context);
            context.WriteNewLineToTranslation();
            context.WriteNewLineToTranslation();
            _ifFalseTranslation.WriteTo(context);
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_translationWriter != null)
            {
                _translationWriter.Invoke(context);
                return;
            }
        }
    }
}