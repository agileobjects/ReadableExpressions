namespace AgileObjects.ReadableExpressions.Translators
{
#if !NET35
    using System.Linq.Expressions;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using ExpressionType = Microsoft.Scripting.Ast.ExpressionType;

#endif

    internal class ExtensionExpressionTranslator : ExpressionTranslatorBase
    {
        public ExtensionExpressionTranslator()
            : base(ExpressionType.Extension)
        {
        }

        public override string Translate(Expression expression, TranslationContext context)
            => expression.ToString();
    }
}