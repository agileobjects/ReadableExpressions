namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingGotos
    {
        [TestMethod]
        public void ShouldTranslateGotoStatements()
        {
            var labelTargetOne = Expression.Label(typeof(void), "One");
            var labelOne = Expression.Label(labelTargetOne);
            Expression<Action> writeOne = () => Console.Write("One");
            var gotoOne = Expression.Goto(labelTargetOne);

            var labelTargetTwo = Expression.Label(typeof(void), "Two");
            var labelTwo = Expression.Label(labelTargetTwo);
            Expression<Action> writeTwo = () => Console.Write("Two");
            var gotoTwo = Expression.Goto(labelTargetTwo);

            var intVariable = Expression.Variable(typeof(int), "i");
            var intEqualsOne = Expression.Equal(intVariable, Expression.Constant(1));
            var intEqualsTwo = Expression.Equal(intVariable, Expression.Constant(2));

            var ifTwoGotoTwo = Expression.IfThen(intEqualsTwo, gotoTwo);
            var gotoOneOrTwo = Expression.IfThenElse(intEqualsOne, gotoOne, ifTwoGotoTwo);

            Expression<Action> writeNeither = () => Console.Write("Neither");
            var returnFromBlock = Expression.Return(Expression.Label());

            var block = Expression.Block(
                gotoOneOrTwo,
                writeNeither.Body,
                returnFromBlock,
                labelOne,
                writeOne.Body,
                labelTwo,
                writeTwo.Body);

            var translated = block.ToReadableString();

            const string EXPECTED = @"
if (i == 1)
{
    goto One;
}
else if (i == 2)
{
    goto Two;
}

Console.Write(""Neither"");
return;

One:
Console.Write(""One"");

Two:
Console.Write(""Two"");
";
            Assert.AreEqual(EXPECTED.Trim(), translated);
        }
    }
}
