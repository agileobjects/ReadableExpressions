namespace AgileObjects.ReadableExpressions.Build.SourceCode
{
    using System;
    using System.Collections.Generic;
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
            this Expression expression,
            IList<ParameterExpression> parameters = null)
        {
            if (expression.NodeType == ExpressionType.Lambda)
            {
                return (LambdaExpression)expression;
            }

            var isAction = !expression.HasReturnType();

            Type lambdaType;

            if (parameters == null)
            {
                lambdaType = isAction
                    ? GetActionType()
                    : GetFuncType(expression.Type);

                return Lambda(lambdaType, expression);
            }

            var parameterCount = parameters.Count;

            Type[] lambdaParameterTypes;

            if (isAction)
            {
                lambdaParameterTypes = parameters.ProjectToArray(p => p.Type);
                lambdaType = GetActionType(lambdaParameterTypes);

                return Lambda(lambdaType, expression, parameters);
            }

            lambdaParameterTypes = new Type[parameterCount + 1];

            for (var i = 0; ;)
            {
                lambdaParameterTypes[i] = parameters[i].Type;

                ++i;

                if (i != parameterCount)
                {
                    continue;
                }

                lambdaParameterTypes[parameterCount] = expression.Type;
                lambdaType = GetFuncType(lambdaParameterTypes);

                return Lambda(lambdaType, expression, parameters);
            }
        }
    }
}