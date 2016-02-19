namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingBlocks
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
int count;
count = 0;
++count;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockLambdaWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(short), "count");
            var assignTenToCount = Expression.Assign(countVariable, Expression.Constant((short)10));
            var decrementCount = Expression.Decrement(countVariable);

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignTenToCount,
                decrementCount);

            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    short count;
    count = 10;
    --count;
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockLambdaWithAReturnValue()
        {
            var countVariable = Expression.Variable(typeof(ushort), "count");
            var countEqualsZero = Expression.Assign(countVariable, Expression.Constant((ushort)0));
            var incrementCount = Expression.Increment(countVariable);
            var returnCount = countVariable;

            var countBlock = Expression.Block(
                new[] { countVariable },
                countEqualsZero,
                incrementCount,
                returnCount);

            var countLambda = Expression.Lambda<Func<ushort>>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    ushort count;
    count = 0;
    ++count;
    return count;
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleAccessVariableBlockWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(ulong), "count");
            var assignTenToCount = Expression.Assign(countVariable, Expression.Constant((ulong)10));
            var addTwoToCount = Expression.AddAssign(countVariable, Expression.Constant((ulong)2));

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignTenToCount,
                addTwoToCount);

            var translated = countBlock.ToReadableString();

            const string EXPECTED = @"
ulong count;
count = 10;
count += 2;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleVariableBlockWithNoReturnValue()
        {
            var countOneVariable = Expression.Variable(typeof(int), "countOne");
            var countTwoVariable = Expression.Variable(typeof(int), "countTwo");
            var countThreeVariable = Expression.Variable(typeof(byte), "countThree");
            var assignOneToCountOne = Expression.Assign(countOneVariable, Expression.Constant(1));
            var assignTwoToCountTwo = Expression.Assign(countTwoVariable, Expression.Constant(2));
            var sumCounts = Expression.Add(countOneVariable, countTwoVariable);
            var castSumToBye = Expression.Convert(sumCounts, typeof(byte));
            var assignSumToCountThree = Expression.Assign(countThreeVariable, castSumToBye);

            var countBlock = Expression.Block(
                new[] { countOneVariable, countTwoVariable, countThreeVariable },
                assignOneToCountOne,
                assignTwoToCountTwo,
                assignSumToCountThree);

            var translated = countBlock.ToReadableString();

            const string EXPECTED = @"
int countOne, countTwo;
byte countThree;
countOne = 1;
countTwo = 2;
countThree = ((byte)(countOne + countTwo));";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateNestedBlocks()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            Expression<Action> beep = () => Console.Beep();

            var innerBlock = Expression.Block(writeLine.Body, beep.Body);
            var outerBlock = Expression.Block(innerBlock, writeLine.Body);

            var translated = outerBlock.ToReadableString();

            const string EXPECTED = @"
Console.WriteLine();
Console.Beep();
Console.WriteLine();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateABlockWithAComment()
        {
            var comment = ReadableExpression.Comment("Anyone listening?");
            Expression<Action> beep = () => Console.Beep();

            var commentedBeep = Expression.Block(comment, beep.Body);

            var translated = commentedBeep.ToReadableString();

            const string EXPECTED = @"
// Anyone listening?
Console.Beep();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldIgnoreAVariableOnlyBlockStatement()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var @false = Expression.Constant(false, typeof(bool));

            var countBlock = Expression.Block(countVariable, @false);

            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() => false";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
