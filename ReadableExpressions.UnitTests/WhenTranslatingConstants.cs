namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingConstants
    {
        [TestMethod]
        public void ShouldTranslateAString()
        {
            var stringConstant = Expression.Constant("hello!", typeof(string));

            var translated = stringConstant.ToReadableString();

            Assert.AreEqual("\"hello!\"", translated);
        }

        [TestMethod]
        public void ShouldTranslateABoolean()
        {
            var boolConstant = Expression.Constant(true, typeof(bool));

            var translated = boolConstant.ToReadableString();

            Assert.AreEqual("true", translated);
        }

        [TestMethod]
        public void ShouldTranslateALong()
        {
            var longConstant = Expression.Constant(123L, typeof(long));

            var translated = longConstant.ToReadableString();

            Assert.AreEqual("123L", translated);
        }

        [TestMethod]
        public void ShouldTranslateAWholeNumberFloat()
        {
            var floatConstant = Expression.Constant(890.0f, typeof(float));

            var translated = floatConstant.ToReadableString();

            Assert.AreEqual("890.00f", translated);
        }

        [TestMethod]
        public void ShouldTranslateANonWholeNumberNullableFloat()
        {
            var floatConstant = Expression.Constant(12.34f, typeof(float?));

            var translated = floatConstant.ToReadableString();

            Assert.AreEqual("12.34f", translated);
        }

        [TestMethod]
        public void ShouldTranslateAWholeNumberDecimal()
        {
            var decimalConstant = Expression.Constant(456.00m, typeof(decimal));

            var translated = decimalConstant.ToReadableString();

            Assert.AreEqual("456.00m", translated);
        }

        [TestMethod]
        public void ShouldTranslateAWholeNumberDouble()
        {
            var doubleConstant = Expression.Constant(999.0, typeof(double));

            var translated = doubleConstant.ToReadableString();

            Assert.AreEqual("999.00", translated);
        }

        [TestMethod]
        public void ShouldTranslateAType()
        {
            var typeConstant = Expression.Constant(typeof(long), typeof(Type));

            var translated = typeConstant.ToReadableString();

            Assert.AreEqual("typeof(long)", translated);
        }

        [TestMethod]
        public void ShouldTranslateANullDefault()
        {
            var nullConstant = Expression.Constant(null, typeof(object));

            var translated = nullConstant.ToReadableString();

            Assert.AreEqual("null", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnEnumMember()
        {
            var enumConstant = Expression.Constant(OddNumber.One, typeof(OddNumber));

            var translated = enumConstant.ToReadableString();

            Assert.AreEqual("OddNumber.One", translated);
        }

        private enum OddNumber
        {
            One = 1
        }
    }
}
