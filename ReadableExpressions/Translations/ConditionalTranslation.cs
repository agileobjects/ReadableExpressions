namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConditionalTranslation : ITranslation, IPotentialSelfTerminatingTranslatable
    {
        private readonly bool _hasNoElseCondition;
        private readonly ITranslation _testTranslation;
        private readonly ITranslation _ifTrueTranslation;
        private readonly ITranslation _ifFalseTranslation;
        private readonly Action<ITranslationContext> _translationWriter;
        private readonly bool _isElseIf;

        public ConditionalTranslation(ConditionalExpression conditional, ITranslationContext context)
        {
            _testTranslation = new ConditionTranslation(conditional.Test, context);
            _hasNoElseCondition = HasNoElseCondition(conditional);
            _ifTrueTranslation = context.GetTranslationFor(conditional.IfTrue);

            if (_hasNoElseCondition)
            {
                _ifTrueTranslation = GetIfTrueCodeBlockTranslation(conditional);
                _translationWriter = WriteIfStatement;
                IsTerminated = true;
                goto EstimateSize;
            }

            _ifFalseTranslation = context.GetTranslationFor(conditional.IfFalse);

            if (IsTernary(conditional))
            {
                _translationWriter = WriteTernary;
                goto EstimateSize;
            }

            if (conditional.IfTrue.IsReturnable())
            {
                _ifTrueTranslation = GetIfTrueCodeBlockTranslation(withReturnKeyword: true);
                _translationWriter = WriteShortCircuitingIf;
                goto EstimateSize;
            }

            _ifTrueTranslation = GetIfTrueCodeBlockTranslation(withReturnKeyword: false);
            _isElseIf = IsElseIf(conditional);

            if (_isElseIf == false)
            {
                _ifFalseTranslation = GetCodeBlockTranslation(_ifFalseTranslation, conditional.IfFalse.IsReturnable());
            }

            EstimateSize:
            EstimatedSize = GetEstimatedSize();
        }

        private static bool HasNoElseCondition(ConditionalExpression conditional)
        {
            return (conditional.IfFalse.NodeType == ExpressionType.Default) &&
                   (conditional.Type == typeof(void));
        }

        private ITranslation GetIfTrueCodeBlockTranslation(ConditionalExpression conditional)
            => GetIfTrueCodeBlockTranslation(conditional.IfTrue.IsReturnable());

        private ITranslation GetIfTrueCodeBlockTranslation(bool withReturnKeyword)
            => GetCodeBlockTranslation(_ifTrueTranslation, withReturnKeyword);

        private static ITranslation GetCodeBlockTranslation(ITranslation translation, bool withReturnKeyword)
        {
            var codeBlockTranslation = new CodeBlockTranslation(translation)
                .WithTermination()
                .WithBraces();

            if (withReturnKeyword)
            {
                codeBlockTranslation = codeBlockTranslation.WithReturnKeyword();
            }

            return codeBlockTranslation;
        }

        private static bool IsTernary(Expression conditional) => conditional.Type != typeof(void);

        private static bool IsElseIf(ConditionalExpression conditional)
            => conditional.IfFalse.NodeType == ExpressionType.Conditional;

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

        public bool IsTerminated { get; }

        private void WriteIfStatement(ITranslationContext context)
        {
            context.WriteToTranslation("if ");
            _testTranslation.WriteInParentheses(context);
            _ifTrueTranslation.WriteTo(context);
        }

        private void WriteTernary(ITranslationContext context)
        {
            var writeToMultipleLines = this.ExceedsLengthThreshold();

            _testTranslation.WriteInParenthesesIfRequired(context);

            if (writeToMultipleLines)
            {
                context.WriteNewLineToTranslation();
                context.Indent();
            }

            context.WriteToTranslation(" ? ");
            _ifTrueTranslation.WriteTo(context);

            if (writeToMultipleLines)
            {
                context.WriteNewLineToTranslation();
            }

            context.WriteToTranslation(" : ");
            _ifFalseTranslation.WriteTo(context);

            if (writeToMultipleLines)
            {
                context.Unindent();
            }
        }

        private void WriteShortCircuitingIf(ITranslationContext context)
        {
            WriteIfStatement(context);
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

            WriteIfStatement(context);
            context.WriteNewLineToTranslation();
            context.WriteToTranslation("else");

            if (_isElseIf)
            {
                context.WriteSpaceToTranslation();
            }

            _ifFalseTranslation.WriteTo(context);
        }
    }
}