namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingMathsOperations
    {
        [TestMethod]
        public void ShouldTranslateAnAdditionExpression()
        {
            Expression<Func<int, int, int>> addInts = (i1, i2) => i1 + i2;

            var translated = addInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 + i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateASubtractionExpression()
        {
            Expression<Func<int, int, int>> subtractInts = (i1, i2) => i1 - i2;

            var translated = subtractInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 - i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateANegationExpression()
        {
            Expression<Func<int, int>> negateInts = i => -i;

            var translated = negateInts.ToReadableString();

            Assert.AreEqual("i => -i", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultiplicationExpression()
        {
            Expression<Func<int, int, int>> multiplyInts = (i1, i2) => i1 * i2;

            var translated = multiplyInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 * i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultiplicationPowerExpression()
        {
            var variableOne = Expression.Variable(typeof(double), "d1");
            var variableTwo = Expression.Variable(typeof(double), "d2");
            var varOneToThePowerOfVarTwo = Expression.Power(variableOne, variableTwo);

            var translated = varOneToThePowerOfVarTwo.ToReadableString();

            Assert.AreEqual("(d1 ** d2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateADivisionExpression()
        {
            Expression<Func<int, int, int>> divideInts = (i1, i2) => i1 / i2;

            var translated = divideInts.ToReadableString();

            Assert.AreEqual("(i1, i2) => i1 / i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAModuloExpression()
        {
            Expression<Func<int, int, int>> findModulo = (i1, i2) => i1 % i2;

            var translated = findModulo.Body.ToReadableString();

            Assert.AreEqual("(i1 % i2)", translated);
        }

        [TestMethod]
        public void ShouldMaintainExpressionParenthesis()
        {
            Expression<Func<int, int, int>> operationWithBrackets = (i1, i2) => (i1 / i2) * i1;

            var translated = operationWithBrackets.ToReadableString();

            Assert.AreEqual("(i1, i2) => (i1 / i2) * i1", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIncrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var increment = Expression.Increment(intVariable);

            var translated = increment.ToReadableString();

            Assert.AreEqual("++i", translated);
        }

        [TestMethod]
        public void ShouldTranslateAPostIncrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var postIncrement = Expression.PostIncrementAssign(intVariable);

            var translated = postIncrement.ToReadableString();

            Assert.AreEqual("i++", translated);
        }

        [TestMethod]
        public void ShouldTranslateADecrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var decrement = Expression.Decrement(intVariable);

            var translated = decrement.ToReadableString();

            Assert.AreEqual("--i", translated);
        }

        [TestMethod]
        public void ShouldTranslateAPostDecrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var postDecrement = Expression.PostDecrementAssign(intVariable);

            var translated = postDecrement.ToReadableString();

            Assert.AreEqual("i--", translated);
        }
    }
}