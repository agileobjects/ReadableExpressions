namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Extensions;
    using Formatting;

    internal class MethodCallExpressionTranslator : ExpressionTranslatorBase
    {
        #region Special Cases

        private readonly SpecialCaseHandlerBase[] _specialCaseHandlers;

        #endregion

        internal MethodCallExpressionTranslator(
            IndexAccessExpressionTranslator indexAccessTranslator,
            IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Call, ExpressionType.Invoke)
        {
            _specialCaseHandlers = new SpecialCaseHandlerBase[]
            {
                new InvocationExpressionHandler(GetMethodCall, registry),
                new IndexedPropertyHandler(indexAccessTranslator)
            };
        }

        public override string Translate(Expression expression)
        {
            var specialCaseHandler = _specialCaseHandlers.FirstOrDefault(sch => sch.AppliesTo(expression));

            if (specialCaseHandler != null)
            {
                return specialCaseHandler.Translate(expression);
            }

            var methodCall = (MethodCallExpression)expression;
            IEnumerable<Expression> methodArguments;
            var methodCallSubject = GetMethodCallSuject(methodCall, out methodArguments);

            return GetMethodCall(methodCallSubject, methodCall.Method.Name, methodArguments);
        }

        private string GetMethodCallSuject(
            MethodCallExpression methodCall,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Object == null)
            {
                return GetStaticMethodCallSubject(methodCall, out arguments);
            }

            arguments = methodCall.Arguments;

            return Registry.Translate(methodCall.Object);
        }

        private string GetStaticMethodCallSubject(
            MethodCallExpression methodCall,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Method.GetCustomAttributes(typeof(ExtensionAttribute), inherit: false).Any())
            {
                var subject = methodCall.Arguments.First();
                arguments = methodCall.Arguments.Skip(1);

                return Registry.Translate(subject);
            }

            arguments = methodCall.Arguments;

            // ReSharper disable once PossibleNullReferenceException
            return methodCall.Method.DeclaringType.GetFriendlyName();
        }

        internal string GetMethodCall(MethodInfo method, IEnumerable<Expression> parameters)
        {
            return GetMethodCall(method.Name, parameters);
        }

        internal string GetMethodCall(string subject, string methodName, IEnumerable<Expression> parameters)
        {
            return subject + "." + GetMethodCall(methodName, parameters);
        }

        internal string GetMethodCall(string methodName, IEnumerable<Expression> parameters)
        {
            var parametersString = Registry
                .TranslateParameters(parameters)
                .WithBrackets();

            return methodName + parametersString;
        }

        #region Helper Classes

        private abstract class SpecialCaseHandlerBase
        {
            private readonly Func<Expression, bool> _applicabilityTester;

            protected SpecialCaseHandlerBase(Func<Expression, bool> applicabilityTester)
            {
                _applicabilityTester = applicabilityTester;
            }

            public bool AppliesTo(Expression expression)
            {
                return _applicabilityTester.Invoke(expression);
            }

            public abstract string Translate(Expression expression);
        }

        private class InvocationExpressionHandler : SpecialCaseHandlerBase
        {
            private readonly Func<string, IEnumerable<Expression>, string> _methodCallTranslator;
            private readonly IExpressionTranslatorRegistry _registry;

            public InvocationExpressionHandler(
                Func<string, IEnumerable<Expression>, string> methodCallTranslator,
                IExpressionTranslatorRegistry registry)
                : base(exp => exp.NodeType == ExpressionType.Invoke)
            {
                _methodCallTranslator = methodCallTranslator;
                _registry = registry;
            }

            public override string Translate(Expression expression)
            {
                var invocation = (InvocationExpression)expression;
                var invocationSubject = _registry.Translate(invocation.Expression);

                if (invocation.Expression.NodeType == ExpressionType.Lambda)
                {
                    invocationSubject = $"({invocationSubject})";
                }

                return invocationSubject + "." + _methodCallTranslator.Invoke("Invoke", invocation.Arguments);
            }
        }

        private class IndexedPropertyHandler : SpecialCaseHandlerBase
        {
            private readonly IndexAccessExpressionTranslator _indexAccessTranslator;

            public IndexedPropertyHandler(IndexAccessExpressionTranslator indexAccessTranslator)
                : base(IsIndexedPropertyAccess)
            {
                _indexAccessTranslator = indexAccessTranslator;
            }

            private static bool IsIndexedPropertyAccess(Expression expression)
            {
                var methodCall = (MethodCallExpression)expression;

                var property = methodCall
                    .Object?
                    .Type
                    .GetProperties()
                    .FirstOrDefault(p => p.GetAccessors().Contains(methodCall.Method));

                if (property == null)
                {
                    return false;
                }

                var propertyIndexParameters = property.GetIndexParameters();

                return propertyIndexParameters.Any();
            }

            public override string Translate(Expression expression)
            {
                var methodCall = (MethodCallExpression)expression;

                return _indexAccessTranslator
                    .TranslateIndexAccess(methodCall.Object, methodCall.Arguments);
            }
        }

        #endregion
    }
}