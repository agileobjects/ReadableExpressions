namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Linq;
    using System.Linq.Expressions;

    internal static class InternalExpressionExtensions
    {
        public static Expression GetSubject(this MethodCallExpression methodCall)
        {
            return methodCall.Method.IsExtensionMethod()
                ? methodCall.Arguments.First() : methodCall.Object;
        }

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

                case ExpressionType.Call:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Invoke:
                case ExpressionType.MemberAccess:
                case ExpressionType.Parameter:
                    return true;
            }

            return false;
        }

        public static bool IsReturnable(this BlockExpression block)
        {
            return (block.Type != typeof(void)) && block.Result.IsReturnable();
        }
    }
}