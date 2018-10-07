namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class CastTranslation : ITranslation
    {
        private readonly bool _isBoxing;
        private readonly bool _isImplicitOperator;
        private readonly bool _isOperator;
        private readonly bool _isAssignmentResultCast;
        private readonly ITranslation _castTypeNameTranslation;
        private readonly ITranslation _castValueTranslation;
        private readonly Action<ITranslationContext> _translationWriter;

        public CastTranslation(UnaryExpression cast, ITranslationContext context)
            : this(cast.Operand.NodeType)
        {
            NodeType = cast.NodeType;
            _castValueTranslation = context.GetTranslationFor(cast.Operand);
            var estimatedSizeFactory = default(Func<int>);

            switch (NodeType)
            {
                case ExpressionType.Convert:
                case ConvertChecked:
                    _isBoxing = cast.Type == typeof(object);

                    if (cast.Method != null)
                    {
                        _isImplicitOperator = cast.Method.IsImplicitOperator();
                        _isOperator = _isImplicitOperator || cast.Method.IsExplicitOperator();
                    }

                    estimatedSizeFactory = EstimateCastSize;
                    _translationWriter = WriteCast;
                    break;

                case TypeAs:
                case TypeIs:
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

            EstimatedSize = estimatedSizeFactory.Invoke();
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
            _isAssignmentResultCast = castValueNodeType == Assign;
        }

        public static ITranslation ForExplicitOperator(
            ITranslation castValueTranslation,
            ITranslation castTypeNameTranslation)
        {
            return new CastTranslation(castValueTranslation, castTypeNameTranslation);
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

            if (_isAssignmentResultCast && (_isBoxing == false))
            {
                estimatedSize += 2;
            }

            return estimatedSize;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        private void WriteCast(ITranslationContext context)
        {
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
            if (_isAssignmentResultCast)
            {
                _castValueTranslation.WriteInParentheses(context);
            }
            else
            {
                _castValueTranslation.WriteTo(context);
            }
        }

        private void WriteTypeAsCast(ITranslationContext context)
        {
            WriteCastValueTranslation(context);
            context.WriteToTranslation(" as ");
            _castTypeNameTranslation.WriteTo(context);
        }

        public void WriteTo(ITranslationContext context) => _translationWriter.Invoke(context);
    }
}