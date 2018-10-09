namespace AgileObjects.ReadableExpressions.Translations
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal class UnaryTranslation : ITranslation
    {
        private static readonly Dictionary<ExpressionType, string> _operatorsByNodeType =
            new Dictionary<ExpressionType, string>(11)
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
                [Throw] = "throw ",
                [UnaryPlus] = "+"
            };

        private readonly string _operator;
        private readonly ITranslation _operandTranslation;
        private readonly bool _operatorIsSuffix;

        public UnaryTranslation(UnaryExpression unary, ITranslationContext context)
        {
            NodeType = unary.NodeType;
            _operator = _operatorsByNodeType[NodeType];

            switch (NodeType)
            {
                case PostDecrementAssign:
                case PostIncrementAssign:
                    _operatorIsSuffix = true;
                    break;
            }

            // ReSharper disable HeuristicUnreachableCode
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // unary.Operand is null when using Expression.Rethrow():
            if (unary.Operand == null)
            {
                EstimatedSize = _operator.Length;
                return;
            }
            // ReSharper restore HeuristicUnreachableCode

            _operandTranslation = context.GetTranslationFor(unary.Operand);
            EstimatedSize = _operandTranslation.EstimatedSize + _operator.Length;
        }

        public ExpressionType NodeType { get; }

        public int EstimatedSize { get; }

        public void WriteTo(ITranslationContext context)
        {
            if (_operatorIsSuffix == false)
            {
                context.WriteToTranslation(_operator);
            }

            _operandTranslation?.WriteTo(context);

            if (_operatorIsSuffix)
            {
                context.WriteToTranslation(_operator);
            }
        }
    }
}