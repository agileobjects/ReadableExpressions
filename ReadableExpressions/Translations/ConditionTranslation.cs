namespace AgileObjects.ReadableExpressions.Translations
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class ConditionTranslation
    {
        public static ITranslation For(Expression condition, ITranslationContext context)
        {
            var conditionTranslation = context.GetTranslationFor(condition);

            if (SplitBinaryConditionToMultipleLines(condition, conditionTranslation))
            {
                return new MultiLineBinaryConditionTranslation((BinaryExpression)condition, conditionTranslation, context);
            }

            var conditionCodeBlockTranslation = new CodeBlockTranslation(conditionTranslation);

            return conditionTranslation.IsMultiStatement()
                ? conditionCodeBlockTranslation.WithSingleLamdaParameterFormatting()
                : conditionCodeBlockTranslation;
        }

        private static bool SplitBinaryConditionToMultipleLines(Expression condition, ITranslatable conditionTranslation)
            => conditionTranslation.ExceedsLengthThreshold() && IsRelevantBinary(condition);

        private static bool IsRelevantBinary(Expression condition)
        {
            switch (condition.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return true;
            }

            return false;
        }

        private class MultiLineBinaryConditionTranslation : ITranslation
        {
            private readonly ITranslation _binaryConditionLeftTranslation;
            private readonly string _binaryConditionOperator;
            private readonly ITranslation _binaryConditionRightTranslation;

            public MultiLineBinaryConditionTranslation(
                BinaryExpression binaryCondition,
                ITranslation conditionTranslation,
                ITranslationContext context)
            {
                NodeType = binaryCondition.NodeType;
                _binaryConditionLeftTranslation = For(binaryCondition.Left, context);
                _binaryConditionOperator = BinaryTranslation.GetOperator(binaryCondition);
                _binaryConditionRightTranslation = For(binaryCondition.Right, context);
                EstimatedSize = conditionTranslation.EstimatedSize;
            }

            public ExpressionType NodeType { get; }

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _binaryConditionLeftTranslation.WriteInParenthesesIfRequired(context);
                context.WriteToTranslation(_binaryConditionOperator.TrimEnd());
                context.WriteNewLineToTranslation();
                context.Indent();
                _binaryConditionRightTranslation.WriteInParenthesesIfRequired(context);
                context.Unindent();
            }
        }
    }
}