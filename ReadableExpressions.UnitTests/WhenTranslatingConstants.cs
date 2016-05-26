namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

            Assert.AreEqual("890f", translated);
        }

        [TestMethod]
        public void ShouldTranslateANonWholeNumberNullableFloat()
        {
            var floatConstant = Expression.Constant(12.34f, typeof(float?));

            var translated = floatConstant.ToReadableString();

            Assert.AreEqual("12.34f", translated);
        }

        [TestMethod]
        public void ShouldTranslateAWholeNumberNullableDecimal()
        {
            var decimalConstant = Expression.Constant(456.00m, typeof(decimal?));

            var translated = decimalConstant.ToReadableString();

            Assert.AreEqual("456m", translated);
        }

        [TestMethod]
        public void ShouldTranslateANonWholeNumberDecimal()
        {
            var decimalConstant = Expression.Constant(6373282.64738m, typeof(decimal));

            var translated = decimalConstant.ToReadableString();

            Assert.AreEqual("6373282.64738m", translated);
        }

        [TestMethod]
        public void ShouldTranslateAWholeNumberDouble()
        {
            var doubleConstant = Expression.Constant(999.0, typeof(double));

            var translated = doubleConstant.ToReadableString();

            Assert.AreEqual("999d", translated);
        }

        [TestMethod]
        public void ShouldTranslateANonWholeNumberDouble()
        {
            var doubleConstant = Expression.Constant(64739.7, typeof(double));

            var translated = doubleConstant.ToReadableString();

            Assert.AreEqual("64739.7d", translated);
        }

        [TestMethod]
        public void ShouldTranslateAType()
        {
            var typeConstant = Expression.Constant(typeof(long), typeof(Type));

            var translated = typeConstant.ToReadableString();

            Assert.AreEqual("typeof(long)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAGenericRuntimeType()
        {
            var value = typeof(Dictionary<string, DateTime>);

            // ReSharper disable once PossibleMistakenCallToGetType.2
            var typeConstant = Expression.Constant(value, value.GetType());

            var translated = typeConstant.ToReadableString();

            Assert.AreEqual("typeof(Dictionary<string, DateTime>)", translated);
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

        [TestMethod]
        public void ShouldTranslateADefaultDate()
        {
            var dateConstant = Expression.Constant(default(DateTime));

            var translated = dateConstant.ToReadableString();

            Assert.AreEqual("default(DateTime)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADefaultTimeSpan()
        {
            var timeSpanConstant = Expression.Constant(default(TimeSpan));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("default(TimeSpan)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADefaultString()
        {
            var nullStringConstant = Expression.Default(typeof(string));

            var translated = nullStringConstant.ToReadableString();

            Assert.AreEqual("null", translated);
        }

        [TestMethod]
        public void ShouldTranslateAParameterlessFunc()
        {
            Func<string> stringFactory = () => "Factory!";
            var funcConstant = Expression.Constant(stringFactory);

            var translated = funcConstant.ToReadableString();

            Assert.AreEqual("Func<string>", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnAction()
        {
            Action<int, long> numberAdder = (i, l) => Console.WriteLine(i + l);
            var actionConstant = Expression.Constant(numberAdder);

            var translated = actionConstant.ToReadableString();

            Assert.AreEqual("Action<int, long>", translated);
        }

        [TestMethod]
        public void ShouldTranslateAFuncWithNestedGenericParameters()
        {
            Func<int?, FileInfo, Dictionary<IDictionary<FileInfo, string[]>, string>> dictionaryFactory =
                (i, fileInfo) => new Dictionary<IDictionary<FileInfo, string[]>, string>
                {
                    [new Dictionary<FileInfo, string[]> { [fileInfo] = new[] { fileInfo.ToString() } }] = i.ToString()
                };

            var funcConstant = Expression.Constant(dictionaryFactory);

            var translated = funcConstant.ToReadableString();

            Assert.AreEqual("Func<int?, FileInfo, Dictionary<IDictionary<FileInfo, string[]>, string>>", translated);
        }

        private enum OddNumber
        {
            One = 1
        }
    }
}
