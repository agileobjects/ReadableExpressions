namespace AgileObjects.ReadableExpressions.Translators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal partial class InitialisationExpressionTranslator
    {
        private interface IInitExpressionHelper
        {
            string Translate(Expression expression);
        }

        private abstract class InitExpressionHelperBase<TExpression, TNewExpression> : IInitExpressionHelper
            where TExpression : Expression
            where TNewExpression : Expression
        {
            protected InitExpressionHelperBase(IExpressionTranslatorRegistry registry)
            {
                Registry = registry;
            }

            protected IExpressionTranslatorRegistry Registry { get; }

            public string Translate(Expression expression)
            {
                var typedExpression = (TExpression)expression;
                var newExpression = GetNewExpressionString(typedExpression);
                var initialisations = GetInitialisations(typedExpression);

                if ((newExpression.Length + initialisations.Sum(init => init.Length + 2)) <= 40)
                {
                    return newExpression + " { " + string.Join(", ", initialisations) + " }";
                }

                return newExpression + Environment.NewLine +
                    "{" + Environment.NewLine +
                        string.Join(
                            "," + Environment.NewLine,
                            initialisations.Select(init => "    " + init)) +
                    Environment.NewLine + "}";
            }

            private string GetNewExpressionString(TExpression initialisation)
            {
                var newExpression = GetNewExpression(initialisation);
                var newExpressionString = Registry.Translate(newExpression);

                if (ConstructorIsParameterless(newExpression))
                {
                    // Remove the empty brackets:
                    newExpressionString = newExpressionString.Substring(0, newExpressionString.Length - 2);
                }

                return newExpressionString;
            }

            protected abstract TNewExpression GetNewExpression(TExpression expression);

            protected abstract bool ConstructorIsParameterless(TNewExpression newExpression);

            protected abstract IEnumerable<string> GetInitialisations(TExpression expression);
        }
    }
}
