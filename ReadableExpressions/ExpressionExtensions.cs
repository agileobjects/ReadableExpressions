namespace AgileObjects.ReadableExpressions
{
    using System.Linq.Expressions;

    public static class ExpressionExtensions
    {
        private static readonly ExpressionTranslatorRegistry _translatorRegistry = new ExpressionTranslatorRegistry();

        public static string ToReadableString(this Expression expression)
        {
            return _translatorRegistry
                .Translate(expression, new TranslationContext())?
                .WithoutUnindents();
        }

        internal static bool IsReturnable(this Expression expression)
        {
            if (expression.Type == typeof(void))
            {
                return false;
            }

            switch (expression.NodeType)
            {
                case ExpressionType.Block:
                    return ((BlockExpression)expression).IsReturnable();

                case ExpressionType.Call:
                case ExpressionType.Conditional:
                case ExpressionType.Constant:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Invoke:
                case ExpressionType.MemberAccess:
                case ExpressionType.Parameter:
                    return true;
            }

            return false;
        }

        internal static bool IsReturnable(this BlockExpression block)
        {
            return (block.Type != typeof(void)) && block.Result.IsReturnable();
        }
    }
}
