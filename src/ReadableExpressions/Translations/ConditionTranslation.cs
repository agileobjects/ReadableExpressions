namespace AgileObjects.ReadableExpressions.Translations;

internal static class ConditionTranslation
{
    public static INodeTranslation For(
        Expression condition,
        ITranslationContext context)
    {
        var conditionTranslation = context.GetTranslationFor(condition);

        if (IsMultiLineBinary(condition, conditionTranslation))
        {
            return new MultiLineBinaryConditionTranslation(
                (BinaryExpression)condition,
                conditionTranslation,
                context);
        }

        var conditionCodeBlockTranslation =
            new CodeBlockTranslation(conditionTranslation, context);

        return conditionTranslation.IsMultiStatement()
            ? conditionCodeBlockTranslation.WithSingleCodeBlockParameterFormatting()
            : conditionCodeBlockTranslation;
    }

    private static bool IsMultiLineBinary(
        Expression condition,
        ITranslation conditionTranslation)
    {
        return IsRelevantBinary(condition) && conditionTranslation.WrapLine();
    }

    private static bool IsRelevantBinary(Expression condition)
    {
        return condition.NodeType switch
        {
            ExpressionType.And => true,
            ExpressionType.AndAlso => true,
            ExpressionType.Or => true,
            ExpressionType.OrElse => true,
            _ => false
        };
    }

    private class MultiLineBinaryConditionTranslation : INodeTranslation
    {
        private readonly ITranslation _conditionTranslation;
        private readonly ITranslationContext _context;
        private readonly INodeTranslation _binaryConditionLeftTranslation;
        private readonly string _binaryConditionOperator;
        private readonly INodeTranslation _binaryConditionRightTranslation;

        public MultiLineBinaryConditionTranslation(
            BinaryExpression binaryCondition,
            ITranslation conditionTranslation,
            ITranslationContext context)
        {
            _conditionTranslation = conditionTranslation;
            _context = context;
            NodeType = binaryCondition.NodeType;
            _binaryConditionLeftTranslation = For(binaryCondition.Left, context);
            _binaryConditionOperator = BinaryTranslation.GetOperator(binaryCondition);
            _binaryConditionRightTranslation = For(binaryCondition.Right, context);
        }

        public ExpressionType NodeType { get; }

        public int TranslationLength => _conditionTranslation.TranslationLength;

        public void WriteTo(TranslationWriter writer)
        {
            _binaryConditionLeftTranslation.WriteInParenthesesIfRequired(writer, _context);
            writer.WriteToTranslation(_binaryConditionOperator.TrimEnd());
            writer.WriteNewLineToTranslation();
            writer.Indent();
            _binaryConditionRightTranslation.WriteInParenthesesIfRequired(writer, _context);
            writer.Unindent();
        }
    }
}