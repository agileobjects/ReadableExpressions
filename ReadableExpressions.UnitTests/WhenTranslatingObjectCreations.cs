namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingObjectCreations
    {
        [TestMethod]
        public void ShouldTranslateAParameterlessNewExpression()
        {
            Expression<Func<object>> createObject = () => new object();

            var translated = createObject.Body.ToReadableString();

            Assert.AreEqual("new Object()", translated);
        }

        [TestMethod]
        public void ShouldTranslateANewExpressionWithParameters()
        {
            Expression<Func<DateTime>> createToday = () => new DateTime(2014, 08, 23);

            var translated = createToday.Body.ToReadableString();

            Assert.AreEqual("new DateTime(2014, 8, 23)", translated);
        }

        [TestMethod]
        public void ShouldTranslateANewExpressionWithASingleInitialisation()
        {
            Expression<Func<MemoryStream>> createMemoryStream = () => new MemoryStream { Position = 0 };

            var translated = createMemoryStream.Body.ToReadableString();

            Assert.AreEqual("new MemoryStream { Position = 0L }", translated);
        }

        [TestMethod]
        public void ShouldTranslateANewExpressionWithMultipleInitialisations()
        {
            Expression<Func<MemoryStream>> createMemoryStream =
                () => new MemoryStream { Capacity = 10000, Position = 100 };

            var translated = createMemoryStream.Body.ToReadableString();

            const string EXPECTED = @"
new MemoryStream
{
    Capacity = 10000,
    Position = 100L
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            Expression<Func<List<decimal>>> createList =
                () => new List<decimal> { 1.00m, 2.00m, 3.00m };

            var translated = createList.Body.ToReadableString();

            const string EXPECTED = @"
new List<decimal>
{
    Add(1.00m),
    Add(2.00m),
    Add(3.00m)
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateANewArrayExpressionWithAdditions()
        {
            Expression<Func<float[]>> createArray = () => new[] { 1.00f, 2.3f, 3.00f };

            var translated = createArray.Body.ToReadableString();

            Assert.AreEqual("new float[3] { 1.00f, 2.3f, 3.00f }", translated);
        }

        [TestMethod]
        public void ShouldTranslateAStringConstantConstructorParameter()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder("Hello!");

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.AreEqual("new StringBuilder(\"Hello!\")", translated);
        }

        [TestMethod]
        public void ShouldTranslateACharacterConstantConstructorParameter()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder('f');

            var translated = createStringBuilder.Body.ToReadableString();

            // Constant character expressions have .Type Int32, so they 
            // can't be differentiated from int constants :(
            Assert.AreEqual($"new StringBuilder({((int)'f')})", translated);
        }

        [TestMethod]
        public void ShouldTranslateConstantConstructorParameters()
        {
            Expression<Func<StringBuilder>> createStringBuilder = () => new StringBuilder(1000, 10000);

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.AreEqual("new StringBuilder(1000, 10000)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAParameterConstructorParameter()
        {
            Expression<Func<string, StringBuilder>> createStringBuilder = str => new StringBuilder(str);

            var translated = createStringBuilder.Body.ToReadableString();

            Assert.AreEqual("new StringBuilder(str)", translated);
        }
    }
}
