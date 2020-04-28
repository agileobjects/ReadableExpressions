namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal static class ConditionTranslation
    {
        public static ITranslation For(Expression condition, ITranslationContext context)
        {
            var conditionTranslation = context.GetTranslationFor(condition);

            if (IsMultiLineBinary(condition, conditionTranslation))
            {
                return new MultiLineBinaryConditionTranslation((BinaryExpression)condition, conditionTranslation, context);
            }

            var conditionCodeBlockTranslation = new CodeBlockTranslation(conditionTranslation, context);

            return conditionTranslation.IsMultiStatement()
                ? conditionCodeBlockTranslation.WithSingleCodeBlockParameterFormatting()
                : conditionCodeBlockTranslation;
        }

        private static bool IsMultiLineBinary(Expression condition, ITranslatable conditionTranslation)
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
            private readonly ITranslationContext _context;
            private readonly ITranslation _binaryConditionLeftTranslation;
            private readonly string _binaryConditionOperator;
            private readonly ITranslation _binaryConditionRightTranslation;

            public MultiLineBinaryConditionTranslation(
                BinaryExpression binaryCondition,
                ITranslatable conditionTranslatable,
                ITranslationContext context)
            {
                _context = context;
                NodeType = binaryCondition.NodeType;
                _binaryConditionLeftTranslation = For(binaryCondition.Left, context);
                _binaryConditionOperator = BinaryTranslation.GetOperator(binaryCondition);
                _binaryConditionRightTranslation = For(binaryCondition.Right, context);
                TranslationSize = conditionTranslatable.TranslationSize;
                FormattingSize = conditionTranslatable.FormattingSize;
            }

            public ExpressionType NodeType { get; }

            public Type Type => typeof(bool);

            public int TranslationSize { get; }
            
            public int FormattingSize { get; }

            public void WriteTo(TranslationBuffer buffer)
            {
                _binaryConditionLeftTranslation.WriteInParenthesesIfRequired(buffer, _context);
                buffer.WriteToTranslation(_binaryConditionOperator.TrimEnd());
                buffer.WriteNewLineToTranslation();
                buffer.Indent();
                _binaryConditionRightTranslation.WriteInParenthesesIfRequired(buffer, _context);
                buffer.Unindent();
            }
        }
    }
}