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

            Assert.AreEqual("i => (double)i", translated);
        }

        [TestMethod]
        public void ShouldTranslateACheckedCastExpression()
        {
            var intParameter = Expression.Parameter(typeof(int), "i");
            var checkedCast = Expression.ConvertChecked(intParameter, typeof(short));

            var checkedCastLambda = Expression.Lambda<Func<int, short>>(checkedCast, intParameter);

            var translated = checkedCastLambda.ToReadableString();

            Assert.AreEqual("i => (short)i", translated);
        }

        [TestMethod]
        public void ShouldTranslateACastToNullableExpression()
        {
            Expression<Func<long, long?>> longToNullable = l => (long?)l;

            var translated = longToNullable.ToReadableString();

            Assert.AreEqual("l => (long?)l", translated);
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

        [TestMethod]
        public void ShouldTranslateAnUnboxExpression()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var unboxObjectToInt = Expression.Unbox(objectVariable, typeof(int));

            var translated = unboxObjectToInt.ToReadableString();

            Assert.AreEqual("((int)o)", translated);
        }
    }
}