namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class UnaryExpressionTranslator : ExpressionTranslatorBase
    {
        private static readonly Dictionary<ExpressionType, Func<string, string>> _operatorsByNodeType =
            new Dictionary<ExpressionType, Func<string, string>>
            {
                [ExpressionType.Decrement] = o => "--" + o,
                [ExpressionType.Increment] = o => "++" + o,
                [ExpressionType.IsTrue] = o => $"({o} == true)",
                [ExpressionType.IsFalse] = o => $"({o} == false)",
                [ExpressionType.OnesComplement] = o => "~" + o,
                [ExpressionType.PostDecrementAssign] = o => o + "--",
                [ExpressionType.PostIncrementAssign] = o => o + "++",
                [ExpressionType.PreDecrementAssign] = o => "--" + o,
                [ExpressionType.PreIncrementAssign] = o => "++" + o,
                [ExpressionType.Throw] = o => "throw " + o,
                [ExpressionType.UnaryPlus] = o => "+" + o
            };

        public UnaryExpressionTranslator(Func<Expression, string> globalTranslator)
            : base(globalTranslator, _operatorsByNodeType.Keys.ToArray())
        {
        }

        public override string Translate(Expression expression)
        {
            var mathsOperation = (UnaryExpression)expression;
            var operand = GetTranslation(mathsOperation.Operand);

            return _operatorsByNodeType[expression.NodeType].Invoke(operand);
        }
    }
}