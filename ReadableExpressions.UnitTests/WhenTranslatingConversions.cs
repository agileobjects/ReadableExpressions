namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using Xunit;

    public class WhenTranslatingConversions
    {
        [Fact]
        public void ShouldTranslateACastExpression()
        {
            Expression<Func<int, double>> intToDouble = i => (double)i;

            var translated = intToDouble.ToReadableString();

            Assert.Equal("i => (double)i", translated);
        }

        [Fact]
        public void ShouldTranslateACheckedCastExpression()
        {
            var intParameter = Expression.Parameter(typeof(int), "i");
            var checkedCast = Expression.ConvertChecked(intParameter, typeof(short));

            var checkedCastLambda = Expression.Lambda<Func<int, short>>(checkedCast, intParameter);

            var translated = checkedCastLambda.ToReadableString();

            Assert.Equal("i => (short)i", translated);
        }

        [Fact]
        public void ShouldTranslateACastToNullableExpression()
        {
            Expression<Func<long, long?>> longToNullable = l => (long?)l;

            var translated = longToNullable.ToReadableString();

            Assert.Equal("l => (long?)l", translated);
        }

        [Fact]
        public void ShouldUseParenthesisInCasting()
        {
            Expression<Func<object, int>> castDateTimeHour = o => ((DateTime)o).Hour;

            var translated = castDateTimeHour.ToReadableString();

            Assert.Equal("o => ((DateTime)o).Hour", translated);
        }

        [Fact]
        public void ShouldTranslateANegationExpression()
        {
            Expression<Func<bool, bool>> negator = b => !b;

            var translated = negator.ToReadableString();

            Assert.Equal("b => !b", translated);
        }

        [Fact]
        public void ShouldTranslateAnAsCastExpression()
        {
            Expression<Func<Stream, IDisposable>> streamAsDisposable = stream => stream as IDisposable;

            var translated = streamAsDisposable.Body.ToReadableString();

            Assert.Equal("(stream as IDisposable)", translated);
        }

        [Fact]
        public void ShouldTranslateAnUnboxExpression()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var unboxObjectToInt = Expression.Unbox(objectVariable, typeof(int));

            var translated = unboxObjectToInt.ToReadableString();

            Assert.Equal("((int)o)", translated);
        }
    }
}