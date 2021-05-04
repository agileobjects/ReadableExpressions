namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using Common;
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

            var translated = intToDouble.ToReadableString();

            translated.ShouldBe("i => (double)i");
        }

        [Fact]
        public void ShouldTranslateACheckedCastExpression()
        {
            var intParameter = Parameter(typeof(int), "i");
            var checkedCast = ConvertChecked(intParameter, typeof(short));

            var checkedCastLambda = Lambda<Func<int, short>>(checkedCast, intParameter);

            var translated = checkedCastLambda.ToReadableString();

            translated.ShouldBe("i => (short)i");
        }

        [Fact]
        public void ShouldTranslateACastToNullableExpression()
        {
            var longToNullable = CreateLambda((long l) => (long?)l);

            var translated = longToNullable.ToReadableString();

            translated.ShouldBe("l => (long?)l");
        }

        [Fact]
        public void ShouldUseParenthesisInCasting()
        {
            var castDateTimeHour = CreateLambda((object o) => ((DateTime)o).Hour);

            var translated = castDateTimeHour.ToReadableString();

            translated.ShouldBe("o => ((DateTime)o).Hour");
        }

        [Fact]
        public void ShouldTranslateANegationExpression()
        {
            var negator = CreateLambda((bool b) => !b);

            var translated = negator.ToReadableString();

            translated.ShouldBe("b => !b");
        }

        [Fact]
        public void ShouldTranslateAnAsCastExpression()
        {
            var streamAsDisposable = CreateLambda((Stream stream) => stream as IDisposable);

            var translated = streamAsDisposable.Body.ToReadableString();

            translated.ShouldBe("stream as IDisposable");
        }

        [Fact]
        public void ShouldTranslateABoxingExpression()
        {
            var intVariable = Variable(typeof(int), "i");
            var boxInt = Convert(intVariable, typeof(object));

            var translated = boxInt.ToReadableString();

            translated.ShouldBe("i");
        }

        [Fact]
        public void ShouldTranslateAnUpcastToObjectExpression()
        {
            var streamVariable = Variable(typeof(Stream), "stream");
            var upcastStreamToObject = Convert(streamVariable, typeof(object));

            var translated = upcastStreamToObject.ToReadableString();

            translated.ShouldBe("(object)stream");
        }

        [Fact]
        public void ShouldTranslateAnUnboxingExpression()
        {
            var objectVariable = Variable(typeof(object), "o");
            var unboxObjectToInt = Unbox(objectVariable, typeof(int));

            var translated = unboxObjectToInt.ToReadableString();

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

            var translated = stringToIntParseLambda.Body.ToReadableString();

            translated.ShouldBe("int.Parse(str)");
        }
    }
}