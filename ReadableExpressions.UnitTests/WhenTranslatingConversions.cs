namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingConversions
    {
        [TestMethod]
        public void ShouldTranslateACastExpression()
        {
            Expression<Func<int, double>> intToDouble = i => (double)i;

            var translated = intToDouble.ToReadableString();

            Assert.AreEqual("i => (Double)i", translated);
        }

        [TestMethod]
        public void ShouldTranslateACastToNullableExpression()
        {
            Expression<Func<long, long?>> longToNullable = l => (long?)l;

            var translated = longToNullable.ToReadableString();

            Assert.AreEqual("l => (Long?)l", translated);
        }

        [TestMethod]
        public void ShouldUseParenthesisInCasting()
        {
            Expression<Func<object, int>> castDateTimeHour = o => ((DateTime)o).Hour;

            var translated = castDateTimeHour.ToReadableString();

            Assert.AreEqual("o => ((DateTime)o).Hour", translated);
        }

        [TestMethod]
        public void ShouldTranslateANegationExpression()
        {
            Expression<Func<bool, bool>> negator = b => !b;

            var translated = negator.ToReadableString();

            Assert.AreEqual("b => !b", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnAsCastExpression()
        {
            Expression<Func<Stream, IDisposable>> streamAsDisposable = stream => stream as IDisposable;

            var translated = streamAsDisposable.Body.ToReadableString();

            Assert.AreEqual("(stream as IDisposable)", translated);
        }
    }
}