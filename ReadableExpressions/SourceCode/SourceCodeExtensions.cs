namespace AgileObjects.ReadableExpressions.SourceCode
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
#if NET35
    using static Microsoft.Scripting.Ast.Expression;
#else
    using static System.Linq.Expressions.Expression;
#endif

    internal static class SourceCodeExtensions
    {
        public static LambdaExpression ToLambdaExpression(
            this Expression expression)
        {
            if (expression.NodeType == ExpressionType.Lambda)
            {
                return (LambdaExpression)expression;
            }

            var lambdaType = expression.HasReturnType()
                ? GetFuncType(expression.Type)
                : GetActionType();

            return Lambda(lambdaType, expression);
        }
    }
}