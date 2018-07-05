namespace AgileObjects.ReadableExpressions.Translators
{
    using System.Collections.Generic;
#if !NET35
    using System.Linq.Expressions;
#else
    using DefaultExpression = Microsoft.Scripting.Ast.DefaultExpression;
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;
#endif
    using Extensions;

    internal struct DefaultExpressionTranslator : IExpressionTranslator
    {
        public IEnumerable<ExpressionType> NodeTypes
        {
            get { yield return ExpressionType.Default; }
        }

        public string Translate(Expression expression, TranslationContext context)
        {
            var defaultExpression = (DefaultExpression)expression;

            if (defaultExpression.Type == typeof(void))
            {
                return null;
            }

            return defaultExpression.Type.CanBeNull()
                ? "null"
                : Translate(defaultExpression);
        }

        internal static string Translate(DefaultExpression defaultExpression)
        {
            if (defaultExpression.Type == typeof(string))
            {
                return "null";
            }

            var typeName = defaultExpression.Type.GetFriendlyName();

            return $"default({typeName})";
        }
    }
}