namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingLogicalOperations
    {
        [TestMethod]
        public void ShouldTranslateAnAndOperation()
        {
            Expression<Func<bool, bool, bool>> bothBoolsAreTheSame = (b1, b2) => b1 && b2;

            var translated = bothBoolsAreTheSame.ToReadableString();

            Assert.AreEqual("(b1, b2) => b1 && b2", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseAndOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseAnd = (b1, b2) => b1 & b2;

            var translated = bitwiseAnd.ToReadableString();

            Assert.AreEqual("(b1, b2) => b1 & b2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnOrOperation()
        {
            Expression<Func<bool, bool, bool>> eitherBoolsIsTrue = (b1, b2) => b1 || b2;

            var translated = eitherBoolsIsTrue.ToReadableString();

            Assert.AreEqual("(b1, b2) => b1 || b2", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseOrOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseOr = (b1, b2) => b1 | b2;

            var translated = bitwiseOr.ToReadableString();

            Assert.AreEqual("(b1, b2) => b1 | b2", translated);
        }

        [TestMethod]
        public void ShouldTranslateACoalesceOperation()
        {
            Expression<Func<bool?, bool, bool>> oneOrTwo = (b1, b2) => b1 ?? b2;

            var translated = oneOrTwo.ToReadableString();

            Assert.AreEqual("(b1, b2) => b1 ?? b2", translated);
        }
    }
}