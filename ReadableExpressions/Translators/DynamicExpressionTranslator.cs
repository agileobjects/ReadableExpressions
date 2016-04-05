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
            IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Dynamic)
        {
            var dynamicMemberAccessTranslator = new DynamicMemberAccessTranslator(memberAccessTranslator, registry);

            _translators = new DynamicOperationTranslatorBase[]
            {
                dynamicMemberAccessTranslator,
                new DynamicMemberWriteTranslator(dynamicMemberAccessTranslator, assignmentTranslator, registry),
                new DynamicMethodCallTranslator(methodCallTranslator, registry)
            };
        }

        public override string Translate(Expression expression)
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

                if (translator.TryTranslate(operationDescription, dynamicExpression.Arguments, out translated))
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
                IExpressionTranslatorRegistry registry)
            {
                Registry = registry;
                _operationMatcher = new Regex(operationPattern);
            }

            protected IExpressionTranslatorRegistry Registry { get; }

            public bool TryTranslate(
                string operationDescription,
                IEnumerable<Expression> arguments,
                out string translated)
            {
                var match = _operationMatcher.Match(operationDescription);

                if (match.Success)
                {
                    return DoTranslate(match, arguments, out translated);
                }

                translated = null;
                return false;
            }

            protected abstract bool DoTranslate(Match match, IEnumerable<Expression> arguments, out string translated);
        }

        private class DynamicMemberAccessTranslator : DynamicOperationTranslatorBase
        {
            private readonly MemberAccessExpressionTranslator _memberAccessTranslator;

            public DynamicMemberAccessTranslator(
                MemberAccessExpressionTranslator memberAccessTranslator,
                IExpressionTranslatorRegistry registry)
                : base(@"^GetMember (?<MemberName>[^\(]+)\(", registry)
            {
                _memberAccessTranslator = memberAccessTranslator;
            }

            protected override bool DoTranslate(Match match, IEnumerable<Expression> arguments, out string translated)
            {
                translated = GetMemberAccess(match, arguments);
                return true;
            }

            internal string GetMemberAccess(Match match, IEnumerable<Expression> arguments)
            {
                var subject = Registry.Translate(arguments.First());
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
                IExpressionTranslatorRegistry registry)
                : base(@"^SetMember (?<MemberName>[^\(]+)\(", registry)
            {
                _memberAccessTranslator = memberAccessTranslator;
                _assignmentTranslator = assignmentTranslator;
            }

            protected override bool DoTranslate(Match match, IEnumerable<Expression> arguments, out string translated)
            {
                var target = _memberAccessTranslator.GetMemberAccess(match, arguments);
                var value = arguments.Last();

                translated = _assignmentTranslator.GetAssignment(target, ExpressionType.Assign, value);
                return true;
            }
        }

        private class DynamicMethodCallTranslator : DynamicOperationTranslatorBase
        {
            private readonly MethodCallExpressionTranslator _methodCallTranslator;

            public DynamicMethodCallTranslator(
                MethodCallExpressionTranslator methodCallTranslator,
                IExpressionTranslatorRegistry registry)
                : base(@"^Call (?<MethodName>[^\(]+)\(", registry)
            {
                _methodCallTranslator = methodCallTranslator;
            }

            protected override bool DoTranslate(Match match, IEnumerable<Expression> arguments, out string translated)
            {
                var subjectObject = arguments.First();
                var subject = Registry.Translate(subjectObject);
                var methodName = match.Groups["MethodName"].Value;
                var method = subjectObject.Type.GetMethod(methodName);

                var methodInfo = (method != null)
                    ? new BclMethodInfoWrapper(method)
                    : (IMethodInfo)new MissingMethodInfo(methodName);

                translated = _methodCallTranslator.GetMethodCall(
                    subject,
                    methodInfo,
                    arguments.Skip(1).ToArray());

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
            }
        }

        #endregion
    }
}