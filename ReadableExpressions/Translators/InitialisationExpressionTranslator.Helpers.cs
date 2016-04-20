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
            private readonly Func<TExpression, TNewExpression> _newExpressionFactory;

            protected InitExpressionHelperBase(
                Translator globalTranslator,
                Func<TExpression, TNewExpression> newExpressionFactory = null)
            {
                _newExpressionFactory = newExpressionFactory;
                GlobalTranslator = globalTranslator;
            }

            protected Translator GlobalTranslator { get; }

            public string Translate(Expression expression, TranslationContext context)
            {
                var typedExpression = (TExpression)expression;
                var newExpression = GetNewExpressionString(typedExpression, context);
                var memberInitialisations = GetMemberInitialisations(typedExpression, context).ToArray();

                return GetInitialisation(newExpression, memberInitialisations);
            }

            protected virtual string GetNewExpressionString(TExpression initialisation, TranslationContext context)
            {
                var newExpression = _newExpressionFactory.Invoke(initialisation);
                var newExpressionString = GlobalTranslator.Invoke(newExpression, context);

                if (ConstructorIsParameterless(newExpression))
                {
                    // Remove the empty brackets:
                    newExpressionString = newExpressionString.Substring(0, newExpressionString.Length - 2);
                }

                return newExpressionString;
            }

            protected abstract bool ConstructorIsParameterless(TNewExpression newExpression);

            protected abstract IEnumerable<string> GetMemberInitialisations(TExpression initialisation, TranslationContext context);

            protected static string GetInitialisation(string newExpression, string[] memberInitialisations)
            {
                if ((newExpression.Length + memberInitialisations.Sum(init => init.Length + 2)) <= 40)
                {
                    return $"{newExpression} {{ {string.Join(", ", memberInitialisations)} }}";
                }

                var initialisationBlock = string.Join(
                    "," + Environment.NewLine,
                    memberInitialisations.Select(init => init.Indent()));

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
