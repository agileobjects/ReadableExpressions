namespace AgileObjects.ReadableExpressions.Translations;

using System;
using Formatting;
using NetStandardPolyfills;

internal static class DefaultValueTranslation
{
    public static INodeTranslation For(
        Expression defaultExpression,
        ITranslationContext context,
        bool allowNullKeyword = true)
    {
        var type = defaultExpression.Type;

        if (type == typeof(void))
        {
            return DefaultVoidTranslation.Instance;
        }

        if (allowNullKeyword && type.FullName != null && type.CanBeNull())
        {
            return new NullKeywordTranslation(type);
        }

        return new DefaultOperatorTranslation(type, context);
    }

    private class DefaultVoidTranslation : EmptyTranslation, INodeTranslation
    {
        public new static readonly INodeTranslation Instance =
            new DefaultVoidTranslation();

        public ExpressionType NodeType => ExpressionType.Default;
    }

    private class NullKeywordTranslation :
        FixedValueTranslation,
        INullKeywordTranslation
    {
        public NullKeywordTranslation(Type nullType) :
            base(ExpressionType.Default, "null", TokenType.Keyword)
        {
            NullType = nullType;
        }

        public Type NullType { get; }
    }
}