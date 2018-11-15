namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Interfaces;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using NetStandardPolyfills;

    internal static class TypeEqualTranslation
    {
        private static readonly MethodInfo _reduceTypeEqualMethod;

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

        public static ITranslation For(TypeBinaryExpression typeBinary, ITranslationContext context)
        {
            ITranslation operandTranslation;

            if (_reduceTypeEqualMethod != null)
            {
                try
                {
                    // TypeEqual '123 TypeEqual int' is reduced to a Block with the Expressions '123' and 'true',
                    // 'o TypeEqual string' is reduced to (o != null) && (o is string):
                    var reducedTypeBinary = (Expression)_reduceTypeEqualMethod.Invoke(typeBinary, null);

                    operandTranslation = context.GetTranslationFor(reducedTypeBinary);

                    if (operandTranslation.NodeType == ExpressionType.Block)
                    {
                        operandTranslation = ((BlockTranslation)operandTranslation).WithoutTermination();
                    }

                    return operandTranslation.WithTypes(ExpressionType.TypeEqual, typeof(bool));
                }
                catch
                {
                    // Unable to invoke the non-public ReduceTypeEqual method - ignore
                }
            }

            operandTranslation = context.GetTranslationFor(typeBinary.Expression);
            var typeNameTranslation = context.GetTranslationFor(typeBinary.TypeOperand);

            if (typeBinary.TypeOperand.IsClass())
            {
                return CastTranslation.For(typeBinary, context);
            }

            return new TypeOfTranslation(operandTranslation, typeNameTranslation);
        }

        private class TypeOfTranslation : ITranslation
        {
            private const string _typeOf = " TypeOf typeof(";
            private readonly ITranslation _operandTranslation;
            private readonly ITranslation _typeNameTranslation;

            public TypeOfTranslation(ITranslation operandTranslation, ITranslation typeNameTranslation)
            {
                _operandTranslation = operandTranslation;
                _typeNameTranslation = typeNameTranslation;
                EstimatedSize = GetEstimatedSize();
            }

            private int GetEstimatedSize()
            {
                var estimatedSize =
                    _operandTranslation.EstimatedSize +
                    _typeNameTranslation.EstimatedSize;

                estimatedSize += _typeOf.Length + 2; // <- +2 for parentheses

                return estimatedSize;
            }

            public ExpressionType NodeType => ExpressionType.TypeEqual;

            public Type Type => typeof(bool);

            public int EstimatedSize { get; }

            public void WriteTo(ITranslationContext context)
            {
                _operandTranslation.WriteTo(context);
                context.WriteToTranslation(_typeOf);
                _typeNameTranslation.WriteTo(context);
                context.WriteToTranslation(')');
            }
        }
    }
}