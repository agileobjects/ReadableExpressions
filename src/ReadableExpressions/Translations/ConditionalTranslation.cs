namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;

internal static class ConditionalTranslation
{
    public static INodeTranslation For(
        ConditionalExpression conditional,
        ITranslationContext context)
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
        return conditional.IfFalse.NodeType == ExpressionType.Default &&
              !conditional.HasReturnType();
    }

    public static bool IsTernary(this Expression conditional) => 
        conditional.HasReturnType();

    private abstract class ConditionalTranslationBase :
        INodeTranslation,
        IPotentialMultiStatementTranslatable
    {
        protected ConditionalTranslationBase(
            ConditionalExpression conditional,
            INodeTranslation ifTrueTranslation,
            ITranslationContext context)
        {
            TestTranslation = ConditionTranslation.For(conditional.Test, context);
            IfTrueTranslation = ifTrueTranslation;
        }

        protected ConditionalTranslationBase(
            ConditionalExpression conditional,
            INodeTranslation ifTrueTranslation,
            INodeTranslation ifFalseTranslation,
            ITranslationContext context)
            : this(conditional, ifTrueTranslation, context)
        {
            IfFalseTranslation = ifFalseTranslation;
        }

        public ExpressionType NodeType => ExpressionType.Conditional;

        protected INodeTranslation TestTranslation { get; }

        protected INodeTranslation IfTrueTranslation { get; }

        protected INodeTranslation IfFalseTranslation { get; set; }

        public int TranslationLength
        {
            get
            {
                var translationLength =
                    TestTranslation.TranslationLength +
                    IfTrueTranslation.TranslationLength;

                if (IfFalseTranslation != null)
                {
                    translationLength += IfFalseTranslation.TranslationLength;
                }

                // +10 for parentheses, ternary symbols, etc:
                return translationLength + 10;
            }
        }

        protected static INodeTranslation GetCodeBlockTranslation(
            INodeTranslation translation,
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

        public abstract void WriteTo(TranslationWriter writer);

        protected void WriteIfStatement(TranslationWriter writer)
        {
            writer.WriteControlStatementToTranslation("if ");
            TestTranslation.WriteInParentheses(writer);
            IfTrueTranslation.WriteTo(writer);
        }
    }

    private class IfStatementTranslation :
        ConditionalTranslationBase,
        IPotentialSelfTerminatingTranslation
    {
        public IfStatementTranslation(
            ConditionalExpression conditional,
            ITranslationContext context) :
            base(
                conditional,
                GetCodeBlockTranslation(
                    context.GetTranslationFor(conditional.IfTrue),
                    withReturnKeyword: conditional.IfTrue.IsReturnable(),
                    context),
                context)
        {
        }

        public bool IsTerminated => true;

        public override void WriteTo(TranslationWriter writer) => 
            WriteIfStatement(writer);
    }

    private class TernaryTranslation : ConditionalTranslationBase
    {
        private readonly ITranslationContext _context;

        public TernaryTranslation(
            ConditionalExpression conditional,
            ITranslationContext context) :
            base(
                conditional,
                context
                    .GetCodeBlockTranslationFor(conditional.IfTrue)
                    .WithoutStartingNewLine(),
                context
                    .GetCodeBlockTranslationFor(conditional.IfFalse)
                    .WithoutStartingNewLine(),
                context)
        {
            _context = context;
        }

        public override bool IsMultiStatement => false;

        public override void WriteTo(TranslationWriter writer)
        {
            if (this.WrapLine())
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
        public ShortCircuitingIfTranslation(
            ConditionalExpression conditional,
            ITranslationContext context) :
            base(
                conditional,
                GetCodeBlockTranslation(
                    context.GetTranslationFor(conditional.IfTrue),
                    withReturnKeyword: true,
                    context),
                context
                    .GetCodeBlockTranslationFor(conditional.IfFalse)
                    .WithoutBraces(),
                context)
        {
        }

        public override void WriteTo(TranslationWriter writer)
        {
            WriteIfStatement(writer);
            writer.WriteNewLineToTranslation();
            writer.WriteNewLineToTranslation();
            IfFalseTranslation.WriteTo(writer);
        }
    }

    private class IfElseTranslation :
        ConditionalTranslationBase,
        IPotentialSelfTerminatingTranslation
    {
        private readonly bool _isElseIf;

        public IfElseTranslation(
            ConditionalExpression conditional,
            ITranslationContext context) :
            base(
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

        private static bool IsElseIf(ConditionalExpression conditional) => 
            conditional.IfFalse.NodeType == ExpressionType.Conditional;

        public bool IsTerminated => true;

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