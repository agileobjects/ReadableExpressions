namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
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

        [TestMethod]
        public void ShouldTranslateANewExpressionWithInitialisation()
        {
            Expression<Func<MemoryStream>> createToday = () => new MemoryStream { Position = 0 };

            var translated = createToday.ToReadableString();

            const string EXPECTED =
@"() => new MemoryStream
{
    Position = 0
}";
            Assert.AreEqual(EXPECTED, translated);
        }
    }
}
