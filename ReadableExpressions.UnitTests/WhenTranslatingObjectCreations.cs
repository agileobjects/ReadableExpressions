namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
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
            Expression<Func<MemoryStream>> createMemoryStream = () => new MemoryStream { Position = 0 };

            var translated = createMemoryStream.ToReadableString();

            const string EXPECTED =
@"() => new MemoryStream
{
    Position = 0
}";
            Assert.AreEqual(EXPECTED, translated);
        }

        [TestMethod]
        public void ShouldTranslateANewExpressionWithMultipleInitialisations()
        {
            Expression<Func<MemoryStream>> createMemoryStream =
                () => new MemoryStream { Capacity = 10000, Position = 100 };

            var translated = createMemoryStream.ToReadableString();

            const string EXPECTED =
@"() => new MemoryStream
{
    Capacity = 10000,
    Position = 100
}";
            Assert.AreEqual(EXPECTED, translated);
        }

        [TestMethod]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            Expression<Func<List<decimal>>> createList =
                () => new List<decimal> { 1.00m, 2.00m, 3.00m };

            var translated = createList.ToReadableString();

            const string EXPECTED =
@"() => new List<Decimal>
{
    Add(1.00),
    Add(2.00),
    Add(3.00)
}";
            Assert.AreEqual(EXPECTED, translated);
        }

        [TestMethod]
        public void ShouldTranslateANewArrayExpressionWithAdditions()
        {
            Expression<Func<float[]>> createArray = () => new[] { 1.00f, 2.00f, 3.00f };

            var translated = createArray.ToReadableString();

            const string EXPECTED =
@"() => new Single[3]
{
    1,
    2,
    3
}";
            Assert.AreEqual(EXPECTED, translated);
        }
    }
}
