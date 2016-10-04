namespace AgileObjects.ReadableExpressions.Translators
{
#if !NET_STANDARD
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;

    internal class DynamicExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly IEnumerable<DynamicOperationTranslatorBase> _translators;

        public DynamicExpressionTranslator(
            MemberAccessExpressionTranslator memberAccessTranslator,
            AssignmentExpressionTranslator assignmentTranslator,
            MethodCallExpressionTranslator methodCallTranslator)
            : base(ExpressionType.Dynamic)
        {
            var dynamicMemberAccessTranslator = new DynamicMemberAccessTranslator(memberAccessTranslator);

            _translators = new DynamicOperationTranslatorBase[]
            {
                dynamicMemberAccessTranslator,
                new DynamicMemberWriteTranslator(dynamicMemberAccessTranslator, assignmentTranslator),
                new DynamicMethodCallTranslator(methodCallTranslator)
            };
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var dynamicExpression = (DynamicExpression)expression;

            var operationDescription = dynamicExpression.ToString();

            // DynamicExpressions are created with CallSiteBinders which are created
            // via language-specific assemblies (e.g. Microsoft.CSharp) to which this 
            // assembly has no access; translating the descriptions provided by the 
            // ToStrings of these assemblies (with a fallback) seems like the easiest 
            // way to go.
            foreach (var translator in _translators)
            {
                string translated;

                if (translator.TryTranslate(
                    operationDescription,
                    dynamicExpression,
                    context,
                    out translated))
                {
                    return translated;
                }
            }

            return operationDescription;
        }

        #region Helper Classes

        private abstract class DynamicOperationTranslatorBase
        {
            private readonly Regex _operationMatcher;

            protected DynamicOperationTranslatorBase(string operationPattern)
            {
                _operationMatcher = new Regex(operationPattern);
            }

            public bool TryTranslate(
                string operationDescription,
                DynamicExpression dynamicExpression,
                TranslationContext context,
                out string translated)
            {
                var match = _operationMatcher.Match(operationDescription);

                if (match.Success)
                {
                    return DoTranslate(match, dynamicExpression, context, out translated);
                }

                translated = null;
                return false;
            }

            protected abstract bool DoTranslate(
                Match match,
                DynamicExpression dynamicExpression,
                TranslationContext context,
                out string translated);
        }

        private class DynamicMemberAccessTranslator : DynamicOperationTranslatorBase
        {
            private readonly MemberAccessExpressionTranslator _memberAccessTranslator;

            public DynamicMemberAccessTranslator(MemberAccessExpressionTranslator memberAccessTranslator)
                : base(@"^GetMember (?<MemberName>[^\(]+)\(")
            {
                _memberAccessTranslator = memberAccessTranslator;
            }

            protected override bool DoTranslate(
                Match match,
                DynamicExpression dynamicExpression,
                TranslationContext context,
                out string translated)
            {
                translated = GetMemberAccess(match, dynamicExpression.Arguments, context);
                return true;
            }

            internal string GetMemberAccess(Match match, IEnumerable<Expression> arguments, TranslationContext context)
            {
                var subject = context.Translate(arguments.First());
                var memberName = match.Groups["MemberName"].Value;

                return _memberAccessTranslator.GetMemberAccess(subject, memberName);
            }
        }

        private class DynamicMemberWriteTranslator : DynamicOperationTranslatorBase
        {
            private readonly DynamicMemberAccessTranslator _memberAccessTranslator;
            private readonly AssignmentExpressionTranslator _assignmentTranslator;

            public DynamicMemberWriteTranslator(
                DynamicMemberAccessTranslator memberAccessTranslator,
                AssignmentExpressionTranslator assignmentTranslator)
                : base(@"^SetMember (?<MemberName>[^\(]+)\(")
            {
                _memberAccessTranslator = memberAccessTranslator;
                _assignmentTranslator = assignmentTranslator;
            }

            protected override bool DoTranslate(
                Match match,
                DynamicExpression dynamicExpression,
                TranslationContext context,
                out string translated)
            {
                var target = _memberAccessTranslator.GetMemberAccess(match, dynamicExpression.Arguments, context);
                var value = dynamicExpression.Arguments.Last();

                translated = _assignmentTranslator.GetAssignment(target, ExpressionType.Assign, value, context);
                return true;
            }
        }

        private class DynamicMethodCallTranslator : DynamicOperationTranslatorBase
        {
            private readonly MethodCallExpressionTranslator _methodCallTranslator;

            public DynamicMethodCallTranslator(MethodCallExpressionTranslator methodCallTranslator)
                : base(@"^Call (?<MethodName>[^\(]+)\(")
            {
                _methodCallTranslator = methodCallTranslator;
            }

            protected override bool DoTranslate(
                Match match,
                DynamicExpression dynamicExpression,
                TranslationContext context,
                out string translated)
            {
                var subjectObject = dynamicExpression.Arguments.First();
                var subject = context.Translate(subjectObject);
                var methodName = match.Groups["MethodName"].Value;
                var method = subjectObject.Type.GetMethod(methodName);
                var methodArguments = dynamicExpression.Arguments.Skip(1).ToArray();

                var methodInfo = GetMethodInfo(
                    methodName,
                    method,
                    methodArguments,
                    dynamicExpression.Type);

                translated = _methodCallTranslator.GetMethodCall(
                    subject,
                    methodInfo,
                    methodArguments,
                    dynamicExpression,
                    context);

                return true;
            }

            private static IMethodInfo GetMethodInfo(
                string methodName,
                MethodInfo method,
                Expression[] methodArguments,
                Type methodReturnType)
            {
                if (method == null)
                {
                    return new MissingMethodInfo(methodName);
                }

                return new BclMethodInfoWrapper(
                    method,
                    GetGenericArgumentsOrNull(method, methodArguments, methodReturnType));
            }

            private static IEnumerable<Type> GetGenericArgumentsOrNull(
                MethodInfo method,
                Expression[] methodArguments,
                Type methodReturnType)
            {
                if (!method.IsGenericMethod)
                {
                    return null;
                }

                var genericParameterTypes = method.GetGenericArguments();

                var methodParameters = method
                    .GetParameters()
                    .Select((p, i) => new { Index = i, Parameter = p })
                    .ToArray();

                var genericArguments = new Type[genericParameterTypes.Length];

                for (var i = 0; i < genericParameterTypes.Length; i++)
                {
                    var genericParameterType = genericParameterTypes[i];

                    if (genericParameterType == method.ReturnType)
                    {
                        genericArguments[i] = methodReturnType;
                        continue;
                    }

                    var matchingMethodParameter = methodParameters
                        .FirstOrDefault(p => p.Parameter.ParameterType == genericParameterType);

                    if (matchingMethodParameter == null)
                    {
                        return null;
                    }

                    var matchingMethodArgument = methodArguments
                        .ElementAtOrDefault(matchingMethodParameter.Index);

                    if (matchingMethodArgument == null)
                    {
                        return null;
                    }

                    genericArguments[i] = matchingMethodArgument.Type;
                }

                return genericArguments;
            }

            private class MissingMethodInfo : IMethodInfo
            {
                public MissingMethodInfo(string name)
                {
                    Name = name;
                }

                public string Name { get; }

                public bool IsGenericMethod => false;

                public bool IsExtensionMethod => false;

                public MethodInfo GetGenericMethodDefinition() => null;

                public IEnumerable<Type> GetGenericArguments() => Enumerable.Empty<Type>();

                public IEnumerable<ParameterInfo> GetParameters() => Enumerable.Empty<ParameterInfo>();

                public Type GetGenericArgumentFor(Type parameterType) => null;
            }
        }

        #endregion
    }
#endif
}