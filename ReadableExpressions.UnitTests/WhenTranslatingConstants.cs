namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Xunit;

    public class WhenTranslatingConstants
    {
        [Fact]
        public void ShouldTranslateAString()
        {
            var stringConstant = Expression.Constant("hello!", typeof(string));

            var translated = stringConstant.ToReadableString();

            Assert.Equal("\"hello!\"", translated);
        }

        [Fact]
        public void ShouldTranslateABoolean()
        {
            var boolConstant = Expression.Constant(true, typeof(bool));

            var translated = boolConstant.ToReadableString();

            Assert.Equal("true", translated);
        }

        [Fact]
        public void ShouldTranslateALong()
        {
            var longConstant = Expression.Constant(123L, typeof(long));

            var translated = longConstant.ToReadableString();

            Assert.Equal("123L", translated);
        }

        [Fact]
        public void ShouldTranslateAWholeNumberFloat()
        {
            var floatConstant = Expression.Constant(890.0f, typeof(float));

            var translated = floatConstant.ToReadableString();

            Assert.Equal("890f", translated);
        }

        [Fact]
        public void ShouldTranslateANonWholeNumberNullableFloat()
        {
            var floatConstant = Expression.Constant(12.34f, typeof(float?));

            var translated = floatConstant.ToReadableString();

            Assert.Equal("12.34f", translated);
        }

        [Fact]
        public void ShouldTranslateAWholeNumberNullableDecimal()
        {
            var decimalConstant = Expression.Constant(456.00m, typeof(decimal?));

            var translated = decimalConstant.ToReadableString();

            Assert.Equal("456m", translated);
        }

        [Fact]
        public void ShouldTranslateANonWholeNumberDecimal()
        {
            var decimalConstant = Expression.Constant(6373282.64738m, typeof(decimal));

            var translated = decimalConstant.ToReadableString();

            Assert.Equal("6373282.64738m", translated);
        }

        [Fact]
        public void ShouldTranslateAWholeNumberDouble()
        {
            var doubleConstant = Expression.Constant(999.0, typeof(double));

            var translated = doubleConstant.ToReadableString();

            Assert.Equal("999d", translated);
        }

        [Fact]
        public void ShouldTranslateANonWholeNumberDouble()
        {
            var doubleConstant = Expression.Constant(64739.7, typeof(double));

            var translated = doubleConstant.ToReadableString();

            Assert.Equal("64739.7d", translated);
        }

        [Fact]
        public void ShouldTranslateAType()
        {
            var typeConstant = Expression.Constant(typeof(long), typeof(Type));

            var translated = typeConstant.ToReadableString();

            Assert.Equal("typeof(long)", translated);
        }

        [Fact]
        public void ShouldTranslateAGenericRuntimeType()
        {
            var value = typeof(Dictionary<string, DateTime>);

            // ReSharper disable once PossibleMistakenCallToGetType.2
            var typeConstant = Expression.Constant(value, value.GetType());

            var translated = typeConstant.ToReadableString();

            Assert.Equal("typeof(Dictionary<string, DateTime>)", translated);
        }

        [Fact]
        public void ShouldTranslateANullDefault()
        {
            var nullConstant = Expression.Constant(null, typeof(object));

            var translated = nullConstant.ToReadableString();

            Assert.Equal("null", translated);
        }

        [Fact]
        public void ShouldTranslateAnEnumMember()
        {
            var enumConstant = Expression.Constant(OddNumber.One, typeof(OddNumber));

            var translated = enumConstant.ToReadableString();

            Assert.Equal("OddNumber.One", translated);
        }

        [Fact]
        public void ShouldTranslateADefaultDate()
        {
            var dateConstant = Expression.Constant(default(DateTime));

            var translated = dateConstant.ToReadableString();

            Assert.Equal("default(DateTime)", translated);
        }

        [Fact]
        public void ShouldTranslateADateTimeWithNoTime()
        {
            var dateConstant = Expression.Constant(new DateTime(2015, 07, 02));

            var translated = dateConstant.ToReadableString();

            Assert.Equal("new DateTime(2015, 07, 02)", translated);
        }

        [Fact]
        public void ShouldTranslateADateTimeWithATime()
        {
            var dateConstant = Expression.Constant(new DateTime(2016, 08, 01, 10, 23, 45));

            var translated = dateConstant.ToReadableString();

            Assert.Equal("new DateTime(2016, 08, 01, 10, 23, 45)", translated);
        }

        [Fact]
        public void ShouldTranslateADateTimeWithMilliseconds()
        {
            var dateConstant = Expression.Constant(new DateTime(2017, 01, 10, 00, 00, 00, 123));

            var translated = dateConstant.ToReadableString();

            Assert.Equal("new DateTime(2017, 01, 10, 00, 00, 00, 123)", translated);
        }

        [Fact]
        public void ShouldTranslateADefaultTimeSpan()
        {
            var timeSpanConstant = Expression.Constant(default(TimeSpan));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("default(TimeSpan)", translated);
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfDays()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromDays(1));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("TimeSpan.FromDays(1)", translated);
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfHours()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromHours(2));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("TimeSpan.FromHours(2)", translated);
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfMinutes()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromMinutes(10));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("TimeSpan.FromMinutes(10)", translated);
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfSeconds()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromSeconds(58));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("TimeSpan.FromSeconds(58)", translated);
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromMilliseconds(923));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("TimeSpan.FromMilliseconds(923)", translated);
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfTicks()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromTicks(428));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("TimeSpan.FromTicks(428)", translated);
        }

        [Fact]
        public void ShouldTranslateADayTimeSpanWithMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(2, 3, 4, 5, 6));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("new TimeSpan(2, 3, 4, 5, 6)", translated);
        }

        [Fact]
        public void ShouldTranslateADayTimeSpanWithoutMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(3, 4, 5, 6));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("new TimeSpan(3, 4, 5, 6)", translated);
        }

        [Fact]
        public void ShouldTranslateAnHourTimeSpan()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(6, 5, 4));

            var translated = timeSpanConstant.ToReadableString();

            Assert.Equal("new TimeSpan(6, 5, 4)", translated);
        }

        [Fact]
        public void ShouldTranslateADefaultString()
        {
            var nullStringConstant = Expression.Default(typeof(string));

            var translated = nullStringConstant.ToReadableString();

            Assert.Equal("null", translated);
        }

        [Fact]
        public void ShouldEscapeTranslatedStrings()
        {
            var stringConstant = Expression.Constant("Escape: \"THIS\"!");

            var translated = stringConstant.ToReadableString();

            Assert.Equal("\"Escape: \\\"THIS\\\"!\"", translated);
        }

        [Fact]
        public void ShouldTranslateADefaultGuid()
        {
            var guidConstant = Expression.Constant(default(Guid));

            var translated = guidConstant.ToReadableString();

            Assert.Equal("default(Guid)", translated);
        }

        [Fact]
        public void ShouldTranslateARegex()
        {
            var regexConstant = Expression.Constant(new Regex("^[0-9]+$"));

            var translated = regexConstant.ToReadableString();

            Assert.Equal("Regex /* ^[0-9]+$ */", translated);
        }

        [Fact]
        public void ShouldTranslateAParameterlessFunc()
        {
            Func<object> stringFactory = () => "Factory!";
            var funcConstant = Expression.Constant(stringFactory);

            var translated = funcConstant.ToReadableString();

            Assert.Equal("Func<object>", translated);
        }

        [Fact]
        public void ShouldTranslateAnAction()
        {
            Action<int, long> numberAdder = (i, l) => Console.WriteLine(i + l);
            var actionConstant = Expression.Constant(numberAdder);

            var translated = actionConstant.ToReadableString();

            Assert.Equal("Action<int, long>", translated);
        }

        [Fact]
        public void ShouldTranslateAParameterisedAction()
        {
            Action<IDictionary<object, List<string>>> dictionaryPrinter = Console.WriteLine;
            var actionConstant = Expression.Constant(dictionaryPrinter);

            var translated = actionConstant.ToReadableString();

            Assert.Equal("Action<IDictionary<object, List<string>>>", translated);
        }

        [Fact]
        public void ShouldTranslateAFuncWithNestedGenericParameters()
        {
            Func<int?, FileInfo, Dictionary<IDictionary<FileInfo, string[]>, string>> dictionaryFactory =
                (i, fileInfo) => new Dictionary<IDictionary<FileInfo, string[]>, string>
                {
                    [new Dictionary<FileInfo, string[]> { [fileInfo] = new[] { fileInfo.ToString() } }] = i.ToString()
                };

            var funcConstant = Expression.Constant(dictionaryFactory);

            var translated = funcConstant.ToReadableString();

            Assert.Equal("Func<int?, FileInfo, Dictionary<IDictionary<FileInfo, string[]>, string>>", translated);
        }

        [Fact]
        public void ShouldTranslateAnActionWithMultipleNestedGenericParameters()
        {
            Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>> genericAction = fileInfo => { };

            var actionConstant = Expression.Constant(genericAction);

            var translated = actionConstant.ToReadableString();

            Assert.Equal("Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>>", translated);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/5
        [Fact]
        public void ShouldTranslateDbNullValue()
        {
            var dbParameter = Expression.Variable(typeof(DbParameter), "param");
            var parameterValue = Expression.Property(dbParameter, "Value");
            var dbNull = Expression.Constant(DBNull.Value, typeof(DBNull));
            var setParamToDbNull = Expression.Assign(parameterValue, dbNull);

            var translated = setParamToDbNull.ToReadableString();

            Assert.Equal("param.Value = DBNull.Value", translated);
        }

        [Fact]
        public void ShouldTranslateAnObjectConstant()
        {
            var objectConstant = Expression.Constant(123, typeof(object));

            var translated = objectConstant.ToReadableString();

            Assert.Equal("123", translated);
        }

        [Fact]
        public void ShouldTranslateALambdaConstant()
        {
            Expression<Func<int, int>> lambda =
                num => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index);

            var lambdaConstant = Expression.Constant(lambda, lambda.GetType());

            var translated = lambdaConstant.ToReadableString();

            const string EXPECTED = @"num => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index)";

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }
    }

    // ReSharper disable UnusedTypeParameter
    internal class GenericOne<T> { }

    internal class GenericTwo<T> { }

    internal class Generic<T1, T2, T3> { }
    // ReSharper restore UnusedTypeParameter

    internal enum OddNumber
    {
        One = 1
    }
}
