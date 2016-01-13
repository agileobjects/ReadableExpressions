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
            var defaultInt = Expression.Default(typeof(int));
            var translated = defaultInt.ToReadableString();

            Assert.AreEqual("default(Int32)", translated);
        }
    }
}
