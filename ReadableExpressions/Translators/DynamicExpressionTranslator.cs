namespace AgileObjects.ReadableExpressions.Translators
{
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
            MethodCallExpressionTranslator methodCallTranslator,
            Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Dynamic)
        {
            var dynamicMemberAccessTranslator = new DynamicMemberAccessTranslator(memberAccessTranslator, globalTranslator);

            _translators = new DynamicOperationTranslatorBase[]
            {
                dynamicMemberAccessTranslator,
                new DynamicMemberWriteTranslator(dynamicMemberAccessTranslator, assignmentTranslator, globalTranslator),
                new DynamicMethodCallTranslator(methodCallTranslator, globalTranslator)
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

            protected DynamicOperationTranslatorBase(
                string operationPattern,
                Translator globalTranslator)
            {
                GlobalTranslator = globalTranslator;
                _operationMatcher = new Regex(operationPattern);
            }

            protected Translator GlobalTranslator { get; }

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

            public DynamicMemberAccessTranslator(
                MemberAccessExpressionTranslator memberAccessTranslator,
                Translator globalTranslator)
                : base(@"^GetMember (?<MemberName>[^\(]+)\(", globalTranslator)
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
                var subject = GlobalTranslator.Invoke(arguments.First(), context);
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
                AssignmentExpressionTranslator assignmentTranslator,
                Translator globalTranslator)
                : base(@"^SetMember (?<MemberName>[^\(]+)\(", globalTranslator)
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

            public DynamicMethodCallTranslator(
                MethodCallExpressionTranslator methodCallTranslator,
                Translator globalTranslator)
                : base(@"^Call (?<MethodName>[^\(]+)\(", globalTranslator)
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
                var subject = GlobalTranslator.Invoke(subjectObject, context);
                var methodName = match.Groups["MethodName"].Value;
                var method = subjectObject.Type.GetMethod(methodName);

                var methodInfo = (method != null)
                    ? new BclMethodInfoWrapper(method)
                    : (IMethodInfo)new MissingMethodInfo(methodName);

                translated = _methodCallTranslator.GetMethodCall(
                    subject,
                    methodInfo,
                    dynamicExpression.Arguments.Skip(1).ToArray(),
                    dynamicExpression,
                    context);

                return true;
            }

            private class MissingMethodInfo : IMethodInfo
            {
                public MissingMethodInfo(string name)
                {
                    Name = name;
                }

                public string Name { get; }

                public bool IsGenericMethod => false;

                public MethodInfo GetGenericMethodDefinition() => null;

                public IEnumerable<Type> GetGenericArguments() => Enumerable.Empty<Type>();

                public IEnumerable<ParameterInfo> GetParameters() => Enumerable.Empty<ParameterInfo>();
            }
        }

        #endregion
    }
}