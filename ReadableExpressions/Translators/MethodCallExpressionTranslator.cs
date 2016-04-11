namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Extensions;

    internal class MethodCallExpressionTranslator : ExpressionTranslatorBase
    {
        #region Special Cases

        private readonly SpecialCaseHandlerBase[] _specialCaseHandlers;

        #endregion

        internal MethodCallExpressionTranslator(
            IndexAccessExpressionTranslator indexAccessTranslator,
            Func<Expression, TranslationContext, string> globalTranslator)
            : base(globalTranslator, ExpressionType.Call, ExpressionType.Invoke)
        {
            _specialCaseHandlers = new SpecialCaseHandlerBase[]
            {
                new InvocationExpressionHandler(GetMethodCall, globalTranslator),
                new IndexedPropertyHandler(indexAccessTranslator)
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var specialCaseHandler = _specialCaseHandlers.FirstOrDefault(sch => sch.AppliesTo(expression));

            if (specialCaseHandler != null)
            {
                return specialCaseHandler.Translate(expression, context);
            }

            var methodCall = (MethodCallExpression)expression;
            IEnumerable<Expression> methodArguments;
            var methodCallSubject = GetMethodCallSubject(methodCall, context, out methodArguments);

            return GetMethodCall(methodCallSubject, methodCall.Method, methodArguments, context);
        }

        private string GetMethodCallSubject(
            MethodCallExpression methodCall,
            TranslationContext context,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Object == null)
            {
                return GetStaticMethodCallSubject(methodCall, context, out arguments);
            }

            arguments = methodCall.Arguments;

            return GetTranslation(methodCall.Object, context);
        }

        private string GetStaticMethodCallSubject(
            MethodCallExpression methodCall,
            TranslationContext context,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Method.GetCustomAttributes(typeof(ExtensionAttribute), inherit: false).Any())
            {
                var subject = methodCall.Arguments.First();
                arguments = methodCall.Arguments.Skip(1);

                return GetTranslation(subject, context);
            }

            arguments = methodCall.Arguments;

            // ReSharper disable once PossibleNullReferenceException
            return methodCall.Method.DeclaringType.GetFriendlyName();
        }

        private string GetMethodCall(
            string subject,
            MethodInfo method,
            IEnumerable<Expression> parameters,
            TranslationContext context)
        {
            return GetMethodCall(subject, new BclMethodInfoWrapper(method), parameters, context);
        }

        internal string GetMethodCall(
            string subject,
            IMethodInfo method,
            IEnumerable<Expression> parameters,
            TranslationContext context)
        {
            return subject + "." + GetMethodCall(method, parameters, context);
        }

        internal string GetMethodCall(
            MethodInfo method,
            IEnumerable<Expression> parameters,
            TranslationContext context)
        {
            return GetMethodCall(new BclMethodInfoWrapper(method), parameters, context);
        }

        private string GetMethodCall(
            IMethodInfo method,
            IEnumerable<Expression> parameters,
            TranslationContext context)
        {
            var parametersString = GetTranslatedParameters(parameters, context, method).WithBrackets();
            var genericArguments = GetGenericArgumentsIfNecessary(method);

            return method.Name + genericArguments + parametersString;
        }

        private static string GetGenericArgumentsIfNecessary(IMethodInfo method)
        {
            if (!method.IsGenericMethod)
            {
                return null;
            }

            var methodGenericDefinition = method.GetGenericMethodDefinition();
            var genericParameterTypes = methodGenericDefinition.GetGenericArguments().ToList();

            RemoveSpecifiedGenericTypeParameters(
                methodGenericDefinition.GetParameters().Select(p => p.ParameterType),
                genericParameterTypes);

            if (!genericParameterTypes.Any())
            {
                return null;
            }

            var argumentNames = method
                .GetGenericArguments()
                .Select(a => a.GetFriendlyName());

            return $"<{string.Join(", ", argumentNames)}>";
        }

        private static void RemoveSpecifiedGenericTypeParameters(
            IEnumerable<Type> types,
            ICollection<Type> genericParameterTypes)
        {
            foreach (var type in types)
            {
                if (type.IsGenericParameter && genericParameterTypes.Contains(type))
                {
                    genericParameterTypes.Remove(type);
                }

                if (type.IsGenericType)
                {
                    RemoveSpecifiedGenericTypeParameters(type.GetGenericArguments(), genericParameterTypes);
                }
            }
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

            public abstract string Translate(Expression expression, TranslationContext context);
        }

        private class InvocationExpressionHandler : SpecialCaseHandlerBase
        {
            private readonly Func<string, MethodInfo, IEnumerable<Expression>, TranslationContext, string> _methodCallTranslator;
            private readonly Func<Expression, TranslationContext, string> _translator;

            public InvocationExpressionHandler(
                Func<string, MethodInfo, IEnumerable<Expression>, TranslationContext, string> methodCallTranslator,
                Func<Expression, TranslationContext, string> translator)
                : base(exp => exp.NodeType == ExpressionType.Invoke)
            {
                _methodCallTranslator = methodCallTranslator;
                _translator = translator;
            }

            public override string Translate(Expression expression, TranslationContext context)
            {
                var invocation = (InvocationExpression)expression;
                var invocationSubject = _translator.Invoke(invocation.Expression, context);

                if (invocation.Expression.NodeType == ExpressionType.Lambda)
                {
                    invocationSubject = $"({invocationSubject})";
                }

                var invocationMethod = invocation.Expression.Type.GetMethod("Invoke");

                return _methodCallTranslator
                    .Invoke(invocationSubject, invocationMethod, invocation.Arguments, context);
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

            public override string Translate(Expression expression, TranslationContext context)
            {
                var methodCall = (MethodCallExpression)expression;

                return _indexAccessTranslator.TranslateIndexAccess(
                    methodCall.Object,
                    methodCall.Arguments,
                    context);
            }
        }

        #endregion
    }
}