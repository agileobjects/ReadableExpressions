#if NET35
namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Scripting.Ast;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using LinqExp = System.Linq.Expressions;

    /// <summary>
    /// Converts a .NET 3.5 Linq Expression object into a DynamicLanguageRuntime Expression object.
    /// </summary>
    public static class LinqExpressionToDlrExpressionConverter
    {
        /// <summary>
        /// Converts the given <paramref name="linqLambda"/> into a DynamicLanguageRuntime Lambda Expression.
        /// </summary>
        /// <param name="linqLambda">The Linq Lambda Expression to convert.</param>
        /// <returns>The given <paramref name="linqLambda"/> converted into a DynamicLanguageRuntime Lambda Expression.</returns>
        public static LambdaExpression Convert(LinqExp.LambdaExpression linqLambda)
            => (LambdaExpression)new Converter().ConvertExp(linqLambda);

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
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.And);

                    case LinqExp.ExpressionType.AndAlso:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.AndAlso);

                    case LinqExp.ExpressionType.ArrayLength:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.ArrayLength);

                    case LinqExp.ExpressionType.ArrayIndex:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.ArrayIndex);

                    case LinqExp.ExpressionType.Call:
                        return Convert((LinqExp.MethodCallExpression)linqExpression);

                    case LinqExp.ExpressionType.Coalesce:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.Coalesce);

                    case LinqExp.ExpressionType.Conditional:
                        return Convert((LinqExp.ConditionalExpression)linqExpression);

                    case LinqExp.ExpressionType.Constant:
                        return Convert((LinqExp.ConstantExpression)linqExpression);

                    case LinqExp.ExpressionType.Convert:
                        return ConvertCast((LinqExp.UnaryExpression)linqExpression, Expression.Convert);

                    case LinqExp.ExpressionType.ConvertChecked:
                        return ConvertCast((LinqExp.UnaryExpression)linqExpression, Expression.ConvertChecked);

                    case LinqExp.ExpressionType.Divide:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Divide);

                    case LinqExp.ExpressionType.Equal:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.Equal);

                    case LinqExp.ExpressionType.ExclusiveOr:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.ExclusiveOr);

                    case LinqExp.ExpressionType.GreaterThan:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.GreaterThan);

                    case LinqExp.ExpressionType.GreaterThanOrEqual:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.GreaterThanOrEqual);

                    case LinqExp.ExpressionType.Invoke:
                        return Convert((LinqExp.InvocationExpression)linqExpression);

                    case LinqExp.ExpressionType.Lambda:
                        return ConvertLambda((LinqExp.LambdaExpression)linqExpression);

                    case LinqExp.ExpressionType.LeftShift:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.LeftShift);

                    case LinqExp.ExpressionType.LessThan:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.LessThan);

                    case LinqExp.ExpressionType.LessThanOrEqual:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.LessThanOrEqual);

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
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.MultiplyChecked);

                    case LinqExp.ExpressionType.Negate:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.Negate);

                    case LinqExp.ExpressionType.UnaryPlus:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.NegateChecked);

                    case LinqExp.ExpressionType.NegateChecked:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.NegateChecked);

                    case LinqExp.ExpressionType.New:
                        return Convert((LinqExp.NewExpression)linqExpression);

                    case LinqExp.ExpressionType.NewArrayBounds:
                        return Convert((LinqExp.NewArrayExpression)linqExpression, Expression.NewArrayBounds);

                    case LinqExp.ExpressionType.NewArrayInit:
                        return Convert((LinqExp.NewArrayExpression)linqExpression, Expression.NewArrayInit);

                    case LinqExp.ExpressionType.Not:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.Not);

                    case LinqExp.ExpressionType.NotEqual:
                        return ConvertImplicit((LinqExp.BinaryExpression)linqExpression, Expression.NotEqual);

                    case LinqExp.ExpressionType.Or:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Or);

                    case LinqExp.ExpressionType.OrElse:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.OrElse);

                    case LinqExp.ExpressionType.Parameter:
                        return Convert((LinqExp.ParameterExpression)linqExpression);

                    case LinqExp.ExpressionType.Power:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Power);

                    case LinqExp.ExpressionType.Quote:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.Quote);

                    case LinqExp.ExpressionType.RightShift:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.RightShift);

                    case LinqExp.ExpressionType.Subtract:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.Subtract);

                    case LinqExp.ExpressionType.SubtractChecked:
                        return Convert((LinqExp.BinaryExpression)linqExpression, Expression.SubtractChecked);

                    case LinqExp.ExpressionType.TypeAs:
                        return Convert((LinqExp.UnaryExpression)linqExpression, Expression.TypeAs);

                    case LinqExp.ExpressionType.TypeIs:
                        return Convert((LinqExp.TypeBinaryExpression)linqExpression);
                }

                throw new NotSupportedException("Can't convert a " + linqExpression.NodeType);
            }

            private Expression Convert(LinqExp.InvocationExpression linqInvoke)
            {
                return Expression.Invoke(
                    ConvertExp(linqInvoke.Expression),
                    linqInvoke.Arguments.Select(ConvertExp));
            }

            private Expression Convert(LinqExp.TypeBinaryExpression linqTypeBinary)
                => Expression.TypeIs(ConvertExp(linqTypeBinary.Expression), linqTypeBinary.TypeOperand);

            private Expression Convert(LinqExp.ListInitExpression linqListInit)
            {
                return Expression.ListInit(
                    Convert(linqListInit.NewExpression),
                    linqListInit.Initializers.Select(Convert));
            }

            private NewExpression Convert(LinqExp.NewExpression linqNew)
            {
                return (linqNew.Members != null)
                    ? Expression.New(
                          linqNew.Constructor,
                          linqNew.Arguments.Select(ConvertExp),
                          linqNew.Members)
                    : Expression.New(
                          linqNew.Constructor,
                          linqNew.Arguments.Select(ConvertExp));
            }

            private ElementInit Convert(LinqExp.ElementInit linqElementInit)
            {
                return Expression.ElementInit(
                    linqElementInit.AddMethod,
                    linqElementInit.Arguments.Select(ConvertExp));
            }

            private Expression Convert(LinqExp.NewArrayExpression linqNewArray, Func<Type, IEnumerable<Expression>, Expression> factory)
            {
                return factory.Invoke(
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
                    case LinqExp.MemberBindingType.Assignment:
                        var linqMemberAssignment = (LinqExp.MemberAssignment)linqBinding;

                        return Expression.Bind(
                            linqMemberAssignment.Member,
                            ConvertExp(linqMemberAssignment.Expression));

                    case LinqExp.MemberBindingType.MemberBinding:
                        var linqMemberBinding = (LinqExp.MemberMemberBinding)linqBinding;

                        return Expression.MemberBind(
                            linqMemberBinding.Member,
                            linqMemberBinding.Bindings.Select(Convert));

                    case LinqExp.MemberBindingType.ListBinding:
                        var linqListBinding = (LinqExp.MemberListBinding)linqBinding;

                        return Expression.ListBind(
                            linqListBinding.Member,
                            linqListBinding.Initializers.Select(Convert));

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private Expression ConvertCast(LinqExp.UnaryExpression linqConvert, Func<Expression, Type, MethodInfo, Expression> factory)
            {
                return factory.Invoke(
                    ConvertExp(linqConvert.Operand),
                    linqConvert.Type,
                    linqConvert.Method);
            }

            private Expression Convert(LinqExp.UnaryExpression linqUnary, Func<Expression, Expression> factory)
            {
                return factory.Invoke(ConvertExp(linqUnary.Operand));
            }

            private Expression Convert(LinqExp.UnaryExpression linqUnary, Func<Expression, Type, Expression> factory)
            {
                return factory.Invoke(ConvertExp(linqUnary.Operand), linqUnary.Type);
            }

            private Expression ConvertImplicit(LinqExp.BinaryExpression linqBinary, Func<Expression, Expression, Expression> factory)
            {
                return factory.Invoke(ConvertExp(linqBinary.Left), ConvertExp(linqBinary.Right));
            }

            private Expression Convert(
                LinqExp.BinaryExpression linqBinary,
                Func<Expression, Expression, MethodInfo, Expression> factory)
            {
                return factory.Invoke(
                    ConvertExp(linqBinary.Left),
                    ConvertExp(linqBinary.Right),
                    linqBinary.Method);
            }

            private static Expression Convert(LinqExp.ConstantExpression linqConstant)
                => Expression.Constant(linqConstant.Value, linqConstant.Type);

            private Expression Convert(LinqExp.MethodCallExpression linqCall)
            {
                var arguments = linqCall.Arguments.Select(arg =>
                    (arg.NodeType == LinqExp.ExpressionType.Quote)
                        ? Expression.Constant(((LinqExp.UnaryExpression)arg).Operand, arg.Type)
                        : ConvertExp(arg));

                return Expression.Call(
                    ConvertExp(linqCall.Object),
                    linqCall.Method,
                    arguments);
            }

            private Expression Convert(LinqExp.MemberExpression linqMemberAccess)
            {
                return Expression.MakeMemberAccess(
                    ConvertExp(linqMemberAccess.Expression),
                    linqMemberAccess.Member);
            }

            private Expression ConvertLambda(LinqExp.LambdaExpression linqLambda)
            {
                return Expression.Lambda(
                    linqLambda.Type,
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