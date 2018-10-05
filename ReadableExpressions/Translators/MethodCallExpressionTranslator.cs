namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using InvocationExpression = Microsoft.Scripting.Ast.InvocationExpression;
    using MethodCallExpression = Microsoft.Scripting.Ast.MethodCallExpression;
#endif
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    internal struct MethodCallExpressionTranslator : IExpressionTranslator
    {
        private static readonly SpecialCaseHandlerBase[] _specialCaseHandlers =
        {
            new InvocationExpressionHandler(GetMethodCall),
            new StringConcatenationHandler(),
            new IndexedPropertyHandler(),
            new ImplicitOperatorHandler(),
            new ExplicitOperatorHandler()
        };

        public IEnumerable<ExpressionType> NodeTypes
        {
            get
            {
                yield return ExpressionType.Call;
                yield return ExpressionType.Invoke;
            }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var specialCaseHandler = _specialCaseHandlers.FirstOrDefault(sch => sch.AppliesTo(expression));

            if (specialCaseHandler != null)
            {
                return specialCaseHandler.Translate(expression, context);
            }

            var methodCall = (MethodCallExpression)expression;
            var methodCallSubject = GetMethodCallSubject(methodCall, context, out var methodArguments);

            return GetMethodCall(
                methodCallSubject,
                methodCall.Method,
                methodArguments,
                expression,
                context);
        }

        public static string GetMethodCallSubject(MethodCallExpression methodCall, TranslationContext context)
        {
            // ReSharper disable once UnusedVariable
            return GetMethodCallSubject(methodCall, context, out var arguments);
        }

        private static string GetMethodCallSubject(
            MethodCallExpression methodCall,
            TranslationContext context,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Object == null)
            {
                return GetStaticMethodCallSubject(methodCall, context, out arguments);
            }

            arguments = methodCall.Arguments;

            return context.Translate(methodCall.Object);
        }

        private static string GetStaticMethodCallSubject(
            MethodCallExpression methodCall,
            TranslationContext context,
            out IEnumerable<Expression> arguments)
        {
            if (methodCall.Method.IsExtensionMethod())
            {
                var subject = methodCall.Arguments.First();
                arguments = methodCall.Arguments.Skip(1).ToArray();

                return context.Translate(subject);
            }

            arguments = methodCall.Arguments;

            // ReSharper disable once PossibleNullReferenceException
            return methodCall.Method.DeclaringType.GetFriendlyName(context.Settings);
        }

        private static string GetMethodCall(
            string subject,
            MethodInfo method,
            IEnumerable<Expression> arguments,
            Expression originalMethodCall,
            TranslationContext context)
        {
            return GetMethodCall(
                subject,
                new BclMethodWrapper(method),
                arguments,
                originalMethodCall,
                context);
        }

        internal static string GetMethodCall(
            string subject,
            IMethod method,
            IEnumerable<Expression> arguments,
            Expression originalMethodCall,
            TranslationContext context)
        {
            var separator = context.IsPartOfMethodCallChain(originalMethodCall)
                ? Environment.NewLine + ".".Indented()
                : ".";

            return subject + separator + GetMethodCall(method, arguments, context);
        }

        internal static string GetMethodCall(
            IMethod method,
            IEnumerable<Expression> arguments,
            TranslationContext context)
        {
            var parametersString = context.TranslateParameters(arguments, method).WithParentheses();
            var genericArguments = GetGenericArgumentsIfNecessary(method, context);

            return method.Name + genericArguments + parametersString;
        }

        private static string GetGenericArgumentsIfNecessary(IMethod method, TranslationContext context)
        {
            if (!method.IsGenericMethod)
            {
                return null;
            }

            var methodGenericDefinition = method.GetGenericMethodDefinition();
            var genericParameterTypes = methodGenericDefinition.GetGenericArguments().ToList();

            if (context.Settings.UseImplicitGenericParameters)
            {
                RemoveSuppliedGenericTypeParameters(
                    methodGenericDefinition.GetParameters().Project(p => p.ParameterType),
                    genericParameterTypes);
            }

            if (!genericParameterTypes.Any())
            {
                return null;
            }

            var argumentNames = method
                .GetGenericArguments()
                .Project(a => a.GetFriendlyName(context.Settings))
                .Filter(name => name != null)
                .ToArray();

            return argumentNames.Any() ? $"<{string.Join(", ", argumentNames)}>" : null;
        }

        private static void RemoveSuppliedGenericTypeParameters(
            IEnumerable<Type> types,
            ICollection<Type> genericParameterTypes)
        {
            foreach (var type in types.Project(t => t.IsByRef ? t.GetElementType() : t))
            {
                if (type.IsGenericParameter && genericParameterTypes.Contains(type))
                {
                    genericParameterTypes.Remove(type);
                }

                if (type.IsGenericType())
                {
                    RemoveSuppliedGenericTypeParameters(type.GetGenericTypeArguments(), genericParameterTypes);
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

        private class StringConcatenationHandler : SpecialCaseHandlerBase
        {
            public StringConcatenationHandler()
                : base(IsStringConcatCall)
            {
            }

            private static bool IsStringConcatCall(Expression expression)
            {
                var method = ((MethodCallExpression)expression).Method;

                return method.IsStatic &&
                      (method.DeclaringType == typeof(string)) &&
                      (method.Name == "Concat");
            }

            public override string Translate(Expression expression, TranslationContext context)
            {
                var methodCall = (MethodCallExpression)expression;

                return methodCall.Arguments.ToStringConcatenation(context);
            }
        }

        private class InvocationExpressionHandler : SpecialCaseHandlerBase
        {
            public delegate string MethodCallTranslator(
                string subject,
                MethodInfo method,
                IEnumerable<Expression> arguments,
                Expression originalMethodCall,
                TranslationContext context);

            private readonly MethodCallTranslator _methodCallTranslator;

            public InvocationExpressionHandler(MethodCallTranslator methodCallTranslator)
                : base(exp => exp.NodeType == ExpressionType.Invoke)
            {
                _methodCallTranslator = methodCallTranslator;
            }

            public override string Translate(Expression expression, TranslationContext context)
            {
                var invocation = (InvocationExpression)expression;
                var invocationSubject = context.Translate(invocation.Expression);

                if (invocation.Expression.NodeType == ExpressionType.Lambda)
                {
                    invocationSubject = invocationSubject.WithSurroundingParentheses();
                }

                var invocationMethod = invocation.Expression.Type.GetPublicInstanceMethod("Invoke");

                return _methodCallTranslator.Invoke(
                    invocationSubject,
                    invocationMethod,
                    invocation.Arguments,
                    expression,
                    context);
            }
        }

        private class IndexedPropertyHandler : SpecialCaseHandlerBase
        {
            public IndexedPropertyHandler()
                : base(IsIndexedPropertyAccess)
            {
            }

            private static bool IsIndexedPropertyAccess(Expression expression)
            {
                var methodCall = (MethodCallExpression)expression;

                var property = methodCall
                    .Object?
                    .Type
                    .GetPublicInstanceProperties()
                    .FirstOrDefault(p => p.IsIndexer() && p.GetAccessors().Contains(methodCall.Method));

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

                return IndexAccessExpressionTranslator.TranslateIndexAccess(
                    methodCall.Object,
                    methodCall.Arguments,
                    context);
            }
        }

        private class ImplicitOperatorHandler : SpecialCaseHandlerBase
        {
            public ImplicitOperatorHandler()
                : base(IsImplicitOperatorCall)
            {
            }

            private static bool IsImplicitOperatorCall(Expression expression)
                => ((MethodCallExpression)expression).Method.IsImplicitOperator();

            public override string Translate(Expression expression, TranslationContext context)
            {
                var methodCall = (MethodCallExpression)expression;

                return context.TranslateAsCodeBlock(methodCall.Arguments.First());
            }
        }

        private class ExplicitOperatorHandler : SpecialCaseHandlerBase
        {
            public ExplicitOperatorHandler()
                : base(IsExplicitOperatorCall)
            {
            }

            private static bool IsExplicitOperatorCall(Expression expression)
                => ((MethodCallExpression)expression).Method.IsExplicitOperator();

            public override string Translate(Expression expression, TranslationContext context)
            {
                var methodCall = (MethodCallExpression)expression;

                return CastExpressionTranslator.Translate(
                    methodCall.Arguments.First(),
                    methodCall.Method,
                    expression.Type,
                    context);
            }
        }

        #endregion
    }
}