namespace AgileObjects.ReadableExpressions.Extensions
{
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides a set of static extension methods for Expression information.
    /// </summary>
    public static class PublicExpressionExtensions
    {
        /// <summary>
        /// Returns the Expression representing the subject of the given <paramref name="methodCall"/>.
        /// </summary>
        /// <param name="methodCall">
        /// The Expression representing the method call the subject of which should be retrieved.
        /// </param>
        /// <returns>
        /// The Expression representing the subject of the given <paramref name="methodCall"/>.
        /// </returns>
        public static Expression GetSubject(this MethodCallExpression methodCall)
        {
            return methodCall.Method.IsExtensionMethod()
                ? methodCall.Arguments.First() : methodCall.Object;
        }

        /// <summary>
        /// Gets the parent Expression for the given <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The Expression for which to retrieve the parent.</param>
        /// <returns>The given <paramref name="expression"/>'s parent Expression.</returns>
        public static Expression GetParentOrNull(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).GetSubject();

                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Expression;
            }

            return null;
        }
    }
}