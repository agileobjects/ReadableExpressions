namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
    using System.Linq;
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
    using ParameterExpression = Microsoft.Scripting.Ast.ParameterExpression;
#endif
    using Extensions;

    internal struct ParameterExpressionTranslator : IExpressionTranslator
    {
        private static readonly IEnumerable<string> _keywords = InternalTypeExtensions
            .TypeNames
            .Combine(new[]
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

        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.Parameter; }
        }

        public string Translate(Expression expression, TranslationContext context)
            => Translate((ParameterExpression)expression, context);

        public static string Translate(ParameterExpression parameter, TranslationContext context)
        {
            var parameterName = parameter.Name;

            if (parameterName.IsNullOrWhiteSpace())
            {
                var variableNumber = context.GetUnnamedVariableNumber(parameter);

                parameterName = parameter.Type.GetVariableNameInCamelCase() + variableNumber;
            }

            return _keywords.Contains(parameterName) ? "@" + parameterName : parameterName;
        }
    }
}