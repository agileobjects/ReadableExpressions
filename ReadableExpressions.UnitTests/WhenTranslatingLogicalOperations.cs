namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Xunit;

    public class WhenTranslatingLogicalOperations
    {
        [Fact]
        public void ShouldTranslateAnAndOperation()
        {
            Expression<Func<bool, bool, bool>> bothBoolsAreTheSame = (b1, b2) => b1 && b2;

            var translated = bothBoolsAreTheSame.Body.ToReadableString();

            Assert.Equal("(b1 && b2)", translated);
        }

        [Fact]
        public void ShouldTranslateABitwiseAndOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseAnd = (b1, b2) => b1 & b2;

            var translated = bitwiseAnd.Body.ToReadableString();

            Assert.Equal("(b1 & b2)", translated);
        }

        [Fact]
        public void ShouldTranslateAnOrOperation()
        {
            Expression<Func<bool, bool, bool>> eitherBoolsIsTrue = (b1, b2) => b1 || b2;

            var translated = eitherBoolsIsTrue.Body.ToReadableString();

            Assert.Equal("(b1 || b2)", translated);
        }

        [Fact]
        public void ShouldTranslateABitwiseOrOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseOr = (b1, b2) => b1 | b2;

            var translated = bitwiseOr.Body.ToReadableString();

            Assert.Equal("(b1 | b2)", translated);
        }

        [Fact]
        public void ShouldTranslateABitwiseExclusiveOrOperation()
        {
            Expression<Func<bool, bool, bool>> bitwiseExclusiveOr = (b1, b2) => b1 ^ b2;

            var translated = bitwiseExclusiveOr.Body.ToReadableString();

            Assert.Equal("(b1 ^ b2)", translated);
        }

        [Fact]
        public void ShouldTranslateABitwiseLeftShiftOperation()
        {
            Expression<Func<int, int, int>> bitwiseLeftShift = (i1, i2) => i1 << i2;

            var translated = bitwiseLeftShift.Body.ToReadableString();

            Assert.Equal("(i1 << i2)", translated);
        }

        [Fact]
        public void ShouldTranslateABitwiseRightShiftOperation()
        {
            Expression<Func<int, int, int>> bitwiseRightShift = (i1, i2) => i1 >> i2;

            var translated = bitwiseRightShift.Body.ToReadableString();

            Assert.Equal("(i1 >> i2)", translated);
        }

        [Fact]
        public void ShouldTranslateAUnaryPlusOperation()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var unaryPlus = Expression.UnaryPlus(intVariable);

            var translated = unaryPlus.ToReadableString();

            Assert.Equal("+i", translated);
        }

        [Fact]
        public void ShouldTranslateAOnesComplementOperation()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var onesComplement = Expression.OnesComplement(intVariable);

            var translated = onesComplement.ToReadableString();

            Assert.Equal("~i", translated);
        }

        [Fact]
        public void ShouldTranslateACoalesceOperation()
        {
            Expression<Func<bool?, bool, bool>> oneOrTwo = (b1, b2) => b1 ?? b2;

            var translated = oneOrTwo.Body.ToReadableString();

            Assert.Equal("(b1 ?? b2)", translated);
        }

        [Fact]
        public void ShouldTranslateAConditionalOperation()
        {
            Expression<Func<int, string>> whatSize = i => (i < 8) ? "Too small" : "Too big";

            var translated = whatSize.Body.ToReadableString();

            Assert.Equal("(i < 8) ? \"Too small\" : \"Too big\"", translated);
        }

        [Fact]
        public void ShouldTranslateAnIsTypeExpression()
        {
            Expression<Func<object, bool>> objectIsDisposable = o => o is IDisposable;

            var translated = objectIsDisposable.Body.ToReadableString();

            Assert.Equal("(o is IDisposable)", translated);
        }

        [Fact]
        public void ShouldTranslateAValueTypeTypeEqualExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intIsLong = Expression.TypeEqual(intVariable, typeof(long));

            var translated = intIsLong.ToReadableString();

            Assert.Equal("false", translated);
        }

        [Fact]
        public void ShouldTranslateANullableValueTypeTypeEqualExpression()
        {
            var nullableLongVariable = Expression.Variable(typeof(long?), "l");
            var nullableLongIsNullableLong = Expression.TypeEqual(nullableLongVariable, typeof(long?));

            var translated = nullableLongIsNullableLong.ToReadableString();

            Assert.Equal("(l != null)", translated);
        }

        [Fact]
        public void ShouldTranslateAConstantTypeEqualExpression()
        {
            var intConstant = Expression.Constant(123, typeof(int));
            var intConstantIsInt = Expression.TypeEqual(intConstant, typeof(int));

            var translated = intConstantIsInt.ToReadableString();

            Assert.Equal("true", translated);
        }

        [Fact]
        public void ShouldTranslateAnObjectTypeEqualExpression()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectIsString = Expression.TypeEqual(objectVariable, typeof(string));

            var translated = objectIsString.ToReadableString();

            Assert.Equal("((o != null) && (o.GetType() == typeof(string)))", translated);
        }

        [Fact]
        public void ShouldTranslateAnIsTrueExpression()
        {
            var boolVariable = Expression.Variable(typeof(bool), "b");
            var boolIsTrue = Expression.IsTrue(boolVariable);

            var translated = boolIsTrue.ToReadableString();

            Assert.Equal("b", translated);
        }

        [Fact]
        public void ShouldTranslateAnIsFalseExpression()
        {
            var boolVariable = Expression.Variable(typeof(bool), "b");
            var boolIsFalse = Expression.IsFalse(boolVariable);

            var translated = boolIsFalse.ToReadableString();

            Assert.Equal("!b", translated);
        }
    }
}