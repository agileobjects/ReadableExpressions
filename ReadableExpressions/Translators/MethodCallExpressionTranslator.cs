namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class MethodCallExpressionTranslator : ExpressionTranslatorBase
    {
        #region Special Cases

        private static readonly SpecialCaseHandlerBase[] _specialCaseHandlers =
        {
            new InvocationExpressionHandler(),
            new CollectionIndexerHandler()
        };

        #endregion

        internal MethodCallExpressionTranslator()
            : base(ExpressionType.Call, ExpressionType.Invoke)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var specialCaseHandler = _specialCaseHandlers.FirstOrDefault(sch => sch.AppliesTo(expression));

            if (specialCaseHandler != null)
            {
                return specialCaseHandler.Translate(expression, translatorRegistry);
            }

            var methodCall = (MethodCallExpression)expression;
            IEnumerable<Expression> methodArguments;
            var methodCallSubject = GetMethodCallSuject(methodCall, translatorRegistry, out methodArguments);

            return methodCallSubject + "." + GetMethodCall(methodCall.Method, methodArguments, translatorRegistry);
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

        #region Helper Classes

        private abstract class SpecialCaseHandlerBase
        {
            private readonly Func<Expression, bool> _applicabilityTester;
            private readonly Func<Expression, IExpressionTranslatorRegistry, string> _translator;

            protected SpecialCaseHandlerBase(
                Func<Expression, bool> applicabilityTester,
                Func<Expression, IExpressionTranslatorRegistry, string> translator)
            {
                _applicabilityTester = applicabilityTester;
                _translator = translator;
            }

            public bool AppliesTo(Expression expression)
            {
                return _applicabilityTester.Invoke(expression);
            }

            public string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
            {
                return _translator.Invoke(expression, translatorRegistry);
            }
        }

        private class InvocationExpressionHandler : SpecialCaseHandlerBase
        {
            public InvocationExpressionHandler()
                : base(exp => exp.NodeType == ExpressionType.Invoke, GetInvocation)
            {
            }

            private static string GetInvocation(
                Expression expression,
                IExpressionTranslatorRegistry translatorRegistry)
            {
                var invocation = (InvocationExpression)expression;
                var invocationSubject = translatorRegistry.Translate(invocation.Expression);

                if (invocation.Expression.NodeType == ExpressionType.Lambda)
                {
                    invocationSubject = $"({invocationSubject})";
                }

                return invocationSubject + "." + GetMethodCall("Invoke", invocation.Arguments, translatorRegistry);
            }
        }

        private class CollectionIndexerHandler : SpecialCaseHandlerBase
        {
            private static readonly Type[] _collectionTypes =
            {
                typeof(IDictionary<,>), typeof(Dictionary<,>),
                typeof(Collection<>),
                typeof(IList<>), typeof(List<>)
            };

            public CollectionIndexerHandler()
                : base(IsCollectionIndexer, GetIndexerAccess)
            {
            }

            private static bool IsCollectionIndexer(Expression expression)
            {
                var methodCall = (MethodCallExpression)expression;

                if (methodCall.Object == null)
                {
                    return false;
                }

                if (!methodCall.Object.Type.IsGenericType)
                {
                    return false;
                }

                if ((methodCall.Method.Name != "get_Item") && (methodCall.Method.Name != "set_Item"))
                {
                    return false;
                }

                var genericTypeDefinition = methodCall.Object.Type.GetGenericTypeDefinition();

                return _collectionTypes.Contains(genericTypeDefinition);
            }

            private static string GetIndexerAccess(
                Expression expression,
                IExpressionTranslatorRegistry translatorRegistry)
            {
                var methodCall = (MethodCallExpression)expression;

                return IndexAccessExpressionTranslator.TranslateIndexAccess(
                    methodCall.Object,
                    methodCall.Arguments.First(),
                    translatorRegistry);
            }
        }

        #endregion
    }
}