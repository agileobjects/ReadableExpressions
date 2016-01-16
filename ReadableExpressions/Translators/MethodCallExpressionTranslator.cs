namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class MethodCallExpressionTranslator : ExpressionTranslatorBase
    {
        internal MethodCallExpressionTranslator()
            : base(ExpressionType.Call, ExpressionType.Invoke)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            if (expression.NodeType == ExpressionType.Invoke)
            {
                return GetInvocation((InvocationExpression)expression, translatorRegistry);
            }

            var methodCall = (MethodCallExpression)expression;
            IEnumerable<Expression> methodArguments;
            var methodCallSubject = GetMethodCallSuject(methodCall, translatorRegistry, out methodArguments);

            return methodCallSubject + "." + GetMethodCall(methodCall.Method, methodArguments, translatorRegistry);
        }

        private static string GetInvocation(
            InvocationExpression invocation,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var invocationSubject = translatorRegistry.Translate(invocation.Expression);

            if (invocation.Expression.NodeType == ExpressionType.Lambda)
            {
                invocationSubject = $"({invocationSubject})";
            }

            return invocationSubject + "." + GetMethodCall("Invoke", invocation.Arguments, translatorRegistry);
        }

        private static string GetMethodCallSuject(
            MethodCallExpression methodCall,
            IExpressionTranslatorRegistry translatorRegistry,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Object != null)
            {
                arguments = methodCall.Arguments;

                return translatorRegistry.Translate(methodCall.Object);
            }

            return GetStaticMethodCallSubject(methodCall, translatorRegistry, out arguments);
        }

        private static string GetStaticMethodCallSubject(
            MethodCallExpression methodCall,
            IExpressionTranslatorRegistry translatorRegistry,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Method.GetCustomAttributes(typeof(ExtensionAttribute), inherit: false).Any())
            {
                var subject = methodCall.Arguments.First();
                arguments = methodCall.Arguments.Skip(1);

                return translatorRegistry.Translate(subject);
            }

            arguments = methodCall.Arguments;

            // ReSharper disable once PossibleNullReferenceException
            return methodCall.Method.DeclaringType.Name;
        }

        internal static string GetMethodCall(
            MethodInfo method,
            IEnumerable<Expression> parameters,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            return GetMethodCall(method.Name, parameters, translatorRegistry);
        }

        private static string GetMethodCall(
            string methodName,
            IEnumerable<Expression> parameters,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var parametersString = TranslationHelper.GetParameters(
                parameters,
                translatorRegistry,
                placeLongListsOnMultipleLines: true,
                encloseSingleParameterInBrackets: true);

            return methodName + parametersString;
        }
    }
}