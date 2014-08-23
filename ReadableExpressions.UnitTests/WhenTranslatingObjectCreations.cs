namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingObjectCreations
    {
        [TestMethod]
        public void ShouldTranslateAParameterlessNewExpression()
        {
            Expression<Func<object>> createObject = () => new object();

            var translated = createObject.ToReadableString();

            Assert.AreEqual("() => new Object()", translated);
        }

        [TestMethod]
        public void ShouldTranslateANewExpressionWithParameters()
        {
            Expression<Func<DateTime>> createToday = () => new DateTime(2014, 08, 23);

            var translated = createToday.ToReadableString();

            Assert.AreEqual("() => new DateTime(2014, 8, 23)", translated);
        }
    }
}
