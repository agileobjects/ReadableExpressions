namespace AgileObjects.ReadableExpressions.UnitTests
{
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingComparisons : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnEqualityExpression()
        {
            var intsAreEqual = CreateLambda((int i1, int i2) => i1 == i2);

            var translated = ToReadableString(intsAreEqual);

            translated.ShouldBe("(i1, i2) => i1 == i2");
        }

        [Fact]
        public void ShouldTranslateALessThanExpression()
        {
            var firstLessThanSecond = CreateLambda((int i1, int i2) => i1 < i2);

            var translated = ToReadableString(firstLessThanSecond);

            translated.ShouldBe("(i1, i2) => i1 < i2");
        }

        [Fact]
        public void ShouldTranslateALessThanOrEqualExpression()
        {
            var firstLessThanOrEqualToSecond = CreateLambda((int i1, int i2) => i1 <= i2);

            var translated = ToReadableString(firstLessThanOrEqualToSecond);

            translated.ShouldBe("(i1, i2) => i1 <= i2");
        }

        [Fact]
        public void ShouldTranslateAGreaterThanOrEqualExpression()
        {
            var firstGreaterThanOrEqualToSecond = CreateLambda((int i1, int i2) => i1 >= i2);

            var translated = ToReadableString(firstGreaterThanOrEqualToSecond);

            translated.ShouldBe("(i1, i2) => i1 >= i2");
        }

        [Fact]
        public void ShouldTranslateAGreaterThanExpression()
        {
            var firstGreaterThanSecond = CreateLambda((int i1, int i2) => i1 > i2);

            var translated = ToReadableString(firstGreaterThanSecond);

            translated.ShouldBe("(i1, i2) => i1 > i2");
        }

        [Fact]
        public void ShouldTranslateAnInequalityExpression()
        {
            var intsAreNotEqual = CreateLambda((int i1, int i2) => i1 != i2);

            var translated = ToReadableString(intsAreNotEqual);

            translated.ShouldBe("(i1, i2) => i1 != i2");
        }

        [Fact]
        public void ShouldAbbreviateBooleanTrueComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsTrue = Expression.Equal(boolVariable, Expression.Constant(true));

            var translated = ToReadableString(boolIsTrue);

            translated.ShouldBe("couldBe");
        }

        [Fact]
        public void ShouldAbbreviateBooleanFalseComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsFalse = Expression.Equal(Expression.Constant(false), boolVariable);

            var translated = ToReadableString(boolIsFalse);

            translated.ShouldBe("!couldBe");
        }

        [Fact]
        public void ShouldAbbreviateNotBooleanTrueComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsNotTrue = Expression.NotEqual(Expression.Constant(true), boolVariable);

            var translated = ToReadableString(boolIsNotTrue);

            translated.ShouldBe("!couldBe");
        }

        [Fact]
        public void ShouldAbbreviateNotBooleanFalseComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsNotFalse = Expression.NotEqual(boolVariable, Expression.Constant(false));

            var translated = ToReadableString(boolIsNotFalse);

            translated.ShouldBe("couldBe");
        }

        [Fact]
        public void ShouldAbbreviateDefaultBooleanComparisons()
        {
            var boolVariable = Expression.Variable(typeof(bool), "couldBe");
            var boolIsFalse = Expression.Equal(Expression.Default(typeof(bool)), boolVariable);

            var translated = ToReadableString(boolIsFalse);

            translated.ShouldBe("!couldBe");
        }
    }
}