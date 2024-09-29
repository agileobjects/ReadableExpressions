namespace AgileObjects.ReadableExpressions.Translations;

using Extensions;

internal static class GotoTranslation
{
    public static INodeTranslation For(
        GotoExpression @goto,
        ITranslationContext context)
    {
        switch (@goto.Kind)
        {
            case GotoExpressionKind.Break:
                return new TerminatedGotoTranslation("break");

            case GotoExpressionKind.Continue:
                return new TerminatedGotoTranslation("continue");

            case GotoExpressionKind.Return:
                if (@goto.Value == null)
                {
                    return new TerminatedGotoTranslation("return");
                }

                return new ReturnValueTranslation(@goto, context);

            case GotoExpressionKind.Goto when context.Analysis.GoesToReturnLabel(@goto):
                goto case GotoExpressionKind.Return;

            default:
                return new GotoNamedLabelTranslation(@goto);
        }
    }

    private class TerminatedGotoTranslation :
        INodeTranslation,
        IPotentialSelfTerminatingTranslation
    {
        private readonly string _statement;

        public TerminatedGotoTranslation(string statement)
        {
            _statement = statement;
        }

        public ExpressionType NodeType => ExpressionType.Goto;

        public int TranslationLength => _statement.Length + ";".Length;

        public bool IsTerminated => true;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteControlStatementToTranslation(_statement);
            writer.WriteSemiColonToTranslation();
        }
    }

    private class GotoNamedLabelTranslation :
        INodeTranslation,
        IPotentialSelfTerminatingTranslation
    {
        private readonly string _labelName;

        public GotoNamedLabelTranslation(GotoExpression @goto)
        {
            _labelName = @goto.Target.Name;
        }

        public ExpressionType NodeType => ExpressionType.Goto;

        public int TranslationLength => 
            "goto ".Length + _labelName.Length + ";".Length;

        public bool IsTerminated => true;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteControlStatementToTranslation("goto ");
            writer.WriteToTranslation(_labelName);
            writer.WriteSemiColonToTranslation();
        }
    }

    private class ReturnValueTranslation :
        INodeTranslation,
        IPotentialGotoTranslation
    {
        private readonly INodeTranslation _returnValueTranslation;

        public ReturnValueTranslation(
            GotoExpression @goto,
            ITranslationContext context)
        {
            _returnValueTranslation = context.GetCodeBlockTranslationFor(@goto.Value);
        }

        public ExpressionType NodeType => ExpressionType.Goto;

        public int TranslationLength => 
            _returnValueTranslation.TranslationLength + "return ".Length;

        public bool HasGoto => true;

        public void WriteTo(TranslationWriter writer)
        {
            writer.WriteReturnToTranslation();
            _returnValueTranslation.WriteTo(writer);
        }
    }
}