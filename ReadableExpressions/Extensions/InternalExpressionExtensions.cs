namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using BlockExpression = Microsoft.Scripting.Ast.BlockExpression;
    using ConstantExpression = Microsoft.Scripting.Ast.ConstantExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
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
                case ExpressionType.Block:
                    return ((BlockExpression)expression).IsReturnable();

                case ExpressionType.Constant:
                    return !expression.IsComment();

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Call:
                case ExpressionType.Coalesce:
                case ExpressionType.Conditional:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Default:
                case ExpressionType.Divide:
                case ExpressionType.Invoke:
                case ExpressionType.ListInit:
                case ExpressionType.MemberAccess:
                case ExpressionType.MemberInit:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.New:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                case ExpressionType.Parameter:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return true;
            }

            return false;
        }

        public static bool IsReturnable(this BlockExpression block)
        {
            return (block.Type != typeof(void)) && block.Result.IsReturnable();
        }

        public static bool IsComment(this Expression expression)
            => (expression.NodeType == ExpressionType.Constant) && ((ConstantExpression)expression).IsComment();

        public static bool IsComment(this ConstantExpression constant)
            => (constant.Value is string value) && value.IsComment();

        public static bool IsAssignment(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AndAssign:
                case ExpressionType.Assign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.RightShiftAssign:
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