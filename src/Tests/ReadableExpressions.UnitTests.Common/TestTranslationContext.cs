namespace AgileObjects.ReadableExpressions.UnitTests.Common
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Translations;

    public class TestTranslationContext : ExpressionTranslation
    {
        public TestTranslationContext(Expression expression)
            : base(expression, TestTranslationSettings.TestSettings)
        {
        }
    }
}