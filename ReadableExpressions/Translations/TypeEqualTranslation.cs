namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
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

            return new TypeOfTranslation(operandTranslation, typeNameTranslation, context);
        }

        private class TypeOfTranslation : ITranslation
        {
            private const string _typeOf = " TypeOf typeof";
            private readonly ITranslation _operandTranslation;
            private readonly ITranslation _typeNameTranslation;

            public TypeOfTranslation(
                ITranslation operandTranslation,
                ITranslation typeNameTranslation,
                ITranslationContext context)
            {
                _operandTranslation = operandTranslation;
                _typeNameTranslation = typeNameTranslation;

                TranslationSize =
                     operandTranslation.TranslationSize +
                     typeNameTranslation.TranslationSize +
                    _typeOf.Length + "()".Length;

                FormattingSize =
                    operandTranslation.FormattingSize +
                    typeNameTranslation.FormattingSize +
                    context.GetKeywordFormattingSize();
            }

            public ExpressionType NodeType => ExpressionType.TypeEqual;

            public Type Type => typeof(bool);

            public int TranslationSize { get; }

            public int FormattingSize { get; }

            public int GetIndentSize()
            {
                return _operandTranslation.GetIndentSize() +
                       _typeNameTranslation.GetIndentSize();
            }

            public int GetLineCount()
            {
                return Math.Max(
                    _operandTranslation.GetLineCount(),
                    _typeNameTranslation.GetLineCount());
            }

            public void WriteTo(TranslationWriter writer)
            {
                _operandTranslation.WriteTo(writer);
                writer.WriteKeywordToTranslation(_typeOf);
                _typeNameTranslation.WriteInParentheses(writer);
            }
        }
    }
}