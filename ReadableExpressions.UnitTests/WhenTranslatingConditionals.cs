namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingConditionals
    {
        [TestMethod]
        public void ShouldTranslateASingleLineIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1, typeof(int));
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            Expression<Action> writeLessThan = () => Console.Write("Less than");
            var ifLessThanOneThenWrite = Expression.IfThen(intVariableLessThanOne, writeLessThan.Body);

            var translated = ifLessThanOneThenWrite.ToReadableString();

            const string EXPECTED = @"
if (i < 1)
{
    Console.Write(""Less than"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleLineIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1, typeof(int));
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            Expression<Action> writeThere = () => Console.WriteLine("There");
            var writeBlock = Expression.Block(writeHello.Body, writeThere.Body);
            var ifLessThanOneThenWrite = Expression.IfThen(intVariableLessThanOne, writeBlock);

            var translated = ifLessThanOneThenWrite.ToReadableString();

            const string EXPECTED = @"
if (i < 1)
{
    Console.WriteLine(""Hello"");
    Console.WriteLine(""There"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIfElseStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1, typeof(int));
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            Expression<Action> writeGoodbye = () => Console.WriteLine("Goodbye");
            var writeHelloOrGoodbye = Expression.IfThenElse(intVariableLessThanOne, writeHello.Body, writeGoodbye.Body);

            var translated = writeHelloOrGoodbye.ToReadableString();

            const string EXPECTED = @"
if (i < 1)
{
    Console.WriteLine(""Hello"");
}
else
{
    Console.WriteLine(""Goodbye"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateMultipleLineIfElseStatements()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var zero = Expression.Constant(0, typeof(int));
            var intVariableEqualsZero = Expression.Equal(intVariable, zero);
            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            Expression<Action> writeGoodbye = () => Console.WriteLine("Goodbye");
            var helloThenGoodbye = Expression.Block(writeHello.Body, writeGoodbye.Body);
            var goodbyeThenHello = Expression.Block(writeGoodbye.Body, writeHello.Body);
            var writeHelloAndGoodbye = Expression.IfThenElse(intVariableEqualsZero, helloThenGoodbye, goodbyeThenHello);

            var translated = writeHelloAndGoodbye.ToReadableString();

            const string EXPECTED = @"
if (i == 0)
{
    Console.WriteLine(""Hello"");
    Console.WriteLine(""Goodbye"");
}
else
{
    Console.WriteLine(""Goodbye"");
    Console.WriteLine(""Hello"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleLineConditional()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var zero = Expression.Constant(0, typeof(int));
            var intVariableEqualsZero = Expression.Equal(intVariable, zero);
            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            Expression<Action> writeGoodbye = () => Console.WriteLine("Goodbye");
            var helloThenGoodbye = Expression.Block(writeHello.Body, writeGoodbye.Body, intVariable);
            var goodbyeThenHello = Expression.Block(writeGoodbye.Body, writeHello.Body, intVariable);
            var writeHelloAndGoodbye = Expression.Condition(intVariableEqualsZero, helloThenGoodbye, goodbyeThenHello);

            var translated = writeHelloAndGoodbye.ToReadableString();

            const string EXPECTED = @"
if (i == 0)
{
    Console.WriteLine(""Hello"");
    Console.WriteLine(""Goodbye"");
    return i;
}

Console.WriteLine(""Goodbye"");
Console.WriteLine(""Hello"");
return i;
";
            Assert.AreEqual(EXPECTED.Trim(), translated);
        }

        [TestMethod]
        public void ShouldTranslateASwitchStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            Expression<Action> writeOne = () => Console.WriteLine("One");
            Expression<Action> writeTwo = () => Console.WriteLine("Two");
            Expression<Action> writeThree = () => Console.WriteLine("Three");

            var switchStatement = Expression.Switch(
                intVariable,
                Expression.SwitchCase(writeOne.Body, Expression.Constant(1)),
                Expression.SwitchCase(writeTwo.Body, Expression.Constant(2)),
                Expression.SwitchCase(writeThree.Body, Expression.Constant(3)));

            var translated = switchStatement.ToReadableString();

            const string EXPECTED = @"
switch (i)
{
    case 1:
        Console.WriteLine(""One"");

    case 2:
        Console.WriteLine(""Two"");

    case 3:
        Console.WriteLine(""Three"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateASwitchStatementWithADefault()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            Expression<Action> writeOne = () => Console.WriteLine("One");
            Expression<Action> writeTwo = () => Console.WriteLine("Two");
            Expression<Action> writeThree = () => Console.WriteLine("Three");

            var writeOneTwoThree = Expression.Block(writeOne.Body, writeTwo.Body, writeThree.Body);

            var switchStatement = Expression.Switch(
                intVariable,
                writeOneTwoThree,
                Expression.SwitchCase(writeOne.Body, Expression.Constant(1)),
                Expression.SwitchCase(writeTwo.Body, Expression.Constant(2)),
                Expression.SwitchCase(writeThree.Body, Expression.Constant(3)));

            var translated = switchStatement.ToReadableString();

            const string EXPECTED = @"
switch (i)
{
    case 1:
        Console.WriteLine(""One"");

    case 2:
        Console.WriteLine(""Two"");

    case 3:
        Console.WriteLine(""Three"");

    default:
        Console.WriteLine(""One"");
        Console.WriteLine(""Two"");
        Console.WriteLine(""Three"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateASwitchStatementWithMultiLineCases()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            Expression<Action> writeOne = () => Console.WriteLine("One");
            Expression<Action> writeTwo = () => Console.WriteLine("Two");
            Expression<Action> writeThree = () => Console.WriteLine("Three");

            var writeOneTwo = Expression.Block(writeOne.Body, writeTwo.Body);
            var writeTwoThree = Expression.Block(writeTwo.Body, writeThree.Body);

            var switchStatement = Expression.Switch(
                intVariable,
                Expression.SwitchCase(writeOneTwo, Expression.Constant(12)),
                Expression.SwitchCase(writeTwoThree, Expression.Constant(23)));

            var translated = switchStatement.ToReadableString();

            const string EXPECTED = @"
switch (i)
{
    case 12:
        Console.WriteLine(""One"");
        Console.WriteLine(""Two"");

    case 23:
        Console.WriteLine(""Two"");
        Console.WriteLine(""Three"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
