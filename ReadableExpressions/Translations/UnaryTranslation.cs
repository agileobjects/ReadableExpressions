﻿namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Interfaces;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class UnaryTranslation : ITranslation
    {
        private readonly string _operator;
        private readonly ITranslation _operandTranslation;
        private readonly bool _operatorIsSuffix;

        public UnaryTranslation(UnaryExpression unary, ITranslationContext context)
        {
            NodeType = unary.NodeType;
            Type = unary.Type;
            _operator = GetOperatorFor(unary.NodeType);

            switch (NodeType)
            {
                case PostDecrementAssign:
                case PostIncrementAssign:
                    _operatorIsSuffix = true;
                    break;
            }

            _operandTranslation = context.GetTranslationFor(unary.Operand);
            EstimatedSize = _operandTranslation.EstimatedSize + _operator.Length;
        }

        private static string GetOperatorFor(ExpressionType nodeType)
        {
            return nodeType switch
            {
                Decrement => "--",
                Increment => "++",
                IsTrue => string.Empty,
                IsFalse => "!",
                OnesComplement => "~",
                PostDecrementAssign => "--",
                PostIncrementAssign => "++",
                PreDecrementAssign => "--",
                PreIncrementAssign => "++",
                _ => "+"
            };
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; }

        public int EstimatedSize { get; }

        public void WriteTo(TranslationBuffer buffer)
        {
            if (_operatorIsSuffix == false)
            {
                buffer.WriteToTranslation(_operator);
            }

            _operandTranslation?.WriteTo(buffer);

            if (_operatorIsSuffix)
            {
                buffer.WriteToTranslation(_operator);
            }
        }
    }
}