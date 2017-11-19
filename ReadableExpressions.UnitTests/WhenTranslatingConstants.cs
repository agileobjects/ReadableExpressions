namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
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
        public void ShouldTranslateADateTimeWithNoTime()
        {
            var dateConstant = Expression.Constant(new DateTime(2015, 07, 02));

            var translated = dateConstant.ToReadableString();

            Assert.AreEqual("new DateTime(2015, 07, 02)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADateTimeWithATime()
        {
            var dateConstant = Expression.Constant(new DateTime(2016, 08, 01, 10, 23, 45));

            var translated = dateConstant.ToReadableString();

            Assert.AreEqual("new DateTime(2016, 08, 01, 10, 23, 45)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADateTimeWithMilliseconds()
        {
            var dateConstant = Expression.Constant(new DateTime(2017, 01, 10, 00, 00, 00, 123));

            var translated = dateConstant.ToReadableString();

            Assert.AreEqual("new DateTime(2017, 01, 10, 00, 00, 00, 123)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADefaultTimeSpan()
        {
            var timeSpanConstant = Expression.Constant(default(TimeSpan));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("default(TimeSpan)", translated);
        }

        [TestMethod]
        public void ShouldTranslateATimeSpanOfDays()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromDays(1));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("TimeSpan.FromDays(1)", translated);
        }

        [TestMethod]
        public void ShouldTranslateATimeSpanOfHours()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromHours(2));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("TimeSpan.FromHours(2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateATimeSpanOfMinutes()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromMinutes(10));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("TimeSpan.FromMinutes(10)", translated);
        }

        [TestMethod]
        public void ShouldTranslateATimeSpanOfSeconds()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromSeconds(58));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("TimeSpan.FromSeconds(58)", translated);
        }

        [TestMethod]
        public void ShouldTranslateATimeSpanOfMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromMilliseconds(923));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("TimeSpan.FromMilliseconds(923)", translated);
        }

        [TestMethod]
        public void ShouldTranslateATimeSpanOfTicks()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromTicks(428));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("TimeSpan.FromTicks(428)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADayTimeSpanWithMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(2, 3, 4, 5, 6));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("new TimeSpan(2, 3, 4, 5, 6)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADayTimeSpanWithoutMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(3, 4, 5, 6));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("new TimeSpan(3, 4, 5, 6)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnHourTimeSpan()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(6, 5, 4));

            var translated = timeSpanConstant.ToReadableString();

            Assert.AreEqual("new TimeSpan(6, 5, 4)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADefaultString()
        {
            var nullStringConstant = Expression.Default(typeof(string));

            var translated = nullStringConstant.ToReadableString();

            Assert.AreEqual("null", translated);
        }

        [TestMethod]
        public void ShouldTranslateADefaultGuid()
        {
            var guidConstant = Expression.Constant(default(Guid));

            var translated = guidConstant.ToReadableString();

            Assert.AreEqual("default(Guid)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAParameterlessFunc()
        {
            Func<object> stringFactory = () => "Factory!";
            var funcConstant = Expression.Constant(stringFactory);

            var translated = funcConstant.ToReadableString();

            Assert.AreEqual("Func<object>", translated);
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
        public void ShouldTranslateAParameterisedAction()
        {
            Action<IDictionary<object, List<string>>> dictionaryPrinter = Console.WriteLine;
            var actionConstant = Expression.Constant(dictionaryPrinter);

            var translated = actionConstant.ToReadableString();

            Assert.AreEqual("Action<IDictionary<object, List<string>>>", translated);
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

        [TestMethod]
        public void ShouldTranslateAnActionWithMultipleNestedGenericParameters()
        {
            Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>> genericAction = fileInfo => { };

            var actionConstant = Expression.Constant(genericAction);

            var translated = actionConstant.ToReadableString();

            Assert.AreEqual("Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>>", translated);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/5
        [TestMethod]
        public void ShouldTranslateDbNullValue()
        {
            var dbParameter = Expression.Variable(typeof(DbParameter), "param");
            var parameterValue = Expression.Property(dbParameter, "Value");
            var dbNull = Expression.Constant(DBNull.Value, typeof(DBNull));
            var setParamToDbNull = Expression.Assign(parameterValue, dbNull);

            var translated = setParamToDbNull.ToReadableString();

            Assert.AreEqual("param.Value = DBNull.Value", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnObjectConstant()
        {
            var objectConstant = Expression.Constant(123, typeof(object));

            var translated = objectConstant.ToReadableString();

            Assert.AreEqual("123", translated);
        }

        [TestMethod]
        public void ShouldTranslateALambdaConstant()
        {
            Expression<Func<int, int>> lambda =
                num => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index);

            var lambdaConstant = Expression.Constant(lambda, lambda.GetType());

            var translated = lambdaConstant.ToReadableString();

            const string EXPECTED = @"num => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index)";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }

    internal class GenericOne<T> { }

    internal class GenericTwo<T> { }

    internal class Generic<T1, T2, T3> { }

    internal enum OddNumber
    {
        One = 1
    }
}
