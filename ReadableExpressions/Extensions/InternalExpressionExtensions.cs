namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#else
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
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
                case ExpressionType.Convert:
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
            => (expression.NodeType == Constant) && ((ConstantExpression)expression).IsComment();

        public static bool IsComment(this ConstantExpression constant)
            => (constant.Value is string value) && value.IsComment();

        public static bool IsAssignment(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case AddAssign:
                case AddAssignChecked:
                case AndAssign:
                case Assign:
                case DivideAssign:
                case ExclusiveOrAssign:
                case LeftShiftAssign:
                case ModuloAssign:
                case MultiplyAssign:
                case MultiplyAssignChecked:
                case OrAssign:
                case PowerAssign:
                case SubtractAssign:
                case SubtractAssignChecked:
                case RightShiftAssign:
                    return true;
            }

            return false;
        }

        public static ExpressionType[] GetCheckedExpressionTypes(this Dictionary<ExpressionType, string> valuesByExpressionTypes)
        {
            return valuesByExpressionTypes
               .Keys
               .Filter(nt => nt.ToString().EndsWith("Checked", StringComparison.Ordinal))
               .ToArray();
        }
    }
}