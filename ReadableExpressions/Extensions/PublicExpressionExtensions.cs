namespace AgileObjects.ReadableExpressions.Extensions
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Collections.Generic;
    using NetStandardPolyfills;

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
                case ExpressionType.ArrayIndex:
                    return ((BinaryExpression)expression).Left;

                case ExpressionType.Index:
                    return ((IndexExpression)expression).Object;

                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).GetSubject();

                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Expression;
            }

            return null;
        }

        /// <summary>
        /// Determines if this <paramref name="expression"/> represents a <see cref="Comment"/>.
        /// </summary>
        /// <param name="expression">The Expression for which to make the determination.</param>
        /// <returns>
        /// Trueif this <paramref name="expression"/> represents a <see cref="Comment"/>, otherwise
        /// false.
        /// </returns>
        public static bool IsComment(this Expression expression)
        {
            return expression.NodeType == ExpressionType.Constant &&
                   expression.Type == typeof(Comment);
        }

        /// <summary>
        /// Creates a new LambdaExpression which is like this one, but using the supplied children.
        /// If all of the children are the same, it will return this expression.
        /// </summary>
        /// <param name="lambda">The LambdaExpression to update or return.</param>
        /// <param name="body">The body of the LambdaExpression.</param>
        /// <param name="parameters">The LambdaExpression's parameters.</param>
        /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
        public static LambdaExpression Update(
            this LambdaExpression lambda,
            Expression body,
            IEnumerable<ParameterExpression> parameters)
        {
            var parameterList = parameters.ToList();

            if (body == lambda.Body && lambda.Parameters.SequenceEqual(parameterList))
            {
                return lambda;
            }

            var parameterCount = parameterList.Count;
            var parameterTypes = parameterList.ProjectToArray(p => p.Type);

            Type[] lambdaTypes;
            Func<Type[], Type> lambdaTypeFactory;

            if (lambda.ReturnType != typeof(void))
            {
                lambdaTypeFactory = Expression.GetFuncType;
                lambdaTypes = new Type[parameterCount + 1];
                parameterTypes.CopyTo(lambdaTypes, 0);
                lambdaTypes[parameterCount] = lambda.ReturnType;
            }
            else
            {
                lambdaTypeFactory = Expression.GetActionType;
                lambdaTypes = parameterTypes;
            }

            return Expression.Lambda(
                lambdaTypeFactory.Invoke(lambdaTypes),
                body,
                parameterList);
        }
    }
}