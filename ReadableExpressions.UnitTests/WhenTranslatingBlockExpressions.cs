namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingBlockExpressions
    {
        [TestMethod]
        public void ShouldTranslateANoVariableBlockWithNoReturnValue()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            Expression<Func<int>> read = () => Console.Read();
            Expression<Action> beep = () => Console.Beep();

            var consoleBlock = Expression.Block(writeLine.Body, read.Body, beep.Body);

            var translated = consoleBlock.ToReadableString();

            const string EXPECTED = @"
Console.WriteLine();
Console.Read();
Console.Beep();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateANoVariableBlockLambdaWithAReturnValue()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            Expression<Func<int>> read = () => Console.Read();

            var consoleBlock = Expression.Block(writeLine.Body, read.Body);
            var consoleLambda = Expression.Lambda<Func<int>>(consoleBlock);

            var translated = consoleLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    Console.WriteLine();
    return Console.Read();
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var assignZeroToCount = Expression.Assign(countVariable, Expression.Constant(0));
            var incrementCount = Expression.Increment(countVariable);
            var returnVoid = Expression.Default(typeof(void));

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignZeroToCount,
                incrementCount,
                returnVoid);

            var translated = countBlock.ToReadableString();

            const string EXPECTED = @"
var count = 0;
++count;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockLambdaWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var assignTenToCount = Expression.Assign(countVariable, Expression.Constant(10));
            var decrementCount = Expression.Decrement(countVariable);

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignTenToCount,
                decrementCount);

            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    var count = 10;
    --count;
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockLambdaWithAReturnValue()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var countEqualsZero = Expression.Assign(countVariable, Expression.Constant(0));
            var incrementCount = Expression.Increment(countVariable);
            var returnCount = countVariable;

            var countBlock = Expression.Block(
                new[] { countVariable },
                countEqualsZero,
                incrementCount,
                returnCount);

            var countLambda = Expression.Lambda<Func<int>>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    var count = 0;
    ++count;
    return count;
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleAccessVariableBlockWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var assignTenToCount = Expression.Assign(countVariable, Expression.Constant(10));
            var addTwoToCount = Expression.AddAssign(countVariable, Expression.Constant(2));

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignTenToCount,
                addTwoToCount);

            var translated = countBlock.ToReadableString();

            const string EXPECTED = @"
var count = 10;
count += 2;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
