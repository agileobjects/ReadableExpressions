namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingMathsOperations
    {
        [TestMethod]
        public void ShouldTranslateAnAdditionExpression()
        {
            Expression<Func<int, int, int>> addInts = (i1, i2) => i1 + i2;

            var translated = addInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 + i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateASubtractionExpression()
        {
            Expression<Func<int, int, int>> subtractInts = (i1, i2) => i1 - i2;

            var translated = subtractInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 - i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateANegationExpression()
        {
            Expression<Func<int, int>> negateInts = i => -i;

            var translated = negateInts.ToReadableString();

            Assert.AreEqual("i => -i", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultiplicationExpression()
        {
            Expression<Func<int, int, int>> multiplyInts = (i1, i2) => i1 * i2;

            var translated = multiplyInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 * i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateADivisionExpression()
        {
            Expression<Func<int, int, int>> divideInts = (i1, i2) => i1 / i2;

            var translated = divideInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 / i2", translated);
        }
    }
}