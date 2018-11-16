﻿namespace AgileObjects.ReadableExpressions.Extensions
{
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static class InternalExpressionExtensions
    {
        public static bool IsReturnable(this Expression expression)
        {
            if (expression.Type == typeof(void))
            {
                return false;
            }

            switch (expression.NodeType)
            {
                case Block:
                    return ((BlockExpression)expression).IsReturnable();

                case Constant:
                    return !expression.IsComment();

                case Add:
                case AddChecked:
                case Call:
                case Coalesce:
                case Conditional:
                case Convert:
                case ConvertChecked:
                case Default:
                case Divide:
                case Invoke:
                case Label:
                case ListInit:
                case MemberAccess:
                case MemberInit:
                case Multiply:
                case MultiplyChecked:
                case New:
                case NewArrayBounds:
                case NewArrayInit:
                case Parameter:
                case Subtract:
                case SubtractChecked:
                    return true;
            }

            return false;
        }

        public static bool IsReturnable(this BlockExpression block)
            => (block.Type != typeof(void)) && block.Result.IsReturnable();

        public static bool IsComment(this Expression expression)
            => (expression.NodeType == Constant) && IsComment((ConstantExpression)expression);

        private static bool IsComment(ConstantExpression constant)
            => (constant.Value is string value) && value.IsComment();
    }
}