namespace AgileObjects.ReadableExpressions.Translations;

using System.Linq;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using System.Reflection;
using Extensions;
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

    public static INodeTranslation For(
        TypeBinaryExpression typeBinary,
        ITranslationContext context)
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

                if (reducedTypeBinary.NodeType == ExpressionType.Block)
                {
                    operandTranslation = ((BlockTranslation)operandTranslation).WithoutTermination();
                }

                return operandTranslation.WithNodeType(ExpressionType.TypeEqual);
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

    private class TypeOfTranslation : INodeTranslation
    {
        private const string _typeOf = " TypeOf typeof";
        private readonly ITranslation _operandTranslation;
        private readonly ITranslation _typeNameTranslation;

        public TypeOfTranslation(
            ITranslation operandTranslation,
            ITranslation typeNameTranslation)
        {
            _operandTranslation = operandTranslation;
            _typeNameTranslation = typeNameTranslation;
        }

        public ExpressionType NodeType => ExpressionType.TypeEqual;

        public int TranslationLength =>
            _operandTranslation.TranslationLength +
            _typeNameTranslation.TranslationLength +
            _typeOf.Length + "()".Length;

        public void WriteTo(TranslationWriter writer)
        {
            _operandTranslation.WriteTo(writer);
            writer.WriteKeywordToTranslation(_typeOf);
            _typeNameTranslation.WriteInParentheses(writer);
        }
    }
}