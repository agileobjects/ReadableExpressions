namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using NetStandardPolyfills;

    internal class TypeEqualTranslation : ITranslation
    {
        private static readonly MethodInfo _reduceTypeEqualMethod;
        private const string _is = " is ";
        private const string _typeOf = " TypeOf typeof(";
        private readonly ITranslation _operandTranslation;
        private readonly ITranslation _typeNameTranslation;
        private readonly Action<ITranslationContext> _translationWriter;
        private readonly bool _typedOperandIsClass;

        public TypeEqualTranslation(TypeBinaryExpression typeBinary, ITranslationContext context)
        {
            if (_reduceTypeEqualMethod != null)
            {
                try
                {
                    var reducedTypeBinary = (Expression)_reduceTypeEqualMethod.Invoke(typeBinary, null);
                    _operandTranslation = context.GetTranslationFor(reducedTypeBinary);
                    EstimatedSize = _operandTranslation.EstimatedSize;
                    _translationWriter = WriteReducedTypeBinary;
                    return;
                }
                catch
                {
                    // Unable to invoke the non-public ReduceTypeEqual method - ignore
                }
            }

            _operandTranslation = context.GetTranslationFor(typeBinary.Expression);
            _typeNameTranslation = context.GetTranslationFor(typeBinary.TypeOperand);
            _typedOperandIsClass = typeBinary.TypeOperand.IsClass();
            EstimatedSize = GetEstimatedSize();
        }

        private int GetEstimatedSize()
        {
            var estimatedSize =
                _operandTranslation.EstimatedSize +
                _typeNameTranslation.EstimatedSize;

            estimatedSize += _typedOperandIsClass
                ? _is.Length
                : _typeOf.Length + 2; // <- +2 for parentheses

            return estimatedSize;
        }

        static TypeEqualTranslation()
        {
            try
            {
                _reduceTypeEqualMethod = typeof(TypeBinaryExpression)
                    .GetNonPublicInstanceMethods("ReduceTypeEqual")
                    .FirstOrDefault();
            }
            catch
            {
                // Unable to find or access ReduceTypeEqual - ignore
            }
        }

        public ExpressionType NodeType => ExpressionType.TypeEqual;

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_translationWriter != null)
            {
                _translationWriter.Invoke(context);
                return;
            }

            _operandTranslation.WriteTo(context);
            context.WriteToTranslation(_typedOperandIsClass ? _is : _typeOf);
            _typeNameTranslation.WriteTo(context);

            if (_typedOperandIsClass)
            {
                context.WriteToTranslation(')');
            }
        }

        private void WriteReducedTypeBinary(ITranslationContext context)
            => _operandTranslation.WriteTo(context);
    }
}