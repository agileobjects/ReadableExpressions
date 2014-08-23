namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingExpressionTrees
    {
        [TestMethod]
        public void ShouldTranslateAnArrayLengthExpression()
        {
            Expression<Func<string[], int>> getArrayLength = a => a.Length;

            var translated = getArrayLength.ToReadableString();

            Assert.AreEqual("a => a.Length", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceCallExpression()
        {
            Expression<Func<object, string>> objectToString = o => o.ToString();

            var translated = objectToString.ToReadableString();

            Assert.AreEqual("o => o.ToString()", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceCallExpressionStaticMemberArgument()
        {
            // ReSharper disable once ReferenceEqualsWithValueType
            Expression<Func<int, string>> intToFormattedString = i => i.ToString(CultureInfo.CurrentCulture);

            var translated = intToFormattedString.ToReadableString();

            Assert.AreEqual("i => i.ToString(CultureInfo.CurrentCulture)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceCallExpressionParameterArgument()
        {
            // ReSharper disable once ReferenceEqualsWithValueType
            Expression<Func<int, CultureInfo, string>> intToFormattedString = (i, ci) => i.ToString(ci);

            var translated = intToFormattedString.ToReadableString();

            Assert.AreEqual("(i, ci) => i.ToString(ci)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAStaticCallExpression()
        {
            // ReSharper disable once ReferenceEqualsWithValueType
            Expression<Func<bool>> oneEqualsTwo = () => ReferenceEquals(1, 2);

            var translated = oneEqualsTwo.ToReadableString();

            Assert.AreEqual("() => Object.ReferenceEquals(1, 2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateACastExpression()
        {
            // ReSharper disable once ReferenceEqualsWithValueType
            Expression<Func<int, double>> intToDouble = i => (double)i;

            var translated = intToDouble.ToReadableString();

            Assert.AreEqual("i => (Double)i", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceMemberExpression()
        {
            Expression<Func<DateTime, int>> getDateDay = d => d.Day;

            var translated = getDateDay.ToReadableString();

            Assert.AreEqual("d => d.Day", translated);
        }

        [TestMethod]
        public void ShouldTranslateAStaticMemberExpression()
        {
            Expression<Func<DateTime>> getToday = () => DateTime.Today;

            var translated = getToday.ToReadableString();

            Assert.AreEqual("() => DateTime.Today", translated);
        }
    }
}
