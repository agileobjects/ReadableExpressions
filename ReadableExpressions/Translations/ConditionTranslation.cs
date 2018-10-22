namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConditionTranslation : ITranslation
    {
        private readonly ITranslation _binaryConditionLeftTranslation;
        private readonly string _binaryConditionOperator;
        private readonly ITranslation _binaryConditionRightTranslation;
        private readonly Action<ITranslationContext> _translationWriter;

        public ConditionTranslation(Expression condition, ITranslationContext context)
        {
            NodeType = condition.NodeType;

            var conditionTranslation = context.GetTranslationFor(condition);

            if (SplitBinaryConditionToMultipleLines(conditionTranslation))
            {
                var binaryCondition = (BinaryExpression)condition;
                _binaryConditionLeftTranslation = new ConditionTranslation(binaryCondition.Left, context);
                _binaryConditionOperator = BinaryTranslation.GetOperator(binaryCondition);
                _binaryConditionRightTranslation = new ConditionTranslation(binaryCondition.Right, context);

                _translationWriter = WriteMultiLineBinaryTranslation;
                EstimatedSize = conditionTranslation.EstimatedSize;
                return;
            }

            var conditionCodeBlockTranslation = new CodeBlockTranslation(conditionTranslation);

            if (conditionTranslation.IsMultiStatement())
            {
                conditionCodeBlockTranslation.WithSingleLamdaParameterFormatting();
            }

            _translationWriter = conditionCodeBlockTranslation.WriteTo;
            EstimatedSize = conditionCodeBlockTranslation.EstimatedSize;
        }

        private bool SplitBinaryConditionToMultipleLines(ITranslatable conditionTranslation)
            => conditionTranslation.ExceedsLengthThreshold() && IsRelevantBinary();

        private bool IsRelevantBinary()
        {
            switch (NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return true;
            }

            return false;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        private void WriteMultiLineBinaryTranslation(ITranslationContext context)
        {
            _binaryConditionLeftTranslation.WriteInParenthesesIfRequired(context);
            context.WriteToTranslation(_binaryConditionOperator.TrimEnd());
            context.WriteNewLineToTranslation();
            context.Indent();
            _binaryConditionRightTranslation.WriteInParenthesesIfRequired(context);
            context.Unindent();
        }

        public void WriteTo(ITranslationContext context) => _translationWriter.Invoke(context);
    }
}