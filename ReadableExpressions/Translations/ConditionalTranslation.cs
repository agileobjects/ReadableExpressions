namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Interfaces;

    internal static class ConditionalTranslation
    {
        public static ITranslation For(ConditionalExpression conditional, ITranslationContext context)
        {
            var hasNoElseCondition = HasNoElseCondition(conditional);

            if (hasNoElseCondition)
            {
                return new IfStatementTranslation(conditional, context);
            }

            if (IsTernary(conditional))
            {
                return new TernaryTranslation(conditional, context);
            }

            if (conditional.IfTrue.IsReturnable())
            {
                return new ShortCircuitingIfTranslation(conditional, context);
            }

            return new IfElseTranslation(conditional, context);
        }

        private static bool HasNoElseCondition(ConditionalExpression conditional)
        {
            return (conditional.IfFalse.NodeType == ExpressionType.Default) &&
                   (conditional.Type == typeof(void));
        }

        public static bool IsTernary(Expression conditional) => conditional.Type != typeof(void);

        private abstract class ConditionalTranslationBase : ITranslation
        {
            private int? _translationSize;

            protected ConditionalTranslationBase(
                ConditionalExpression conditional,
                ITranslation ifTrueTranslation,
                ITranslationContext context)
            {
                Type = conditional.Type;
                TestTranslation = ConditionTranslation.For(conditional.Test, context);
                IfTrueTranslation = ifTrueTranslation;
            }

            protected ConditionalTranslationBase(
                ConditionalExpression conditional,
                ITranslation ifTrueTranslation,
                ITranslation ifFalseTranslation,
                ITranslationContext context)
                : this(conditional, ifTrueTranslation, context)
            {
                IfFalseTranslation = ifFalseTranslation;
            }

            public ExpressionType NodeType => ExpressionType.Conditional;

            public Type Type { get; }

            protected ITranslation TestTranslation { get; }

            protected ITranslation IfTrueTranslation { get; }

            protected ITranslation IfFalseTranslation { get; set; }

            public int TranslationSize => _translationSize ??= GetTranslationSize();

            private int GetTranslationSize()
            {
                var translationSize = TestTranslation.TranslationSize + IfTrueTranslation.TranslationSize;

                if (IfFalseTranslation != null)
                {
                    translationSize += IfFalseTranslation.TranslationSize;
                }

                // +10 for parentheses, ternary symbols, etc:
                return translationSize + 10;
            }

            public int FormattingSize => GetFormattingSize();

            private int GetFormattingSize()
            {
                var formattingSize = TestTranslation.FormattingSize + IfTrueTranslation.FormattingSize;

                if (IfFalseTranslation != null)
                {
                    formattingSize += IfFalseTranslation.FormattingSize;
                }

                return formattingSize;
            }

            protected static ITranslation GetCodeBlockTranslation(
                ITranslation translation,
                bool withReturnKeyword,
                ITranslationContext context)
            {
                var codeBlockTranslation = new CodeBlockTranslation(translation, context)
                    .WithTermination()
                    .WithBraces();

                if (withReturnKeyword)
                {
                    codeBlockTranslation = codeBlockTranslation.WithReturnKeyword();
                }

                return codeBlockTranslation;
            }

            public abstract int GetLineCount();

            public abstract void WriteTo(TranslationBuffer buffer);

            protected void WriteIfStatement(TranslationBuffer buffer)
            {
                buffer.WriteControlStatementToTranslation("if ");
                TestTranslation.WriteInParentheses(buffer);
                IfTrueTranslation.WriteTo(buffer);
            }
        }

        private class IfStatementTranslation : ConditionalTranslationBase, IPotentialSelfTerminatingTranslatable
        {
            public IfStatementTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    GetCodeBlockTranslation(
                        context.GetTranslationFor(conditional.IfTrue),
                        withReturnKeyword: conditional.IfTrue.IsReturnable(),
                        context),
                    context)
            {
            }

            public bool IsTerminated => true;

            public override int GetLineCount()
                => TestTranslation.GetLineCount() + IfTrueTranslation.GetLineCount();

            public override void WriteTo(TranslationBuffer buffer) => WriteIfStatement(buffer);
        }

        private class TernaryTranslation : ConditionalTranslationBase
        {
            private readonly ITranslationContext _context;
            private readonly Action<TranslationBuffer, ITranslationContext> _translationWriter;
            private readonly Func<int> _lineCounter;

            public TernaryTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    context.GetCodeBlockTranslationFor(conditional.IfTrue).WithoutStartingNewLine(),
                    context.GetCodeBlockTranslationFor(conditional.IfFalse).WithoutStartingNewLine(),
                    context)
            {
                _context = context;

                if (this.ExceedsLengthThreshold())
                {
                    _translationWriter = WriteMultiLineTernary;
                    _lineCounter = GetSingleLineTernaryLineCount;
                }
                else
                {
                    _translationWriter = WriteSingleLineTernary;
                    _lineCounter = GetMultiLineTernaryLineCount;
                }
            }

            public override int GetLineCount() => _lineCounter.Invoke();

            private int GetSingleLineTernaryLineCount()
            {
                var lineCount = TestTranslation.GetLineCount();

                var ifTrueLineCount = IfTrueTranslation.GetLineCount();

                if (ifTrueLineCount > 1)
                {
                    lineCount += ifTrueLineCount - 1;
                }

                var ifFalseLineCount = IfFalseTranslation.GetLineCount();

                if (ifFalseLineCount > 1)
                {
                    lineCount += ifFalseLineCount - 1;
                }

                return lineCount;
            }

            private int GetMultiLineTernaryLineCount()
            {
                return
                    TestTranslation.GetLineCount() +
                    IfTrueTranslation.GetLineCount() +
                    IfFalseTranslation.GetLineCount();
            }

            public override void WriteTo(TranslationBuffer buffer)
                => _translationWriter.Invoke(buffer, _context);

            private void WriteSingleLineTernary(TranslationBuffer buffer, ITranslationContext context)
            {
                TestTranslation.WriteInParenthesesIfRequired(buffer, context);
                buffer.WriteToTranslation(" ? ");
                IfTrueTranslation.WriteTo(buffer);
                buffer.WriteToTranslation(" : ");
                IfFalseTranslation.WriteTo(buffer);
            }

            private void WriteMultiLineTernary(TranslationBuffer buffer, ITranslationContext context)
            {
                TestTranslation.WriteInParenthesesIfRequired(buffer, context);

                buffer.WriteNewLineToTranslation();
                buffer.Indent();
                buffer.WriteToTranslation("? ");

                IfTrueTranslation.WriteTo(buffer);

                buffer.WriteNewLineToTranslation();
                buffer.WriteToTranslation(": ");

                IfFalseTranslation.WriteTo(buffer);

                buffer.Unindent();
            }
        }

        private class ShortCircuitingIfTranslation : ConditionalTranslationBase
        {
            public ShortCircuitingIfTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    GetCodeBlockTranslation(
                        context.GetTranslationFor(conditional.IfTrue),
                        withReturnKeyword: true,
                        context),
                    context.GetCodeBlockTranslationFor(conditional.IfFalse).WithoutBraces(),
                    context)
            {
            }

            public override int GetLineCount()
            {
                return
                    TestTranslation.GetLineCount() +
                    IfTrueTranslation.GetLineCount() + 
                    1 + // for space after the if statement
                    IfFalseTranslation.GetLineCount();
            }

            public override void WriteTo(TranslationBuffer buffer)
            {
                WriteIfStatement(buffer);
                buffer.WriteNewLineToTranslation();
                buffer.WriteNewLineToTranslation();
                IfFalseTranslation.WriteTo(buffer);
            }
        }

        private class IfElseTranslation : ConditionalTranslationBase, IPotentialSelfTerminatingTranslatable
        {
            private readonly bool _isElseIf;

            public IfElseTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    GetCodeBlockTranslation(
                        context.GetTranslationFor(conditional.IfTrue),
                        withReturnKeyword: false,
                        context),
                    context.GetTranslationFor(conditional.IfFalse),
                    context)
            {
                _isElseIf = IsElseIf(conditional);

                if (_isElseIf == false)
                {
                    IfFalseTranslation = GetCodeBlockTranslation(
                        IfFalseTranslation,
                        conditional.IfFalse.IsReturnable(),
                        context);
                }
            }

            private static bool IsElseIf(ConditionalExpression conditional)
                => conditional.IfFalse.NodeType == ExpressionType.Conditional;

            public bool IsTerminated => true;

            public override int GetLineCount()
            {
                return
                    TestTranslation.GetLineCount() +
                    IfTrueTranslation.GetLineCount() + 
                    IfFalseTranslation.GetLineCount(); 
            }

            public override void WriteTo(TranslationBuffer buffer)
            {
                WriteIfStatement(buffer);
                buffer.WriteNewLineToTranslation();
                buffer.WriteControlStatementToTranslation("else");

                if (_isElseIf)
                {
                    buffer.WriteSpaceToTranslation();
                }

                IfFalseTranslation.WriteTo(buffer);
            }
        }
    }
}