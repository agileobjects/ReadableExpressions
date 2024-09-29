namespace AgileObjects.ReadableExpressions.Translations;

using System;
using Extensions;
using NetStandardPolyfills;
using Reflection;
using static ExpressionType;

internal static class CastTranslation
{
    public static INodeTranslation For(
        UnaryExpression cast,
        ITranslationContext context)
    {
        var castValueTranslation = context.GetTranslationFor(cast.Operand);

        switch (cast.NodeType)
        {
            case ExpressionType.Convert:
            case ConvertChecked:
                if (IsBoxing(cast))
                {
                    if (context.Analysis.IsNestedCast(cast))
                    {
                        break;
                    }

                    // Don't bother to show boxing:
                    return castValueTranslation;
                }

                if (cast.Method != null)
                {
                    var isImplicitOperator = cast.Method.IsImplicitOperator();

                    if (isImplicitOperator)
                    {
                        return castValueTranslation.ShouldWriteInParentheses()
                            ? castValueTranslation.WithParentheses()
                            : castValueTranslation;
                    }

                    if (cast.Method.IsExplicitOperator())
                    {
                        break;
                    }

                    return MethodCallTranslation.ForCustomMethodCast(
                        new ClrMethodWrapper(cast.Method, context),
                        castValueTranslation,
                        context);
                }

                if (IsDelegateCast(cast, out var createDelegateCall))
                {
                    return MethodGroupTranslation.ForCreateDelegateCall(
                        cast.NodeType,
                        createDelegateCall,
                        context);
                }

                break;

            case TypeAs:
                return new TypeTestedTranslation(
                    TypeAs,
                    castValueTranslation,
                    " as ",
                    cast.Type,
                    context);
        }

        return new StandardCastTranslation(cast, castValueTranslation, context);
    }

    private static bool IsBoxing(UnaryExpression cast)
    {
        return cast.Type == typeof(object) &&
              (cast.Operand.Type == typeof(string) || cast.Operand.Type.IsValueType());
    }

    private static bool IsDelegateCast(
        UnaryExpression cast,
        out MethodCallExpression createDelegateCall)
    {
        if (cast.Operand.NodeType == Call &&
            cast.Operand.Type == typeof(Delegate) &&
           (createDelegateCall = (MethodCallExpression)cast.Operand).Method.Name == "CreateDelegate")
        {
            return true;
        }

        createDelegateCall = null;
        return false;
    }

    public static INodeTranslation For(
        TypeBinaryExpression typeIs,
        ITranslationContext context)
    {
        return new TypeTestedTranslation(
            TypeIs,
            context.GetTranslationFor(typeIs.Expression),
            " is ",
            typeIs.TypeOperand,
            context);
    }

    public static INodeTranslation ForExplicitOperator(
        INodeTranslation castValueTranslation,
        INodeTranslation castTypeNameTranslation)
    {
        return new StandardCastTranslation(
            Call,
            castTypeNameTranslation,
            castValueTranslation);
    }

    public static bool IsCast(ExpressionType nodeType)
    {
        return nodeType switch
        {
            TypeAs => true,
            TypeIs => true,
            Unbox => true,
            _ when IsConversion(nodeType) => true,
            _ => false
        };
    }

    public static bool IsConversion(ExpressionType nodeType)
    {
        return nodeType switch
        {
            ExpressionType.Convert => true,
            ConvertChecked => true,
            _ => false
        };
    }

    private class TypeTestedTranslation : INodeTranslation
    {
        private readonly INodeTranslation _testedValueTranslation;
        private readonly string _test;
        private readonly INodeTranslation _testedTypeNameTranslation;

        public TypeTestedTranslation(
            ExpressionType nodeType,
            INodeTranslation testedValueTranslation,
            string test,
            Type testedType,
            ITranslationContext context)
        {
            NodeType = nodeType;
            _testedValueTranslation = testedValueTranslation;
            _test = test;
            _testedTypeNameTranslation = context.GetTranslationFor(testedType);
        }

        public ExpressionType NodeType { get; }

        public int TranslationLength =>
            _testedValueTranslation.TranslationLength +
            _test.Length +
            _testedTypeNameTranslation.TranslationLength;

        public void WriteTo(TranslationWriter writer)
        {
            _testedValueTranslation.WriteTo(writer);
            writer.WriteKeywordToTranslation(_test);
            _testedTypeNameTranslation.WriteTo(writer);
        }
    }

    private class StandardCastTranslation : INodeTranslation
    {
        private readonly INodeTranslation _castValueTranslation;
        private readonly INodeTranslation _castTypeNameTranslation;

        public StandardCastTranslation(
            Expression cast,
            INodeTranslation castValueTranslation,
            ITranslationContext context) :
            this(
                cast.NodeType,
                context.GetTranslationFor(cast.Type),
                castValueTranslation)
        {
        }

        public StandardCastTranslation(
            ExpressionType nodeType,
            INodeTranslation castTypeNameTranslation,
            INodeTranslation castValueTranslation)
        {
            NodeType = nodeType;
            _castTypeNameTranslation = castTypeNameTranslation;
            _castValueTranslation = castValueTranslation;

            if (_castValueTranslation.ShouldWriteInParentheses())
            {
                _castValueTranslation = _castValueTranslation.WithParentheses();
            }
        }

        public ExpressionType NodeType { get; }

        public int TranslationLength =>
            _castTypeNameTranslation.TranslationLength +
            "()".Length +
            _castValueTranslation.TranslationLength;

        public void WriteTo(TranslationWriter writer)
        {
            _castTypeNameTranslation.WriteInParentheses(writer);
            _castValueTranslation.WriteTo(writer);
        }
    }
}