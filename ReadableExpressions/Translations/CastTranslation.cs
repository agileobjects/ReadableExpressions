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
        {
            NodeType = cast.NodeType;
            _castValueTranslation = context.GetTranslationFor(cast.Operand);
            _isAssignmentResultCast = cast.Operand.NodeType == Assign;
            var estimatedSizeFactory = default(Func<int>);

            switch (cast.NodeType)
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
                case Unbox:
                    _translationWriter = WriteCastCore;
                    estimatedSizeFactory = null;
                    break;
            }

            if ((_isImplicitOperator == false) && (_isBoxing == false))
            {
                _castTypeNameTranslation = context.GetTranslationFor(cast.Type);
            }

            EstimatedSize = estimatedSizeFactory.Invoke();
        }

        private CastTranslation(
            ITranslation castValueTranslation,
            ITranslationContext context)
        {
            _castValueTranslation = castValueTranslation;
            _isOperator = true;
            _translationWriter = WriteCastCore;
            EstimatedSize = EstimateCastSize();
        }

        public static ITranslation ForExplicitOperator(
            ITranslation castValueTranslation)
        {
            return new CastTranslation(castValueTranslation, null);
        }

        private int EstimateCastSize()
        {
            var estimatedSize = _castValueTranslation.EstimatedSize;

            if (_isBoxing)
            {
                return estimatedSize;
            }

            if (_isAssignmentResultCast)
            {
                estimatedSize += 2;
            }

            if (_isImplicitOperator)
            {
                return estimatedSize;
            }

            estimatedSize +=
                _castTypeNameTranslation.EstimatedSize +
                2; // <- For cast type name parentheses

            if (_isOperator)
            {
                // TODO: Explicit operator
                return estimatedSize;
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
            if (_isOperator == false)
            {
                context.WriteToTranslation('(');
            }
            else if (_isImplicitOperator == false)
            {
                _castTypeNameTranslation.WriteInParentheses(context);
            }

            if (_isAssignmentResultCast)
            {
                _castValueTranslation.WriteInParentheses(context);
            }
            else
            {
                _castValueTranslation.WriteTo(context);
            }

            if (_isOperator == false)
            {
                context.WriteToTranslation(')');
            }
        }

        public void WriteTo(ITranslationContext context) => _translationWriter.Invoke(context);
    }
}