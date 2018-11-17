namespace AgileObjects.ReadableExpressions.Translations
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using Interfaces;

    internal class UnaryTranslation : ITranslation
    {
        private static readonly Dictionary<ExpressionType, string> _operatorsByNodeType =
            new Dictionary<ExpressionType, string>(10)
            {
                [Decrement] = "--",
                [Increment] = "++",
                [IsTrue] = string.Empty,
                [IsFalse] = "!",
                [OnesComplement] = "~",
                [PostDecrementAssign] = "--",
                [PostIncrementAssign] = "++",
                [PreDecrementAssign] = "--",
                [PreIncrementAssign] = "++",
                [UnaryPlus] = "+"
            };

        private readonly string _operator;
        private readonly ITranslation _operandTranslation;
        private readonly bool _operatorIsSuffix;

        public UnaryTranslation(UnaryExpression unary, ITranslationContext context)
        {
            NodeType = unary.NodeType;
            Type = unary.Type;
            _operator = _operatorsByNodeType[NodeType];

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