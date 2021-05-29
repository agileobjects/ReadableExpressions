﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
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

            if (allowNullKeyword && (type.FullName != null) && type.CanBeNull())
            {
                return new NullKeywordTranslation(type, context);
            }

            return new DefaultOperatorTranslation(type, context);
        }

        private class DefaultVoidTranslation : EmptyTranslatable, ITranslation
        {
            public new static readonly ITranslation Instance = new DefaultVoidTranslation();

            public ExpressionType NodeType => ExpressionType.Default;

            public Type Type => typeof(void);
        }

        private class NullKeywordTranslation : FixedValueTranslation, INullKeywordTranslation
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