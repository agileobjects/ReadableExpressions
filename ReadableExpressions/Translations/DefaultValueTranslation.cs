namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Formatting;
    using NetStandardPolyfills;

    internal static class DefaultValueTranslation
    {
        public static ITranslation For(
            Expression defaultExpression,
            ITranslationContext context,
            bool allowNullKeyword = true)
        {
            var type = defaultExpression.Type;

            if (type == typeof(void))
            {
                return DefaultVoidTranslation.Instance;
            }

            if (allowNullKeyword)
            {
                allowNullKeyword =
                    (type.FullName != null) &&
                    !string.IsNullOrEmpty(type.GetAssembly().GetLocation());

                if (allowNullKeyword && type.CanBeNull())
                {
                    return new NullKeywordTranslation(type, context);
                }
            }

            return new DefaultOperatorTranslation(context.GetTranslationFor(type), context.Settings);
        }

        private class DefaultVoidTranslation : ITranslation, IPotentialEmptyTranslatable
        {
            public static readonly ITranslation Instance = new DefaultVoidTranslation();

            public ExpressionType NodeType => ExpressionType.Default;

            public Type Type => typeof(void);

            public bool IsEmpty => true;

            public int TranslationSize => 0;

            public int FormattingSize => 0;

            public int GetIndentSize() => 0;

            public int GetLineCount() => 1;

            public void WriteTo(TranslationWriter writer)
            {
            }
        }

        private class NullKeywordTranslation : FixedValueTranslation
        {
            public NullKeywordTranslation(Type nullType, ITranslationContext context)
                : base(
                    ExpressionType.Default,
                    "null",
                    nullType,
                    TokenType.Keyword,
                    context)
            {
            }
        }
    }
}