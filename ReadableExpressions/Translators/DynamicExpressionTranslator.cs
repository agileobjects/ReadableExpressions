namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    internal class DynamicExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly IEnumerable<DynamicOperationTranslatorBase> _translators;

        public DynamicExpressionTranslator(
            MemberAccessExpressionTranslator memberAccessTranslator,
            MethodCallExpressionTranslator methodCallTranslator,
            IExpressionTranslatorRegistry registry)
            : base(registry, ExpressionType.Dynamic)
        {
            _translators = new DynamicOperationTranslatorBase[]
            {
                new DynamicMemberAccessTranslator(memberAccessTranslator, registry),
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

        #region Helper classes

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
                var subject = Registry.Translate(arguments.First());
                var memberName = match.Groups["MemberName"].Value;

                translated = _memberAccessTranslator.GetMemberAccess(subject, memberName);
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
                var subject = Registry.Translate(arguments.First());
                var methodName = match.Groups["MethodName"].Value;

                translated = _methodCallTranslator.GetMethodCall(
                    subject,
                    methodName,
                    arguments.Skip(1).ToArray());

                return true;
            }
        }

        #endregion
    }
}