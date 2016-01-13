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
        public void ShouldTranslateANoVariableBlockWithAReturnValue()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            Expression<Func<int>> read = () => Console.Read();

            var consoleBlock = Expression.Block(writeLine.Body, read.Body);

            var translated = consoleBlock.ToReadableString();

            const string EXPECTED = @"
Console.WriteLine();
return Console.Read();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var countEqualsZero = Expression.Assign(countVariable, Expression.Constant(0));
            var incrementCount = Expression.Increment(countVariable);
            var noReturnValue = Expression.Default(typeof(void));

            var consoleBlock = Expression.Block(
                new[] { countVariable },
                countEqualsZero,
                incrementCount,
                noReturnValue);

            var translated = consoleBlock.ToReadableString();

            const string EXPECTED = @"
var count = 0;
++count;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
