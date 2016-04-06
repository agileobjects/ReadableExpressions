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
            string Translate(Expression expression, TranslationContext context);
        }

        private abstract class InitExpressionHelperBase<TExpression, TNewExpression> : IInitExpressionHelper
            where TExpression : Expression
            where TNewExpression : Expression
        {
            protected InitExpressionHelperBase(Func<Expression, TranslationContext, string> globalTranslator)
            {
                GlobalTranslator = globalTranslator;
            }

            protected Func<Expression, TranslationContext, string> GlobalTranslator { get; }

            public string Translate(Expression expression, TranslationContext context)
            {
                var typedExpression = (TExpression)expression;
                var newExpression = GetNewExpressionString(typedExpression, context);
                var initialisations = GetInitialisations(typedExpression, context).ToArray();

                return GetInitialisations(initialisations, newExpression);
            }

            private string GetNewExpressionString(TExpression initialisation, TranslationContext context)
            {
                var newExpression = GetNewExpression(initialisation);
                var newExpressionString = GlobalTranslator.Invoke(newExpression, context);

                if (ConstructorIsParameterless(newExpression))
                {
                    // Remove the empty brackets:
                    newExpressionString = newExpressionString.Substring(0, newExpressionString.Length - 2);
                }

                return newExpressionString;
            }

            protected abstract TNewExpression GetNewExpression(TExpression expression);

            protected abstract bool ConstructorIsParameterless(TNewExpression newExpression);

            protected abstract IEnumerable<string> GetInitialisations(TExpression expression, TranslationContext context);

            protected static string GetInitialisations(string[] initialisations, string newExpression = null)
            {
                if ((newExpression?.Length + initialisations.Sum(init => init.Length + 2)) <= 40)
                {
                    return $"{newExpression} {{ {string.Join(", ", initialisations)} }}";
                }

                var initialisationBlock = string.Join(
                    "," + Environment.NewLine,
                    initialisations.Select(init => init.Indent()));

                var initialisation = $@"
{newExpression}
{{
{initialisationBlock}
}}";
                return initialisation.TrimStart();
            }
        }
    }
}
