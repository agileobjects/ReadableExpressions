namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    internal class InitialisationExpressionTranslator : ExpressionTranslatorBase
    {
        internal InitialisationExpressionTranslator()
            : base(ExpressionType.MemberInit)
        {
        }

        public override string Translate(Expression expression, IExpressionTranslatorRegistry translatorRegistry)
        {
            var initialisation = (MemberInitExpression)expression;
            var newExpression = GetNewExpression(initialisation, translatorRegistry);

            var initialisations = string.Join(
                "," + Environment.NewLine,
                initialisation.Bindings.Select(b => GetMemberBinding(b, translatorRegistry)));

            return newExpression + Environment.NewLine +
                "{" + Environment.NewLine +
                    initialisations +
                Environment.NewLine + "}";
        }

        private static string GetNewExpression(
            MemberInitExpression initialisation,
            IExpressionTranslatorRegistry translatorRegistry)
        {
            var newExpression = translatorRegistry.Translate(initialisation.NewExpression);

            if (!initialisation.NewExpression.Arguments.Any())
            {
                // Remove the empty brackets:
                newExpression = newExpression.Substring(0, newExpression.Length - 2);
            }

            return newExpression;
        }

        private static string GetMemberBinding(MemberBinding binding, IExpressionTranslatorRegistry translatorRegistry)
        {
            if (binding.BindingType == MemberBindingType.Assignment)
            {
                var assignment = (MemberAssignment)binding;
                var value = translatorRegistry.Translate(assignment.Expression);

                return "    " + assignment.Member.Name + " = " + value;
            }

            return null;
        }
    }
}