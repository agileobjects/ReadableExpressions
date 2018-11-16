﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;

    internal class FixedValueTranslation : ITranslation
    {
        private readonly string _value;

        public FixedValueTranslation(Expression expression)
            : this(expression.NodeType, expression.ToString(), expression.Type)
        {
        }

        public FixedValueTranslation(ExpressionType expressionType, string value, Type type)
        {
            NodeType = expressionType;
            Type = type;
            _value = value;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize => _value.Length;

        public void WriteTo(TranslationBuffer buffer) => buffer.WriteToTranslation(_value);
    }
}