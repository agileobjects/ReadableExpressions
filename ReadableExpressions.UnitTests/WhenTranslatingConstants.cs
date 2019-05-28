namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingConstants : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAString()
        {
            var stringConstant = Expression.Constant("hello!", typeof(string));

            var translated = ToReadableString(stringConstant);

            translated.ShouldBe("\"hello!\"");
        }

        [Fact]
        public void ShouldTranslateABoolean()
        {
            var boolConstant = Expression.Constant(true, typeof(bool));

            var translated = ToReadableString(boolConstant);

            translated.ShouldBe("true");
        }

        [Fact]
        public void ShouldTranslateALong()
        {
            var longConstant = Expression.Constant(123L, typeof(long));

            var translated = ToReadableString(longConstant);

            translated.ShouldBe("123L");
        }

        [Fact]
        public void ShouldTranslateAWholeNumberFloat()
        {
            var floatConstant = Expression.Constant(890.0f, typeof(float));

            var translated = ToReadableString(floatConstant);

            translated.ShouldBe("890f");
        }

        [Fact]
        public void ShouldTranslateANonWholeNumberNullableFloat()
        {
            var floatConstant = Expression.Constant(12.34f, typeof(float?));

            var translated = ToReadableString(floatConstant);

            translated.ShouldBe("12.34f");
        }

        [Fact]
        public void ShouldTranslateAWholeNumberNullableDecimal()
        {
            var decimalConstant = Expression.Constant(456.00m, typeof(decimal?));

            var translated = ToReadableString(decimalConstant);

            translated.ShouldBe("456m");
        }

        [Fact]
        public void ShouldTranslateANonWholeNumberDecimal()
        {
            var decimalConstant = Expression.Constant(6373282.64738m, typeof(decimal));

            var translated = ToReadableString(decimalConstant);

            translated.ShouldBe("6373282.64738m");
        }

        [Fact]
        public void ShouldTranslateAWholeNumberDouble()
        {
            var doubleConstant = Expression.Constant(999.0, typeof(double));

            var translated = ToReadableString(doubleConstant);

            translated.ShouldBe("999d");
        }

        [Fact]
        public void ShouldTranslateANonWholeNumberDouble()
        {
            var doubleConstant = Expression.Constant(64739.7, typeof(double));

            var translated = ToReadableString(doubleConstant);

            translated.ShouldBe("64739.7d");
        }

        [Fact]
        public void ShouldTranslateAType()
        {
            var typeConstant = Expression.Constant(typeof(long), typeof(Type));

            var translated = ToReadableString(typeConstant);

            translated.ShouldBe("typeof(long)");
        }

        [Fact]
        public void ShouldTranslateAGenericRuntimeType()
        {
            var value = typeof(Dictionary<string, DateTime>);

            // ReSharper disable once PossibleMistakenCallToGetType.2
            var typeConstant = Expression.Constant(value, value.GetType());

            var translated = ToReadableString(typeConstant);

            translated.ShouldBe("typeof(Dictionary<string, DateTime>)");
        }

        [Fact]
        public void ShouldTranslateANullDefault()
        {
            var nullConstant = Expression.Constant(null, typeof(object));

            var translated = ToReadableString(nullConstant);

            translated.ShouldBe("null");
        }

        [Fact]
        public void ShouldTranslateAnEnumMember()
        {
            var enumConstant = Expression.Constant(OddNumber.One, typeof(OddNumber));

            var translated = ToReadableString(enumConstant);

            translated.ShouldBe("OddNumber.One");
        }

        [Fact]
        public void ShouldTranslateADefaultDate()
        {
            var dateConstant = Expression.Constant(default(DateTime));

            var translated = ToReadableString(dateConstant);

            translated.ShouldBe("default(DateTime)");
        }

        [Fact]
        public void ShouldTranslateADateTimeWithNoTime()
        {
            var dateConstant = Expression.Constant(new DateTime(2015, 07, 02));

            var translated = ToReadableString(dateConstant);

            translated.ShouldBe("new DateTime(2015, 07, 02)");
        }

        [Fact]
        public void ShouldTranslateADateTimeWithATime()
        {
            var dateConstant = Expression.Constant(new DateTime(2016, 08, 01, 10, 23, 45));

            var translated = ToReadableString(dateConstant);

            translated.ShouldBe("new DateTime(2016, 08, 01, 10, 23, 45)");
        }

        [Fact]
        public void ShouldTranslateADateTimeWithMilliseconds()
        {
            var dateConstant = Expression.Constant(new DateTime(2017, 01, 10, 00, 00, 00, 123));

            var translated = ToReadableString(dateConstant);

            translated.ShouldBe("new DateTime(2017, 01, 10, 00, 00, 00, 123)");
        }

        [Fact]
        public void ShouldTranslateADefaultTimeSpan()
        {
            var timeSpanConstant = Expression.Constant(default(TimeSpan));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("default(TimeSpan)");
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfDays()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromDays(1));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("TimeSpan.FromDays(1)");
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfHours()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromHours(2));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("TimeSpan.FromHours(2)");
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfMinutes()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromMinutes(10));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("TimeSpan.FromMinutes(10)");
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfSeconds()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromSeconds(58));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("TimeSpan.FromSeconds(58)");
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromMilliseconds(923));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("TimeSpan.FromMilliseconds(923)");
        }

        [Fact]
        public void ShouldTranslateATimeSpanOfTicks()
        {
            var timeSpanConstant = Expression.Constant(TimeSpan.FromTicks(428));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("TimeSpan.FromTicks(428)");
        }

        [Fact]
        public void ShouldTranslateADayTimeSpanWithMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(2, 3, 4, 5, 6));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("new TimeSpan(2, 3, 4, 5, 6)");
        }

        [Fact]
        public void ShouldTranslateADayTimeSpanWithoutMilliseconds()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(3, 4, 5, 6));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("new TimeSpan(3, 4, 5, 6)");
        }

        [Fact]
        public void ShouldTranslateAnHourTimeSpan()
        {
            var timeSpanConstant = Expression.Constant(new TimeSpan(6, 5, 4));

            var translated = ToReadableString(timeSpanConstant);

            translated.ShouldBe("new TimeSpan(6, 5, 4)");
        }

        [Fact]
        public void ShouldTranslateADefaultString()
        {
            var nullStringConstant = Expression.Default(typeof(string));

            var translated = ToReadableString(nullStringConstant);

            translated.ShouldBe("null");
        }

        [Fact]
        public void ShouldEscapeTranslatedStrings()
        {
            var stringConstant = Expression.Constant("Escape: \"THIS\"!");

            var translated = ToReadableString(stringConstant);

            translated.ShouldBe("\"Escape: \\\"THIS\\\"!\"");
        }

        [Fact]
        public void ShouldTranslateADefaultGuid()
        {
            var guidConstant = Expression.Constant(default(Guid));

            var translated = ToReadableString(guidConstant);

            translated.ShouldBe("default(Guid)");
        }

        [Fact]
        public void ShouldTranslateARegex()
        {
            var regexConstant = Expression.Constant(new Regex("^[0-9]+$"));

            var translated = ToReadableString(regexConstant);

            translated.ShouldBe("Regex /* ^[0-9]+$ */");
        }

        [Fact]
        public void ShouldTranslateAParameterlessFunc()
        {
            Func<object> stringFactory = () => "Factory!";
            var funcConstant = Expression.Constant(stringFactory);

            var translated = ToReadableString(funcConstant);

            translated.ShouldBe("Func<object>");
        }

        [Fact]
        public void ShouldTranslateAnAction()
        {
            Action<int, long> numberAdder = (i, l) => Console.WriteLine(i + l);
            var actionConstant = Expression.Constant(numberAdder);

            var translated = ToReadableString(actionConstant);

            translated.ShouldBe("Action<int, long>");
        }

        [Fact]
        public void ShouldTranslateAParameterisedAction()
        {
            Action<IDictionary<object, List<string>>> dictionaryPrinter = Console.WriteLine;
            var actionConstant = Expression.Constant(dictionaryPrinter);

            var translated = ToReadableString(actionConstant);

            translated.ShouldBe("Action<IDictionary<object, List<string>>>");
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

            var translated = ToReadableString(funcConstant);

            translated.ShouldBe("Func<int?, FileInfo, Dictionary<IDictionary<FileInfo, string[]>, string>>");
        }

        [Fact]
        public void ShouldTranslateAnActionWithMultipleNestedGenericParameters()
        {
            Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>> genericAction = fileInfo => { };

            var actionConstant = Expression.Constant(genericAction);

            var translated = ToReadableString(actionConstant);

            translated.ShouldBe("Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>>");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/5
        [Fact]
        public void ShouldTranslateDbNullValue()
        {
            var dbParameter = Expression.Variable(typeof(DbParameter), "param");
            var parameterValue = Expression.Property(dbParameter, "Value");
            var dbNull = Expression.Constant(DBNull.Value, typeof(DBNull));
            var setParamToDbNull = Expression.Assign(parameterValue, dbNull);

            var translated = ToReadableString(setParamToDbNull);

            translated.ShouldBe("param.Value = DBNull.Value");
        }

        [Fact]
        public void ShouldTranslateAnObjectConstant()
        {
            var objectConstant = Expression.Constant(123, typeof(object));

            var translated = ToReadableString(objectConstant);

            translated.ShouldBe("123");
        }

        [Fact]
        public void ShouldTranslateALambdaConstant()
        {
            var lambda = CreateLambda((int num)
                => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index));

            var lambdaConstant = Expression.Constant(lambda, lambda.GetType());

            var translated = ToReadableString(lambdaConstant);

            const string EXPECTED = @"num => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/35
        [Fact]
        public void ShouldUseAUserDefinedConstantTranslator()
        {
            var stringConstant = Expression.Constant("hello!", typeof(string));

            var translated = ToReadableString(
                stringConstant, 
                settings => settings.TranslateConstantsUsing((t, v) => "custom!"));

            translated.ShouldBe("custom!");
        }
        
        [Fact]
        public void ShouldUseADefaultValueIfUserDefinedConstantTranslatorReturnsNull()
        {
            var stringConstant = Expression.Constant("hello!", typeof(string));

            var translated = ToReadableString(
                stringConstant, 
                settings => settings.TranslateConstantsUsing((t, v) => null));

            translated.ShouldBe("null");
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
