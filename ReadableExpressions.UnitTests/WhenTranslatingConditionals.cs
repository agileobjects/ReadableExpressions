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
    }
}
