namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class ParameterExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly IEnumerable<string> _keywords;

        internal ParameterExpressionTranslator(Translator globalTranslator)
            : base(globalTranslator, ExpressionType.Parameter)
        {
            _keywords = TypeExtensions
                .TypeNames
                .Concat(new[]
                {
                    "typeof",
                    "default",
                    "void",
                    "readonly",
                    "do",
                    "while",
                    "switch",
                    "if",
                    "else",
                    "try",
                    "catch",
                    "finally",
                    "throw",
                    "for",
                    "foreach",
                    "goto",
                    "return",
                    "implicit",
                    "explicit"
                })
                .ToArray();
        }

        public override string Translate(Expression expression, TranslationContext context)
        {
            var parameterName = ((ParameterExpression)expression).Name;

            return _keywords.Contains(parameterName) ? "@" + parameterName : parameterName;
        }
    }
}