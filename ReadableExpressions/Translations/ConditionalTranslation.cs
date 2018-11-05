namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

        private static bool IsTernary(Expression conditional) => conditional.Type != typeof(void);

        private abstract class ConditionalTranslationBase : ITranslation
        {
            private int? _estimatedSize;

            protected ConditionalTranslationBase(
                ConditionalExpression conditional,
                ITranslation ifTrueTranslation,
                ITranslationContext context)
            {
                TestTranslation = new ConditionTranslation(conditional.Test, context);
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

            protected ITranslation TestTranslation { get; }

            protected ITranslation IfTrueTranslation { get; }

            protected ITranslation IfFalseTranslation { get; set; }

            public int EstimatedSize
                => _estimatedSize ?? ((_estimatedSize = GetEstimatedSize()).Value);

            private int GetEstimatedSize()
            {
                var estimatedSize = TestTranslation.EstimatedSize + IfTrueTranslation.EstimatedSize;

                if (IfFalseTranslation != null)
                {
                    estimatedSize += IfFalseTranslation.EstimatedSize;
                }

                // +10 for parentheses, ternary symbols, etc:
                return estimatedSize + 10;
            }

            protected static ITranslation GetCodeBlockTranslation(ITranslation translation, bool withReturnKeyword)
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

            public abstract void WriteTo(ITranslationContext context);

            protected void WriteIfStatement(ITranslationContext context)
            {
                context.WriteToTranslation("if ");
                TestTranslation.WriteInParentheses(context);
                IfTrueTranslation.WriteTo(context);
            }
        }

        private class IfStatementTranslation : ConditionalTranslationBase, IPotentialSelfTerminatingTranslatable
        {
            public IfStatementTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    GetCodeBlockTranslation(
                        context.GetTranslationFor(conditional.IfTrue),
                        withReturnKeyword: conditional.IfTrue.IsReturnable()),
                    context)
            {
            }

            public bool IsTerminated => true;

            public override void WriteTo(ITranslationContext context) => WriteIfStatement(context);
        }

        private class TernaryTranslation : ConditionalTranslationBase
        {
            private readonly Action<ITranslationContext> _translationWriter;

            public TernaryTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    context.GetCodeBlockTranslationFor(conditional.IfTrue).WithoutStartingNewLine(),
                    context.GetCodeBlockTranslationFor(conditional.IfFalse).WithoutStartingNewLine(),
                    context)
            {
                if (this.ExceedsLengthThreshold())
                {
                    _translationWriter = WriteMultiLineTernary;
                }
                else
                {
                    _translationWriter = WriteSingleLineTernary;
                }
            }

            public override void WriteTo(ITranslationContext context) => _translationWriter.Invoke(context);

            private void WriteSingleLineTernary(ITranslationContext context)
            {
                TestTranslation.WriteInParenthesesIfRequired(context);
                context.WriteToTranslation(" ? ");
                IfTrueTranslation.WriteTo(context);
                context.WriteToTranslation(" : ");
                IfFalseTranslation.WriteTo(context);
            }

            private void WriteMultiLineTernary(ITranslationContext context)
            {
                TestTranslation.WriteInParenthesesIfRequired(context);

                context.WriteNewLineToTranslation();
                context.Indent();
                context.WriteToTranslation("? ");

                IfTrueTranslation.WriteTo(context);

                context.WriteNewLineToTranslation();
                context.WriteToTranslation(": ");

                IfFalseTranslation.WriteTo(context);

                context.Unindent();
            }
        }

        private class ShortCircuitingIfTranslation : ConditionalTranslationBase
        {
            public ShortCircuitingIfTranslation(ConditionalExpression conditional, ITranslationContext context)
                : base(
                    conditional,
                    GetCodeBlockTranslation(
                        context.GetTranslationFor(conditional.IfTrue),
                        withReturnKeyword: true),
                    context.GetCodeBlockTranslationFor(conditional.IfFalse).WithoutBraces(),
                    context)
            {
            }

            public override void WriteTo(ITranslationContext context)
            {
                WriteIfStatement(context);
                context.WriteNewLineToTranslation();
                context.WriteNewLineToTranslation();
                IfFalseTranslation.WriteTo(context);
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
                        withReturnKeyword: false),
                    context.GetTranslationFor(conditional.IfFalse),
                    context)
            {
                _isElseIf = IsElseIf(conditional);

                if (_isElseIf == false)
                {
                    IfFalseTranslation = GetCodeBlockTranslation(IfFalseTranslation, conditional.IfFalse.IsReturnable());
                }
            }

            private static bool IsElseIf(ConditionalExpression conditional)
                => conditional.IfFalse.NodeType == ExpressionType.Conditional;

            public bool IsTerminated => true;

            public override void WriteTo(ITranslationContext context)
            {
                WriteIfStatement(context);
                context.WriteNewLineToTranslation();
                context.WriteToTranslation("else");

                if (_isElseIf)
                {
                    context.WriteSpaceToTranslation();
                }

                IfFalseTranslation.WriteTo(context);
            }
        }
    }
}