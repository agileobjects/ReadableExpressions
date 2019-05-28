namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using NetStandardPolyfills;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingConversions : TestClassBase
    {
        [Fact]
        public void ShouldTranslateACastExpression()
        {
            var intToDouble = CreateLambda((int i) => (double)i);

            var translated = ToReadableString(intToDouble);

            translated.ShouldBe("i => (double)i");
        }

        [Fact]
        public void ShouldTranslateACheckedCastExpression()
        {
            var intParameter = Parameter(typeof(int), "i");
            var checkedCast = ConvertChecked(intParameter, typeof(short));

            var checkedCastLambda = Lambda<Func<int, short>>(checkedCast, intParameter);

            var translated = ToReadableString(checkedCastLambda);

            translated.ShouldBe("i => (short)i");
        }

        [Fact]
        public void ShouldTranslateACastToNullableExpression()
        {
            var longToNullable = CreateLambda((long l) => (long?)l);

            var translated = ToReadableString(longToNullable);

            translated.ShouldBe("l => (long?)l");
        }

        [Fact]
        public void ShouldUseParenthesisInCasting()
        {
            var castDateTimeHour = CreateLambda((object o) => ((DateTime)o).Hour);

            var translated = ToReadableString(castDateTimeHour);

            translated.ShouldBe("o => ((DateTime)o).Hour");
        }

        [Fact]
        public void ShouldTranslateANegationExpression()
        {
            var negator = CreateLambda((bool b) => !b);

            var translated = ToReadableString(negator);

            translated.ShouldBe("b => !b");
        }

        [Fact]
        public void ShouldTranslateAnAsCastExpression()
        {
            var streamAsDisposable = CreateLambda((Stream stream) => stream as IDisposable);

            var translated = ToReadableString(streamAsDisposable.Body);

            translated.ShouldBe("stream as IDisposable");
        }

        [Fact]
        public void ShouldTranslateAnUnboxExpression()
        {
            var objectVariable = Variable(typeof(object), "o");
            var unboxObjectToInt = Unbox(objectVariable, typeof(int));

            var translated = ToReadableString(unboxObjectToInt);

            translated.ShouldBe("(int)o");
        }

        // https://github.com/agileobjects/ReadableExpressions/issues/20
        [Fact]
        public void ShouldTranslateConversionWithCustomStaticMethod()
        {
            var stringParameter = Parameter(typeof(string), "str");
            var targetType = typeof(int);

            var body = Convert(
                stringParameter,
                targetType,
                targetType.GetPublicStaticMethod(nameof(int.Parse), stringParameter.Type));

            var stringToIntParseLambda = Lambda<Func<string, int>>(body, stringParameter);

            var translated = ToReadableString(stringToIntParseLambda.Body);

            translated.ShouldBe("int.Parse(str)");
        }
    }
}