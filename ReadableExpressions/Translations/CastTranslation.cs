namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using NetStandardPolyfills;
    using Translators;

    internal class CastTranslation : ITranslation
    {
        private readonly ITranslation _castValueTranslation;
        private readonly ITranslation _castTypeNameTranslation;
        private readonly bool _isBoxing;
        private readonly bool _isImplicitOperator;
        private readonly bool _isOperator;
        private readonly bool _writeCastValueInParentheses;
        private readonly Action<ITranslationContext> _translationWriter;

        public CastTranslation(UnaryExpression cast, ITranslationContext context)
            : this(cast.Operand.NodeType)
        {
            NodeType = cast.NodeType;
            _castValueTranslation = context.GetTranslationFor(cast.Operand);
            var estimatedSizeFactory = default(Func<int>);
            var isCustomMethodCast = false;

            switch (NodeType)
            {
                case ExpressionType.Convert:
                case ConvertChecked:
                    _isBoxing = cast.Type == typeof(object);

                    if (cast.Method != null)
                    {
                        _isImplicitOperator = cast.Method.IsImplicitOperator();
                        _isOperator = _isImplicitOperator || cast.Method.IsExplicitOperator();

                        if (_isOperator == false)
                        {
                            isCustomMethodCast = true;
                            _translationWriter = WriteCustomMethodCast;
                            break;
                        }
                    }

                    estimatedSizeFactory = EstimateCastSize;
                    break;

                case TypeAs:
                    _translationWriter = WriteTypeAsCast;
                    estimatedSizeFactory = EstimateTypeCastSize;
                    break;

                case Unbox:
                    _translationWriter = WriteCastCore;
                    estimatedSizeFactory = EstimateCastSize;
                    break;
            }

            if ((_isImplicitOperator == false) && (_isBoxing == false))
            {
                _castTypeNameTranslation = context.GetTranslationFor(cast.Type);
            }

            if (isCustomMethodCast == false)
            {
                // ReSharper disable once PossibleNullReferenceException
                EstimatedSize = estimatedSizeFactory.Invoke();
                return;
            }

            _castValueTranslation = MethodCallTranslation.ForCustomMethodCast(
                _castTypeNameTranslation,
                new BclMethodWrapper(cast.Method),
                _castValueTranslation);

            EstimatedSize = _castValueTranslation.EstimatedSize;
        }

        public CastTranslation(TypeBinaryExpression cast, ITranslationContext context)
        {
            NodeType = cast.NodeType;
            _castValueTranslation = context.GetTranslationFor(cast.Expression);
            _castTypeNameTranslation = context.GetTranslationFor(cast.TypeOperand);
            _translationWriter = WriteTypeIsCast;
            EstimatedSize = EstimateTypeCastSize();
        }

        private CastTranslation(ITranslation castValueTranslation, ITranslation castTypeNameTranslation)
            : this(castValueTranslation.NodeType)
        {
            _castValueTranslation = castValueTranslation;
            _castTypeNameTranslation = castTypeNameTranslation;
            _isOperator = true;
            _translationWriter = WriteCastCore;
            EstimatedSize = EstimateCastSize();
        }

        private CastTranslation(ExpressionType castValueNodeType)
        {
            if (castValueNodeType == Assign || IsCast(castValueNodeType))
            {
                _writeCastValueInParentheses = true;
            }
        }

        public static ITranslation ForExplicitOperator(
            ITranslation castValueTranslation,
            ITranslation castTypeNameTranslation)
        {
            return new CastTranslation(castValueTranslation, castTypeNameTranslation);
        }

        public static bool IsCast(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Convert:
                case ConvertChecked:
                case TypeAs:
                case TypeIs:
                case Unbox:
                    return true;
            }

            return false;
        }

        private int EstimateCastSize()
        {
            var estimatedSize = GetBaseEstimatedSize();

            if (_isBoxing || _isImplicitOperator)
            {
                return estimatedSize;
            }

            // +2 for cast type name parentheses:
            estimatedSize += _castTypeNameTranslation.EstimatedSize + 2;

            if (_isOperator)
            {
                // TODO: Explicit operator
                return estimatedSize;
            }

            return estimatedSize;
        }

        private int EstimateTypeCastSize()
        {
            var estimatedSize = GetBaseEstimatedSize();

            if (_isBoxing)
            {
                return estimatedSize;
            }

            // +4 for ' as ' or ' is ':
            return estimatedSize + 4;
        }

        private int GetBaseEstimatedSize()
        {
            var estimatedSize = _castValueTranslation.EstimatedSize;

            if (_writeCastValueInParentheses && (_isBoxing == false))
            {
                estimatedSize += 2;
            }

            return estimatedSize;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        private void WriteCustomMethodCast(ITranslationContext context)
            => _castValueTranslation.WriteTo(context);

        private void WriteTypeAsCast(ITranslationContext context) => WriteTypeTestedCast(" as ", context);

        private void WriteTypeIsCast(ITranslationContext context) => WriteTypeTestedCast(" is ", context);

        private void WriteTypeTestedCast(string typeTest, ITranslationContext context)
        {
            WriteCastValueTranslation(context);
            context.WriteToTranslation(typeTest);
            _castTypeNameTranslation.WriteTo(context);
        }

        public void WriteTo(ITranslationContext context)
        {
            if (_translationWriter != null)
            {
                _translationWriter.Invoke(context);
                return;
            }

            if (_isBoxing)
            {
                // Don't bother showing a boxing operation:
                _castValueTranslation.WriteTo(context);
                return;
            }

            WriteCastCore(context);
        }

        private void WriteCastCore(ITranslationContext context)
        {
            if (_isImplicitOperator == false)
            {
                _castTypeNameTranslation.WriteInParentheses(context);
            }

            WriteCastValueTranslation(context);
        }

        private void WriteCastValueTranslation(ITranslationContext context)
        {
            if (_writeCastValueInParentheses)
            {
                _castValueTranslation.WriteInParentheses(context);
                return;
            }

            _castValueTranslation.WriteTo(context);
        }
    }
}