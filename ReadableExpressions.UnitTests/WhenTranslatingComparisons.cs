namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingComparisons
    {
        [TestMethod]
        public void ShouldTranslateAnEqualityExpression()
        {
            Expression<Func<int, int, bool>> intsAreEqual = (i1, i2) => i1 == i2;

            var translated = intsAreEqual.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 == i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateALessThanExpression()
        {
            Expression<Func<int, int, bool>> firstLessThanSecond = (i1, i2) => i1 < i2;

            var translated = firstLessThanSecond.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 < i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateALessThanOrEqualExpression()
        {
            Expression<Func<int, int, bool>> firstLessThanOrEqualToSecond = (i1, i2) => i1 <= i2;

            var translated = firstLessThanOrEqualToSecond.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 <= i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAGreaterThanOrEqualExpression()
        {
            Expression<Func<int, int, bool>> firstGreaterThanOrEqualToSecond = (i1, i2) => i1 >= i2;

            var translated = firstGreaterThanOrEqualToSecond.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 >= i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAGreaterThanExpression()
        {
            Expression<Func<int, int, bool>> firstGreaterThanSecond = (i1, i2) => i1 > i2;

            var translated = firstGreaterThanSecond.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 > i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInequalityExpression()
        {
            Expression<Func<int, int, bool>> intsAreNotEqual = (i1, i2) => i1 != i2;

            var translated = intsAreNotEqual.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 != i2", translated);
        }

        [TestMethod]
        public void ShouldAbbreviateBooleanTrueComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsTrue = Expression.Equal(boolVariable, Expression.Constant(true));

            var translated = boolIsTrue.ToReadableString();

            Assert.AreEqual("couldBe", translated);
        }

        [TestMethod]
        public void ShouldAbbreviateBooleanFalseComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsFalse = Expression.Equal(Expression.Constant(false), boolVariable);

            var translated = boolIsFalse.ToReadableString();

            Assert.AreEqual("!couldBe", translated);
        }

        [TestMethod]
        public void ShouldAbbreviateNotBooleanTrueComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsNotTrue = Expression.NotEqual(Expression.Constant(true), boolVariable);

            var translated = boolIsNotTrue.ToReadableString();

            Assert.AreEqual("!couldBe", translated);
        }

        [TestMethod]
        public void ShouldAbbreviateNotBooleanFalseComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsNotFalse = Expression.NotEqual(boolVariable, Expression.Constant(false));

            var translated = boolIsNotFalse.ToReadableString();

            Assert.AreEqual("couldBe", translated);
        }

        [TestMethod]
        public void ShouldAbbreviateDefaultBooleanComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsFalse = Expression.Equal(Expression.Default(typeof(bool)), boolVariable);

            var translated = boolIsFalse.ToReadableString();

            Assert.AreEqual("!couldBe", translated);
        }
    }
}