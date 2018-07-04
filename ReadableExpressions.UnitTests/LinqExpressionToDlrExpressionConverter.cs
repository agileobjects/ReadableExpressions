#if NET35
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq.Expressions;
    using Drl = Microsoft.Scripting.Ast;

    /// <summary>
    /// Converts a DynamicRuntimeLibrary Expression object to a Linq Expression object.
    /// </summary>
    public class DlrExpressionToLinqExpressionConverter
    {
        /// <summary>
        /// Converts the given <paramref name="drlExpression"/> to a Linq Expression object.
        /// </summary>
        /// <param name="drlExpression">The DynamicRuntimeLibrary Expression to convert.</param>
        /// <returns>The converted Linq Expression.</returns>
        public static Expression Convert(Drl.Expression drlExpression)
        {
            switch (drlExpression.NodeType)
            {
                case Drl.ExpressionType.Add:
                    break;

                case Drl.ExpressionType.AddChecked:
                    break;
                case Drl.ExpressionType.And:
                    break;
                case Drl.ExpressionType.AndAlso:
                    break;
                case Drl.ExpressionType.ArrayLength:
                    break;
                case Drl.ExpressionType.ArrayIndex:
                    break;
                
                case Drl.ExpressionType.Block:
                    return Expression.Block
                
                case Drl.ExpressionType.Call:
                    break;
                case Drl.ExpressionType.Coalesce:
                    break;
                case Drl.ExpressionType.Conditional:
                    break;
                case Drl.ExpressionType.Constant:
                    return Expression.Constant(((Drl.ConstantExpression)drlExpression).Value, drlExpression.Type);

                case Drl.ExpressionType.Convert:
                    break;
                case Drl.ExpressionType.ConvertChecked:
                    break;
                case Drl.ExpressionType.Divide:
                    break;
                case Drl.ExpressionType.Equal:
                    break;
                case Drl.ExpressionType.ExclusiveOr:
                    break;
                case Drl.ExpressionType.GreaterThan:
                    break;
                case Drl.ExpressionType.GreaterThanOrEqual:
                    break;
                case Drl.ExpressionType.Invoke:
                    break;
                case Drl.ExpressionType.Lambda:
                    break;
                case Drl.ExpressionType.LeftShift:
                    break;
                case Drl.ExpressionType.LessThan:
                    break;
                case Drl.ExpressionType.LessThanOrEqual:
                    break;
                case Drl.ExpressionType.ListInit:
                    break;
                case Drl.ExpressionType.MemberAccess:
                    break;
                case Drl.ExpressionType.MemberInit:
                    break;
                case Drl.ExpressionType.Modulo:
                    break;
                case Drl.ExpressionType.Multiply:
                    break;
                case Drl.ExpressionType.MultiplyChecked:
                    break;
                case Drl.ExpressionType.Negate:
                    break;
                case Drl.ExpressionType.UnaryPlus:
                    break;
                case Drl.ExpressionType.NegateChecked:
                    break;
                case Drl.ExpressionType.New:
                    break;
                case Drl.ExpressionType.NewArrayInit:
                    break;
                case Drl.ExpressionType.NewArrayBounds:
                    break;
                case Drl.ExpressionType.Not:
                    break;
                case Drl.ExpressionType.NotEqual:
                    break;
                case Drl.ExpressionType.Or:
                    break;
                case Drl.ExpressionType.OrElse:
                    break;
                case Drl.ExpressionType.Parameter:
                    break;
                case Drl.ExpressionType.Power:
                    break;
                case Drl.ExpressionType.Quote:
                    break;
                case Drl.ExpressionType.RightShift:
                    break;
                case Drl.ExpressionType.Subtract:
                    break;
                case Drl.ExpressionType.SubtractChecked:
                    break;
                case Drl.ExpressionType.TypeAs:
                    break;
                case Drl.ExpressionType.TypeIs:
                    break;
            }

            throw new NotSupportedException(
                "Unable to convert Expressions of type " + drlExpression.NodeType);
        }
    }
}
#endif