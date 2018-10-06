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
        private readonly UnaryExpression _cast;
        private readonly bool _isBoxing;
        private readonly bool _isImplicitOperator;
        private readonly bool _isOperator;
        private readonly bool _isAssignmentResultCast;
        private readonly ITranslation _castTypeNameTranslation;
        private readonly ITranslation _castValueTranslation;
        private readonly Action<ITranslationContext> _translationWriter;

        public CastTranslation(UnaryExpression cast, ITranslationContext context)
        {
            _cast = cast;
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

        public int EstimatedSize { get; }

        private int EstimateCastSize()
        {
            if (_isBoxing)
            {
                return _castValueTranslation.EstimatedSize;
            }

            var castValueSupplement = _isAssignmentResultCast ? 2 : 0;

            if (_isImplicitOperator)
            {
                return _castValueTranslation.EstimatedSize + castValueSupplement;
            }

            return _castValueTranslation.EstimatedSize +
                   _castTypeNameTranslation.EstimatedSize +
                   2 + // <- For cast type name parentheses
                   castValueSupplement;
        }

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

            if (_cast.NodeType == Assign)
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