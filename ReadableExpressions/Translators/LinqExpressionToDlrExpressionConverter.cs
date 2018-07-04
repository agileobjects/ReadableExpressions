#if NET35
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using LinqExp = System.Linq.Expressions;

    /// <summary>
    /// Converts a .NET 3.5 Linq Expression object into a DynamicLanguageRuntime Expression object.
    /// </summary>
    public static class LinqExpressionToDlrExpressionConverter
    {
        /// <summary>
        /// Converts the given <paramref name="linqExpression"/> into a DynamicLanguageRuntime Expression.
        /// </summary>
        /// <param name="linqExpression">The Linq Expression to convert.</param>
        /// <returns>The given <paramref name="linqExpression"/> converted into a DynamicLanguageRuntime Expression.</returns>
        public static Expression Convert(LinqExp.Expression linqExpression)
        {
            switch (linqExpression.NodeType)
            {
                case LinqExp.ExpressionType.Add:
                    break;
                case LinqExp.ExpressionType.AddChecked:
                    break;
                case LinqExp.ExpressionType.And:
                    break;
                case LinqExp.ExpressionType.AndAlso:
                    break;
                case LinqExp.ExpressionType.ArrayLength:
                    break;
                case LinqExp.ExpressionType.ArrayIndex:
                    break;
                case LinqExp.ExpressionType.Call:
                    break;
                case LinqExp.ExpressionType.Coalesce:
                    break;
                case LinqExp.ExpressionType.Conditional:
                    break;
                case LinqExp.ExpressionType.Constant:
                    break;
                case LinqExp.ExpressionType.Convert:
                    break;
                case LinqExp.ExpressionType.ConvertChecked:
                    break;
                case LinqExp.ExpressionType.Divide:
                    break;
                case LinqExp.ExpressionType.Equal:
                    break;
                case LinqExp.ExpressionType.ExclusiveOr:
                    break;
                case LinqExp.ExpressionType.GreaterThan:
                    break;
                case LinqExp.ExpressionType.GreaterThanOrEqual:
                    break;
                case LinqExp.ExpressionType.Invoke:
                    break;
                case LinqExp.ExpressionType.Lambda:
                    break;
                case LinqExp.ExpressionType.LeftShift:
                    break;
                case LinqExp.ExpressionType.LessThan:
                    break;
                case LinqExp.ExpressionType.LessThanOrEqual:
                    break;
                case LinqExp.ExpressionType.ListInit:
                    break;
                case LinqExp.ExpressionType.MemberAccess:
                    break;
                case LinqExp.ExpressionType.MemberInit:
                    break;
                case LinqExp.ExpressionType.Modulo:
                    break;
                case LinqExp.ExpressionType.Multiply:
                    break;
                case LinqExp.ExpressionType.MultiplyChecked:
                    break;
                case LinqExp.ExpressionType.Negate:
                    break;
                case LinqExp.ExpressionType.UnaryPlus:
                    break;
                case LinqExp.ExpressionType.NegateChecked:
                    break;
                case LinqExp.ExpressionType.New:
                    break;
                case LinqExp.ExpressionType.NewArrayInit:
                    break;
                case LinqExp.ExpressionType.NewArrayBounds:
                    break;
                case LinqExp.ExpressionType.Not:
                    break;
                case LinqExp.ExpressionType.NotEqual:
                    break;
                case LinqExp.ExpressionType.Or:
                    break;
                case LinqExp.ExpressionType.OrElse:
                    break;
                case LinqExp.ExpressionType.Parameter:
                    break;
                case LinqExp.ExpressionType.Power:
                    break;
                case LinqExp.ExpressionType.Quote:
                    break;
                case LinqExp.ExpressionType.RightShift:
                    break;
                case LinqExp.ExpressionType.Subtract:
                    break;
                case LinqExp.ExpressionType.SubtractChecked:
                    break;
                case LinqExp.ExpressionType.TypeAs:
                    break;
                case LinqExp.ExpressionType.TypeIs:
                    break;
            }

            throw new NotSupportedException("Can't convert a " + linqExpression.NodeType);
        }
    }
}
#endif