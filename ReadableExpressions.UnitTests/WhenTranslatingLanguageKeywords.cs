namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingLanguageKeywords
    {
        [TestMethod]
        public void ShouldTranslateADefaultExpression()
        {
            var defaultInt = Expression.Default(typeof(uint));
            var translated = defaultInt.ToReadableString();

            Assert.AreEqual("default(uint)", translated);
        }

        [TestMethod]
        public void ShouldIgnoreADefaultVoidExpression()
        {
            var defaultVoid = Expression.Default(typeof(void));
            var translated = defaultVoid.ToReadableString();

            Assert.IsNull(translated);
        }
    }
}
