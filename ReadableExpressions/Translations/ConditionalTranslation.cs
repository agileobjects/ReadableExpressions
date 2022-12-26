namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;

    internal static class ConditionalTranslation
    {
        public static ITranslation For(ConditionalExpression conditional, ITranslationContext context)
        {
            var hasNoElseCondition = HasNoElseCondition(conditional);

            if (hasNoElseCondition)
            {
                return new IfStatementTranslation(conditional, context);
            }

            if (conditional.IsTernary())
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
                   !conditional.HasReturnType();
        }

        public static bool IsTernary(this Expression conditional) => conditional.HasReturnType();

        private abstract class ConditionalTranslationBase :
            ITranslation,
            IPotentialMultiStatementTranslatable
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

            public virtual bool IsMultiStatement => true;

            public abstract int GetIndentSize();

            public abstract int GetLineCount();

            public abstract void WriteTo(TranslationWriter writer);

            protected void WriteIfStatement(TranslationWriter writer)
            {
                writer.WriteControlStatementToTranslation("if ");
                TestTranslation.WriteInParentheses(writer);
                IfTrueTranslation.WriteTo(writer);
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

            public override int GetIndentSize()
                => TestTranslation.GetIndentSize() + IfTrueTranslation.GetIndentSize();

            public override int GetLineCount()
                => TestTranslation.GetLineCount() + IfTrueTranslation.GetLineCount();

            public override void WriteTo(TranslationWriter writer) => WriteIfStatement(writer);
        }

        private class TernaryTranslation : ConditionalTranslationBase
        {
            private readonly ITranslationContext _context;

            public TernaryTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    context.GetCodeBlockTranslationFor(conditional.IfTrue).WithoutStartingNewLine(),
                    context.GetCodeBlockTranslationFor(conditional.IfFalse).WithoutStartingNewLine(),
                    context)
            {
                _context = context;
            }

            public override bool IsMultiStatement => false;

            public override int GetIndentSize()
            {
                return this.ExceedsLengthThreshold()
                    ? GetMultiLineTernaryIndentSize()
                    : GetSingleLineTernaryIndentSize();
            }

            private int GetMultiLineTernaryIndentSize()
            {
                var indentLength = _context.Settings.IndentLength;

                return GetSingleLineTernaryIndentSize() +
                       IfTrueTranslation.GetLineCount() * indentLength +
                       IfFalseTranslation.GetLineCount() * indentLength;
            }

            private int GetSingleLineTernaryIndentSize()
            {
                return TestTranslation.GetLineCount() +
                       IfTrueTranslation.GetIndentSize() +
                       IfFalseTranslation.GetIndentSize();
            }

            public override int GetLineCount()
            {
                return this.ExceedsLengthThreshold()
                    ? GetMultiLineTernaryLineCount()
                    : GetSingleLineTernaryLineCount();
            }

            private int GetMultiLineTernaryLineCount()
            {
                return TestTranslation.GetLineCount() +
                       IfTrueTranslation.GetLineCount() +
                       IfFalseTranslation.GetLineCount();
            }

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

            public override void WriteTo(TranslationWriter writer)
            {
                if (this.ExceedsLengthThreshold())
                {
                    WriteMultiLineTernary(writer);
                    return;
                }

                WriteSingleLineTernary(writer);
            }

            private void WriteMultiLineTernary(TranslationWriter writer)
            {
                TestTranslation.WriteInParenthesesIfRequired(writer, _context);

                writer.WriteNewLineToTranslation();
                writer.Indent();
                writer.WriteToTranslation("? ");

                IfTrueTranslation.WriteTo(writer);

                writer.WriteNewLineToTranslation();
                writer.WriteToTranslation(": ");

                IfFalseTranslation.WriteTo(writer);

                writer.Unindent();
            }

            private void WriteSingleLineTernary(TranslationWriter writer)
            {
                TestTranslation.WriteInParenthesesIfRequired(writer, _context);
                writer.WriteToTranslation(" ? ");
                IfTrueTranslation.WriteTo(writer);
                writer.WriteToTranslation(" : ");
                IfFalseTranslation.WriteTo(writer);
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

            public override int GetIndentSize()
            {
                return TestTranslation.GetIndentSize() +
                       IfTrueTranslation.GetIndentSize() +
                       IfFalseTranslation.GetIndentSize();
            }

            public override int GetLineCount()
            {
                return TestTranslation.GetLineCount() +
                       IfTrueTranslation.GetLineCount() +
                       1 + // for space after the if statement
                       IfFalseTranslation.GetLineCount();
            }

            public override void WriteTo(TranslationWriter writer)
            {
                WriteIfStatement(writer);
                writer.WriteNewLineToTranslation();
                writer.WriteNewLineToTranslation();
                IfFalseTranslation.WriteTo(writer);
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
                if (IsElseIf(conditional))
                {
                    _isElseIf = true;
                    return;
                }

                IfFalseTranslation = GetCodeBlockTranslation(
                    IfFalseTranslation,
                    conditional.IfFalse.IsReturnable(),
                    context);
            }

            private static bool IsElseIf(ConditionalExpression conditional)
                => conditional.IfFalse.NodeType == ExpressionType.Conditional;

            public bool IsTerminated => true;

            public override int GetIndentSize()
            {
                return TestTranslation.GetIndentSize() +
                       IfTrueTranslation.GetIndentSize() +
                       IfFalseTranslation.GetIndentSize();
            }

            public override int GetLineCount()
            {
                return TestTranslation.GetLineCount() +
                       IfTrueTranslation.GetLineCount() +
                       IfFalseTranslation.GetLineCount();
            }

            public override void WriteTo(TranslationWriter writer)
            {
                WriteIfStatement(writer);
                writer.WriteNewLineToTranslation();
                writer.WriteControlStatementToTranslation("else");

                if (_isElseIf)
                {
                    writer.WriteSpaceToTranslation();
                }

                IfFalseTranslation.WriteTo(writer);
            }
        }
    }
}