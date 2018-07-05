#if NET35
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Scripting.Ast;
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
            => new Converter().ConvertExp(linqExpression);

        private class Converter
        {
            private readonly Dictionary<LinqExp.ParameterExpression, ParameterExpression> _parameters;

            public Converter()
            {
                _parameters = new Dictionary<LinqExp.ParameterExpression, ParameterExpression>();
            }

            public Expression ConvertExp(LinqExp.Expression linqExpression)
            {
                if (linqExpression == null)
                {
                    return null;
                }

                switch (linqExpression.NodeType)
                {
                    case LinqExp.ExpressionType.Add:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Add);

                    case LinqExp.ExpressionType.AddChecked:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.AddChecked);

                    case LinqExp.ExpressionType.And:
                        break;
                    case LinqExp.ExpressionType.AndAlso:
                        break;
                    case LinqExp.ExpressionType.ArrayLength:
                        break;
                    case LinqExp.ExpressionType.ArrayIndex:
                        break;

                    case LinqExp.ExpressionType.Call:
                        return Convert((LinqExp.MethodCallExpression)linqExpression);

                    case LinqExp.ExpressionType.Coalesce:
                        break;

                    case LinqExp.ExpressionType.Conditional:
                        return Convert((LinqExp.ConditionalExpression)linqExpression);

                    case LinqExp.ExpressionType.Constant:
                        return Convert((LinqExp.ConstantExpression)linqExpression);

                    case LinqExp.ExpressionType.Convert:
                        return ConvertConversion((LinqExp.UnaryExpression)linqExpression);

                    case LinqExp.ExpressionType.ConvertChecked:
                        break;

                    case LinqExp.ExpressionType.Divide:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Divide);

                    case LinqExp.ExpressionType.Equal:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Equal);

                    case LinqExp.ExpressionType.ExclusiveOr:
                        break;

                    case LinqExp.ExpressionType.GreaterThan:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.GreaterThan);

                    case LinqExp.ExpressionType.GreaterThanOrEqual:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.GreaterThanOrEqual);

                    case LinqExp.ExpressionType.Invoke:
                        break;

                    case LinqExp.ExpressionType.Lambda:
                        return Convert((LinqExp.LambdaExpression)linqExpression);

                    case LinqExp.ExpressionType.LeftShift:
                        break;
                    case LinqExp.ExpressionType.LessThan:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.LessThan);

                    case LinqExp.ExpressionType.LessThanOrEqual:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.LessThanOrEqual);

                    case LinqExp.ExpressionType.ListInit:
                        return Convert((LinqExp.ListInitExpression)linqExpression);

                    case LinqExp.ExpressionType.MemberAccess:
                        return Convert((LinqExp.MemberExpression)linqExpression);

                    case LinqExp.ExpressionType.MemberInit:
                        return Convert((LinqExp.MemberInitExpression)linqExpression);

                    case LinqExp.ExpressionType.Modulo:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Modulo);

                    case LinqExp.ExpressionType.Multiply:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Multiply);

                    case LinqExp.ExpressionType.MultiplyChecked:
                        break;

                    case LinqExp.ExpressionType.Negate:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.Negate);

                    case LinqExp.ExpressionType.UnaryPlus:
                        break;
                    case LinqExp.ExpressionType.NegateChecked:
                        break;

                    case LinqExp.ExpressionType.New:
                        return Convert((LinqExp.NewExpression)linqExpression);

                    case LinqExp.ExpressionType.NewArrayInit:
                        return Convert((LinqExp.NewArrayExpression)linqExpression);

                    case LinqExp.ExpressionType.NewArrayBounds:
                        break;

                    case LinqExp.ExpressionType.Not:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.Not);

                    case LinqExp.ExpressionType.NotEqual:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.NotEqual);

                    case LinqExp.ExpressionType.Or:
                        break;
                    case LinqExp.ExpressionType.OrElse:
                        break;

                    case LinqExp.ExpressionType.Parameter:
                        return Convert((LinqExp.ParameterExpression)linqExpression);

                    case LinqExp.ExpressionType.Power:
                        break;

                    case LinqExp.ExpressionType.Quote:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.Quote);

                    case LinqExp.ExpressionType.RightShift:
                        break;

                    case LinqExp.ExpressionType.Subtract:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Subtract);

                    case LinqExp.ExpressionType.SubtractChecked:
                        break;
                    case LinqExp.ExpressionType.TypeAs:
                        break;
                    case LinqExp.ExpressionType.TypeIs:
                        break;
                }

                throw new NotSupportedException("Can't convert a " + linqExpression.NodeType);
            }

            private Expression Convert(LinqExp.ListInitExpression linqListInit)
            {
                return Expression.ListInit(
                    Convert(linqListInit.NewExpression),
                    linqListInit.Initializers.Select(Convert));
            }

            private NewExpression Convert(LinqExp.NewExpression linqNew)
            {
                return Expression.New(
                    linqNew.Constructor,
                    linqNew.Arguments.Select(ConvertExp),
                    linqNew.Members);
            }

            private ElementInit Convert(LinqExp.ElementInit linqElementInit)
            {
                return Expression.ElementInit(
                    linqElementInit.AddMethod,
                    linqElementInit.Arguments.Select(ConvertExp));
            }

            private Expression Convert(LinqExp.NewArrayExpression linqNewArray)
            {
                return Expression.NewArrayInit(
                    linqNewArray.Type.GetElementType(),
                    linqNewArray.Expressions.Select(ConvertExp));
            }

            private Expression Convert(LinqExp.ConditionalExpression linqConditional)
            {
                return Expression.Condition(
                    ConvertExp(linqConditional.Test),
                    ConvertExp(linqConditional.IfTrue),
                    ConvertExp(linqConditional.IfFalse));
            }

            private MemberInitExpression Convert(LinqExp.MemberInitExpression linqMemberInit)
            {
                return Expression.MemberInit(
                    Convert(linqMemberInit.NewExpression),
                    linqMemberInit.Bindings.Select(Convert));
            }

            private MemberBinding Convert(LinqExp.MemberBinding linqBinding)
            {
                switch (linqBinding.BindingType)
                {
                    //case LinqExp.MemberBindingType.Assignment:
                        
                    case LinqExp.MemberBindingType.MemberBinding:
                        var linqMemberBinding = (LinqExp.MemberMemberBinding)linqBinding;

                        return Expression.MemberBind(
                            linqMemberBinding.Member,
                            linqMemberBinding.Bindings.Select(Convert));

                    //case LinqExp.MemberBindingType.ListBinding:
                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private Expression ConvertConversion(LinqExp.UnaryExpression linqConvert)
            {
                return Expression.Convert(
                    ConvertExp(linqConvert.Operand),
                    linqConvert.Type,
                    linqConvert.Method);
            }

            private Expression Convert(LinqExp.UnaryExpression linqUnary, Func<Expression, Expression> factory)
            {
                return factory.Invoke(ConvertExp(linqUnary.Operand));
            }

            private Expression Convert(LinqExp.BinaryExpression linqBinary, Func<Expression, Expression, Expression> factory)
            {
                return factory.Invoke(ConvertExp(linqBinary.Left), ConvertExp(linqBinary.Right));
            }

            private static Expression Convert(LinqExp.ConstantExpression linqConstant)
                => Expression.Constant(linqConstant.Value, linqConstant.Type);

            private Expression Convert(LinqExp.MethodCallExpression linqCall)
            {
                return Expression.Call(
                    ConvertExp(linqCall.Object),
                    linqCall.Method,
                    linqCall.Arguments.Select(ConvertExp));
            }

            private Expression Convert(LinqExp.MemberExpression linqMemberAccess)
            {
                return Expression.MakeMemberAccess(
                    ConvertExp(linqMemberAccess.Expression),
                    linqMemberAccess.Member);
            }

            private Expression Convert(LinqExp.LambdaExpression linqLambda)
            {
                return Expression.Lambda(
                    ConvertExp(linqLambda.Body),
                    linqLambda.Parameters.Select(Convert));
            }

            private ParameterExpression Convert(LinqExp.ParameterExpression linqParam)
            {
                if (_parameters.TryGetValue(linqParam, out var param))
                {
                    return param;
                }

                return _parameters[linqParam] = Expression.Parameter(linqParam.Type, linqParam.Name);
            }
        }
    }
}
#endif