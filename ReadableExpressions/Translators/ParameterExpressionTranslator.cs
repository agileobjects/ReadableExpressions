namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class ParameterExpressionTranslator : ExpressionTranslatorBase
    {
        private readonly IEnumerable<string> _keywords;

        internal ParameterExpressionTranslator()
            : base(ExpressionType.Parameter)
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
            return Translate((ParameterExpression)expression);
        }

        public string Translate(ParameterExpression parameter)
        {
            return _keywords.Contains(parameter.Name) ? "@" + parameter.Name : parameter.Name;
        }
    }
}