namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using NetStandardPolyfills;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

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
            var intParameter = Expression.Parameter(typeof(int), "i");
            var checkedCast = Expression.ConvertChecked(intParameter, typeof(short));

            var checkedCastLambda = Expression.Lambda<Func<int, short>>(checkedCast, intParameter);

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
            var objectVariable = Expression.Variable(typeof(object), "o");
            var unboxObjectToInt = Expression.Unbox(objectVariable, typeof(int));

            var translated = ToReadableString(unboxObjectToInt);

            translated.ShouldBe("((int)o)");
        }

        // https://github.com/agileobjects/ReadableExpressions/issues/20
        [Fact]
        public void ShouldTranslateConversionWithCustomStaticMethod()
        {
            var stringParameter = Expression.Parameter(typeof(string), "str");
            var targetType = typeof(int);

            var body = Expression.Convert(
                stringParameter,
                targetType,
                targetType.GetPublicStaticMethod(nameof(int.Parse), stringParameter.Type));

            var stringToIntParseLambda = Expression.Lambda<Func<string, int>>(body, stringParameter);

            var translated = ToReadableString(stringToIntParseLambda.Body);

            translated.ShouldBe("int.Parse(str)");
        }
    }
}