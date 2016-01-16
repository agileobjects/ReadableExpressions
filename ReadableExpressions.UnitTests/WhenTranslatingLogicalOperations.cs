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

            var translated = bothBoolsAreTheSame.Body.ToReadableString();

            Assert.AreEqual("(b1 && b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseAndOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseAnd = (b1, b2) => b1 & b2;

            var translated = bitwiseAnd.Body.ToReadableString();

            Assert.AreEqual("(b1 & b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnOrOperation()
        {
            Expression<Func<bool, bool, bool>> eitherBoolsIsTrue = (b1, b2) => b1 || b2;

            var translated = eitherBoolsIsTrue.Body.ToReadableString();

            Assert.AreEqual("(b1 || b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseOrOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseOr = (b1, b2) => b1 | b2;

            var translated = bitwiseOr.Body.ToReadableString();

            Assert.AreEqual("(b1 | b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateACoalesceOperation()
        {
            Expression<Func<bool?, bool, bool>> oneOrTwo = (b1, b2) => b1 ?? b2;

            var translated = oneOrTwo.Body.ToReadableString();

            Assert.AreEqual("(b1 ?? b2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAConditionalOperation()
        {
            Expression<Func<int, string>> whatSize = i => (i < 8) ? "Too small" : "Too big";

            var translated = whatSize.Body.ToReadableString();

            Assert.AreEqual("(i < 8) ? \"Too small\" : \"Too big\"", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIsTypeExpression()
        {
            Expression<Func<object, bool>> objectIsDisposable = o => o is IDisposable;

            var translated = objectIsDisposable.Body.ToReadableString();

            Assert.AreEqual("(o is IDisposable)", translated);
        }
    }
}