namespace AgileObjects.ReadableExpressions.UnitTests
{
    using Common;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingComparisons : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnEqualityExpression()
        {
            var intsAreEqual = CreateLambda((int i1, int i2) => i1 == i2);

            var translated = intsAreEqual.ToReadableString();

            translated.ShouldBe("(i1, i2) => i1 == i2");
        }

        [Fact]
        public void ShouldTranslateALessThanExpression()
        {
            var firstLessThanSecond = CreateLambda((int i1, int i2) => i1 < i2);

            var translated = firstLessThanSecond.ToReadableString();

            translated.ShouldBe("(i1, i2) => i1 < i2");
        }

        [Fact]
        public void ShouldTranslateALessThanOrEqualExpression()
        {
            var firstLessThanOrEqualToSecond = CreateLambda((int i1, int i2) => i1 <= i2);

            var translated = firstLessThanOrEqualToSecond.ToReadableString();

            translated.ShouldBe("(i1, i2) => i1 <= i2");
        }

        [Fact]
        public void ShouldTranslateAGreaterThanOrEqualExpression()
        {
            var firstGreaterThanOrEqualToSecond = CreateLambda((int i1, int i2) => i1 >= i2);

            var translated = firstGreaterThanOrEqualToSecond.ToReadableString();

            translated.ShouldBe("(i1, i2) => i1 >= i2");
        }

        [Fact]
        public void ShouldTranslateAGreaterThanExpression()
        {
            var firstGreaterThanSecond = CreateLambda((int i1, int i2) => i1 > i2);

            var translated = firstGreaterThanSecond.ToReadableString();

            translated.ShouldBe("(i1, i2) => i1 > i2");
        }

        [Fact]
        public void ShouldTranslateAnInequalityExpression()
        {
            var intsAreNotEqual = CreateLambda((int i1, int i2) => i1 != i2);

            var translated = intsAreNotEqual.ToReadableString();

            translated.ShouldBe("(i1, i2) => i1 != i2");
        }

        [Fact]
        public void ShouldAbbreviateBooleanTrueComparisons()
        {
            var boolVariable = Variable(typeof(bool), "couldBe");
            var boolIsTrue = Equal(boolVariable, Constant(true));

            var translated = boolIsTrue.ToReadableString();

            translated.ShouldBe("couldBe");
        }

        [Fact]
        public void ShouldAbbreviateBooleanFalseComparisons()
        {
            var boolVariable = Variable(typeof(bool), "couldBe");
            var boolIsFalse = Equal(Constant(false), boolVariable);

            var translated = boolIsFalse.ToReadableString();

            translated.ShouldBe("!couldBe");
        }

        [Fact]
        public void ShouldAbbreviateNotBooleanTrueComparisons()
        {
            var boolVariable = Variable(typeof(bool), "couldBe");
            var boolIsNotTrue = NotEqual(Constant(true), boolVariable);

            var translated = boolIsNotTrue.ToReadableString();

            translated.ShouldBe("!couldBe");
        }

        [Fact]
        public void ShouldAbbreviateNotBooleanFalseComparisons()
        {
            var boolVariable = Variable(typeof(bool), "couldBe");
            var boolIsNotFalse = NotEqual(boolVariable, Constant(false));

            var translated = boolIsNotFalse.ToReadableString();

            translated.ShouldBe("couldBe");
        }

        [Fact]
        public void ShouldAbbreviateDefaultBooleanComparisons()
        {
            var boolVariable = Variable(typeof(bool), "couldBe");
            var boolIsFalse = Equal(Default(typeof(bool)), boolVariable);

            var translated = boolIsFalse.ToReadableString();

            translated.ShouldBe("!couldBe");
        }
    }
}