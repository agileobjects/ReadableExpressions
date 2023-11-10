namespace AgileObjects.ReadableExpressions.UnitTests;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Common;
#if !NET35
using Xunit;
using static System.Linq.Expressions.Expression;
#else
using Fact = NUnit.Framework.TestAttribute;
using static Microsoft.Scripting.Ast.Expression;

[NUnit.Framework.TestFixture]
#endif
public class WhenTranslatingConstants : TestClassBase
{
    [Fact]
    public void ShouldTranslateAString()
    {
        var stringConstant = Constant("hello!", typeof(string));

        var translated = stringConstant.ToReadableString();

        translated.ShouldBe("\"hello!\"");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/43
    [Fact]
    public void ShouldTranslateAStringWithANullTerminatingCharacter()
    {
        var stringConstant = Constant("hel\0lo!", typeof(string));

        var translated = stringConstant.ToReadableString();

        translated.ShouldBe(@"""hel\0lo!""");
    }

    [Fact]
    public void ShouldTranslateAStringWithAnEscapedTabCharacter()
    {
        var stringConstant = Constant("hello\tthere!", typeof(string));

        var translated = stringConstant.ToReadableString();

        translated.ShouldBe("\"hello\tthere!\"");
    }

    [Fact]
    public void ShouldTranslateAVerbatimStringWithAnEscapedTabCharacter()
    {
        var stringConstant = Constant(@"hello\tthere!", typeof(string));

        var translated = stringConstant.ToReadableString();

        // In a verbatim string, \t isn't really a tab, it's '\\t';
        // to replicate that in a translated string constant we use
        // a verbatim '\\t':
        translated.ShouldBe(@"""hello\\tthere!""");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/99
    [Fact]
    public void ShouldTranslateAVerbatimStringWithDoubleQuotes()
    {
        const string VERBATIM = @"
12
""1""2
";
        var stringConstant = Constant(VERBATIM, typeof(string));

        var translated = stringConstant.ToReadableString();

        translated.ShouldBe(@"@""
12
""""1""""2
""");
    }

    [Fact]
    public void ShouldTranslateAStringWithACarriageReturnNewline()
    {
        var stringConstant = Constant("hello\r\nthere!", typeof(string));
        var translated = stringConstant.ToReadableString();

        translated.ShouldBe(@"@""hello
there!""");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/107
    [Fact]
    public void ShouldTranslateAStringWithJustACarriageReturnline()
    {
        var stringConstant = Constant("hello\rthere!", typeof(string));
        var translated = stringConstant.ToReadableString();

        translated.ShouldBe(@"@""hello" + '\r' + @"there!""");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/107
    [Fact]
    public void ShouldTranslateAStringWithJustANewline()
    {
        var stringConstant = Constant("hello\nthere!", typeof(string));
        var translated = stringConstant.ToReadableString();

        translated.ShouldBe(@"@""hello" + '\n' + @"there!""");
    }

    [Fact]
    public void ShouldTranslateABoolean()
    {
        var boolConstant = Constant(true, typeof(bool));

        var translated = boolConstant.ToReadableString();

        translated.ShouldBe("true");
    }

    [Fact]
    public void ShouldTranslateALong()
    {
        var longConstant = Constant(123L, typeof(long));

        var translated = longConstant.ToReadableString();

        translated.ShouldBe("123L");
    }

    [Fact]
    public void ShouldTranslateAWholeNumberFloat()
    {
        var floatConstant = Constant(890.0f, typeof(float));

        var translated = floatConstant.ToReadableString();

        translated.ShouldBe("890f");
    }

    [Fact]
    public void ShouldTranslateANonWholeNumberNullableFloat()
    {
        var floatConstant = Constant(12.34f, typeof(float?));

        var translated = floatConstant.ToReadableString();

        translated.ShouldBe("12.34f");
    }

    [Fact]
    public void ShouldTranslateAWholeNumberNullableDecimal()
    {
        var decimalConstant = Constant(456.00m, typeof(decimal?));

        var translated = decimalConstant.ToReadableString();

        translated.ShouldBe("456m");
    }

    [Fact]
    public void ShouldTranslateANonWholeNumberDecimal()
    {
        var decimalConstant = Constant(6373282.64738m, typeof(decimal));

        var translated = decimalConstant.ToReadableString();

        translated.ShouldBe("6373282.64738m");
    }

    [Fact]
    public void ShouldTranslateAWholeNumberDouble()
    {
        var doubleConstant = Constant(999.0, typeof(double));

        var translated = doubleConstant.ToReadableString();

        translated.ShouldBe("999d");
    }

    [Fact]
    public void ShouldTranslateANonWholeNumberDouble()
    {
        var doubleConstant = Constant(64739.7, typeof(double));

        var translated = doubleConstant.ToReadableString();

        translated.ShouldBe("64739.7d");
    }

    [Fact]
    public void ShouldTranslateAType()
    {
        var typeConstant = Constant(typeof(long), typeof(Type));

        var translated = typeConstant.ToReadableString();

        translated.ShouldBe("typeof(long)");
    }

    [Fact]
    public void ShouldTranslateAGenericRuntimeType()
    {
        var value = typeof(Dictionary<string, DateTime>);

        // ReSharper disable once PossibleMistakenCallToGetType.2
        var typeConstant = Constant(value, value.GetType());

        var translated = typeConstant.ToReadableString();

        translated.ShouldBe("typeof(Dictionary<string, DateTime>)");
    }

    [Fact]
    public void ShouldTranslateANullDefault()
    {
        var nullConstant = Constant(null, typeof(object));

        var translated = nullConstant.ToReadableString();

        translated.ShouldBe("null");
    }

    [Fact]
    public void ShouldTranslateAnEnumMember()
    {
        var enumConstant = Constant(OddNumber.One, typeof(OddNumber));

        var translated = enumConstant.ToReadableString();

        translated.ShouldBe("OddNumber.One");
    }

    [Fact]
    public void ShouldTranslateANullableEnumMember()
    {
        var enumConstant = Constant(OddNumber.One, typeof(OddNumber?));

        var translated = enumConstant.ToReadableString();

        translated.ShouldBe("OddNumber.One");
    }

    [Fact]
    public void ShouldTranslateANullNullableEnumMember()
    {
        var nullEnumConstant = Constant(default(OddNumber?));

        var translated = nullEnumConstant.ToReadableString();

        translated.ShouldBe("null");
    }

    [Fact]
    public void ShouldTranslateADefaultFlagsEnumMember()
    {
        var flagsEnumConstant = Constant(BindingFlags.Default, typeof(BindingFlags));

        var translated = flagsEnumConstant.ToReadableString();

        translated.ShouldBe("BindingFlags.Default");
    }

    [Fact]
    public void ShouldTranslateASingleFlagsEnumMember()
    {
        var flagsEnumConstant = Constant(BindingFlags.Instance, typeof(BindingFlags));

        var translated = flagsEnumConstant.ToReadableString();

        translated.ShouldBe("BindingFlags.Instance");
    }

    [Fact]
    public void ShouldTranslateACompositeFlagsEnumValue()
    {
        var flagsEnumConstant = Constant(BindingFlags.Public | BindingFlags.Static, typeof(BindingFlags));

        var translated = flagsEnumConstant.ToReadableString();

        translated.ShouldBe("BindingFlags.Static | BindingFlags.Public");
    }

    [Fact]
    public void ShouldTranslateAConjunctionFlagsEnumValue()
    {
        var flagsEnumConstant = Constant(AttributeTargets.All, typeof(AttributeTargets));

        var translated = flagsEnumConstant.ToReadableString();

        translated.ShouldBe("AttributeTargets.All");
    }

    [Fact]
    public void ShouldTranslateADefaultDate()
    {
        var dateConstant = Constant(default(DateTime));

        var translated = dateConstant.ToReadableString();

        translated.ShouldBe("default(DateTime)");
    }

    [Fact]
    public void ShouldTranslateADefaultGenericType()
    {
        var listConstant = Constant(default(List<string>));

        var translated = listConstant.ToReadableString();

        translated.ShouldBe("null");
    }

    [Fact]
    public void ShouldTranslateADateTimeWithNoTime()
    {
        var dateConstant = Constant(new DateTime(2015, 07, 02));

        var translated = dateConstant.ToReadableString();

        translated.ShouldBe("new DateTime(2015, 07, 02)");
    }

    [Fact]
    public void ShouldTranslateADateTimeWithATime()
    {
        var dateConstant = Constant(new DateTime(2016, 08, 01, 10, 23, 45));

        var translated = dateConstant.ToReadableString();

        translated.ShouldBe("new DateTime(2016, 08, 01, 10, 23, 45)");
    }

    [Fact]
    public void ShouldTranslateADateTimeWithMilliseconds()
    {
        var dateConstant = Constant(new DateTime(2017, 01, 10, 00, 00, 00, 123));

        var translated = dateConstant.ToReadableString();

        translated.ShouldBe("new DateTime(2017, 01, 10, 00, 00, 00, 123)");
    }

    [Fact]
    public void ShouldTranslateADefaultTimeSpan()
    {
        var timeSpanConstant = Constant(default(TimeSpan));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("default(TimeSpan)");
    }

    [Fact]
    public void ShouldTranslateATimeSpanOfDays()
    {
        var timeSpanConstant = Constant(TimeSpan.FromDays(1));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("TimeSpan.FromDays(1)");
    }

    [Fact]
    public void ShouldTranslateATimeSpanOfHours()
    {
        var timeSpanConstant = Constant(TimeSpan.FromHours(2));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("TimeSpan.FromHours(2)");
    }

    [Fact]
    public void ShouldTranslateATimeSpanOfMinutes()
    {
        var timeSpanConstant = Constant(TimeSpan.FromMinutes(10));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("TimeSpan.FromMinutes(10)");
    }

    [Fact]
    public void ShouldTranslateATimeSpanOfSeconds()
    {
        var timeSpanConstant = Constant(TimeSpan.FromSeconds(58));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("TimeSpan.FromSeconds(58)");
    }

    [Fact]
    public void ShouldTranslateATimeSpanOfMilliseconds()
    {
        var timeSpanConstant = Constant(TimeSpan.FromMilliseconds(923));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("TimeSpan.FromMilliseconds(923)");
    }

    [Fact]
    public void ShouldTranslateATimeSpanOfTicks()
    {
        var timeSpanConstant = Constant(TimeSpan.FromTicks(428));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("TimeSpan.FromTicks(428)");
    }

    [Fact]
    public void ShouldTranslateADayTimeSpanWithMilliseconds()
    {
        var timeSpanConstant = Constant(new TimeSpan(2, 3, 4, 5, 6));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("new TimeSpan(2, 3, 4, 5, 6)");
    }

    [Fact]
    public void ShouldTranslateADayTimeSpanWithoutMilliseconds()
    {
        var timeSpanConstant = Constant(new TimeSpan(3, 4, 5, 6));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("new TimeSpan(3, 4, 5, 6)");
    }

    [Fact]
    public void ShouldTranslateAnHourTimeSpan()
    {
        var timeSpanConstant = Constant(new TimeSpan(6, 5, 4));

        var translated = timeSpanConstant.ToReadableString();

        translated.ShouldBe("new TimeSpan(6, 5, 4)");
    }

    [Fact]
    public void ShouldTranslateADefaultString()
    {
        var nullStringConstant = Default(typeof(string));

        var translated = nullStringConstant.ToReadableString();

        translated.ShouldBe("null");
    }

    [Fact]
    public void ShouldEscapeTranslatedStrings()
    {
        var stringConstant = Constant("Escape: \"THIS\"!");

        var translated = stringConstant.ToReadableString();

        translated.ShouldBe("\"Escape: \\\"THIS\\\"!\"");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/119
    [Fact]
    public void ShouldTranslateAGuid()
    {
        var guid = Guid.NewGuid();
        var guidConstant = Constant(guid, typeof(Guid));

        var translated = guidConstant.ToReadableString();

        translated.ShouldBe($"new Guid(\"{guid}\")");
    }

    [Fact]
    public void ShouldTranslateANullableGuid()
    {
        var guid = Guid.NewGuid();
        var guidConstant = Constant(guid, typeof(Guid?));

        var translated = guidConstant.ToReadableString();

        translated.ShouldBe($"new Guid(\"{guid}\")");
    }

    [Fact]
    public void ShouldTranslateANullNullableGuid()
    {
        var guidConstant = Constant(null, typeof(Guid?));

        var translated = guidConstant.ToReadableString();

        translated.ShouldBe("null");
    }

    [Fact]
    public void ShouldTranslateADefaultGuid()
    {
        var guidConstant = Constant(default(Guid));

        var translated = guidConstant.ToReadableString();

        translated.ShouldBe("default(Guid)");
    }

    [Fact]
    public void ShouldTranslateARegex()
    {
        var regexConstant = Constant(new Regex("^[0-9]+$"));

        var translated = regexConstant.ToReadableString();

        translated.ShouldBe("new Regex(\"^[0-9]+$\")");
    }

    [Fact]
    public void ShouldTranslateAParameterlessFunc()
    {
        Func<object> stringFactory = () => "Factory!";
        var funcConstant = Constant(stringFactory);

        var translated = funcConstant.ToReadableString();

        translated.ShouldBe("Func<object>");
    }

    [Fact]
    public void ShouldTranslateAnAction()
    {
        Action<int, long> numberAdder = (i, l) => Console.WriteLine(i + l);
        var actionConstant = Constant(numberAdder);

        var translated = actionConstant.ToReadableString();

        translated.ShouldBe("Action<int, long>");
    }

    [Fact]
    public void ShouldTranslateAParameterisedAction()
    {
        Action<IDictionary<object, List<string>>> dictionaryPrinter = Console.WriteLine;
        var actionConstant = Constant(dictionaryPrinter);

        var translated = actionConstant.ToReadableString();

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

        var funcConstant = Constant(dictionaryFactory);

        var translated = funcConstant.ToReadableString();

        translated.ShouldBe("Func<int?, FileInfo, Dictionary<IDictionary<FileInfo, string[]>, string>>");
    }

    [Fact]
    public void ShouldTranslateAnActionWithMultipleNestedGenericParameters()
    {
        Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>> genericAction = fileInfo => { };

        var actionConstant = Constant(genericAction);

        var translated = actionConstant.ToReadableString();

        translated.ShouldBe("Action<Generic<GenericOne<int>, GenericTwo<long>, GenericTwo<long>>>");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/5
    [Fact]
    public void ShouldTranslateDbNullValue()
    {
        var dbParameter = Variable(typeof(DbParameter), "param");
        var parameterValue = Property(dbParameter, "Value");
        var dbNull = Constant(DBNull.Value, typeof(DBNull));
        var setParamToDbNull = Assign(parameterValue, dbNull);

        var translated = setParamToDbNull.ToReadableString();

        translated.ShouldBe("param.Value = DBNull.Value");
    }

    [Fact]
    public void ShouldTranslateAnObjectConstant()
    {
        var objectConstant = Constant(123, typeof(object));

        var translated = objectConstant.ToReadableString();

        translated.ShouldBe("123");
    }

    [Fact]
    public void ShouldTranslateALambdaConstant()
    {
        var lambda = CreateLambda((int num)
            => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index));

        var lambdaConstant = Constant(lambda, lambda.GetType());

        var translated = lambdaConstant.ToReadableString();

        const string EXPECTED = @"num => Enumerable.Range(num, 10).Select(i => new { Index = i }).Sum(d => d.Index)";

        translated.ShouldBe(EXPECTED.TrimStart());
    }

    [Fact]
    public void ShouldTranslateAStringArrayConstant()
    {
        var arrayConstant = Constant(new[] { "One", "Two", "Three" }, typeof(string[]));

        var translated = arrayConstant.ToReadableString();

        translated.ShouldBe("new[] { \"One\", \"Two\", \"Three\" }");
    }

    [Fact]
    public void ShouldTranslateAStringArrayICollectionConstant()
    {
        var arrayConstant = Constant(new[] { "Five", "Five", "Five" }, typeof(ICollection<string>));

        var translated = arrayConstant.ToReadableString();

        translated.ShouldBe("(ICollection<string>)new[] { \"Five\", \"Five\", \"Five\" }");
    }

    [Fact]
    public void ShouldTranslateATimeSpanArrayConstant()
    {
        var arrayConstant = Constant(
            new[] { TimeSpan.FromHours(1), TimeSpan.FromHours(2) },
            typeof(TimeSpan[]));

        var translated = arrayConstant.ToReadableString();

        translated.ShouldBe("new[] { TimeSpan.FromHours(1), TimeSpan.FromHours(2) }");
    }

    [Fact]
    public void ShouldTranslateAnEmptyIntArrayConstant()
    {
        var arrayConstant = Constant(new int[0], typeof(int[]));

        var translated = arrayConstant.ToReadableString();

        translated.ShouldBe("new int[0]");
    }

    [Fact]
    public void ShouldTranslateADictionaryConstant()
    {
        var dictionaryConstant = Constant(new Dictionary<int, int>(0));

        var translated = dictionaryConstant.ToReadableString();

        translated.ShouldBe("Dictionary<int, int>");
    }
}

// ReSharper disable UnusedTypeParameter
internal class GenericOne<T> { }

internal class GenericTwo<T> { }

internal class Generic<T1, T2, T3> { }
// ReSharper restore UnusedTypeParameter