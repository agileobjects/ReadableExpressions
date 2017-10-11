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

        [TestMethod]
        public void ShouldUnindentGotoTargetLabels()
        {
            var labelTargetOne = Expression.Label(typeof(void), "One");
            var labelOne = Expression.Label(labelTargetOne);
            var gotoOne = Expression.Goto(labelTargetOne);

            var labelTargetTwo = Expression.Label(typeof(void), "Two");
            var labelTwo = Expression.Label(labelTargetTwo);
            var gotoTwo = Expression.Goto(labelTargetTwo);

            var gotoBlock = Expression.Block(labelOne, gotoTwo, labelTwo, gotoOne);

            var ifTrueGoto = Expression.IfThen(Expression.Constant(true), gotoBlock);

            var translated = ifTrueGoto.ToReadableString();

            const string EXPECTED = @"
if (true)
{
One:
    goto Two;

Two:
    goto One;
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAReturnStatementWithAValue()
        {
            var returnTarget = Expression.Label(typeof(int));

            var returnOne = Expression.Return(returnTarget, Expression.Constant(1));
            var returnTwo = Expression.Return(returnTarget, Expression.Constant(2));

            var numberParameter = Expression.Parameter(typeof(string), "i");
            var numberEqualsOne = Expression.Equal(numberParameter, Expression.Constant("One"));

            var ifOneReturnOneElseTwo = Expression.IfThenElse(numberEqualsOne, returnOne, returnTwo);

            var returnLabel = Expression.Label(returnTarget, Expression.Constant(0));
            var gotoBlock = Expression.Block(ifOneReturnOneElseTwo, returnLabel);

            var gotoLambda = Expression.Lambda<Func<string, int>>(gotoBlock, numberParameter);
            gotoLambda.Compile();

            var translated = gotoLambda.ToReadableString();

            const string EXPECTED = @"
i =>
{
    if (i == ""One"")
    {
        return 1;
    }
    else
    {
        return 2;
    }

    return 0;
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldNotIncludeLabelNamesWithoutAGoto()
        {
            var returnLabelTarget = Expression.Label(typeof(bool), "ReturnTarget");

            var intVariable = Expression.Variable(typeof(int), "i");
            var variableLessThanOne = Expression.LessThan(intVariable, Expression.Constant(1));
            var returnTrue = Expression.Return(returnLabelTarget, Expression.Constant(true));

            var ifLessThanOneReturnTrue = Expression.IfThen(variableLessThanOne, returnTrue);

            var testBlock = Expression.Block(
                ifLessThanOneReturnTrue,
                Expression.Label(returnLabelTarget, Expression.Constant(false)));

            var translated = testBlock.ToReadableString();

            const string EXPECTED = @"
if (i < 1)
{
    return true;
}

return false;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAReturnStatementWithABlock()
        {
            var returnLabelTarget = Expression.Label(typeof(int));

            var intVariable = Expression.Variable(typeof(int), "i");
            var variableInit = Expression.Assign(intVariable, Expression.Constant(0));
            var variablePlusOne = Expression.Add(intVariable, Expression.Constant(1));
            var variableAdditionOne = Expression.Assign(intVariable, variablePlusOne);
            var variablePlusTwo = Expression.Add(intVariable, Expression.Constant(2));
            var variableAdditionTwo = Expression.Assign(intVariable, variablePlusTwo);

            var variableBlock = Expression.Block(
                new[] { intVariable },
                variableInit,
                variableAdditionOne,
                variableAdditionTwo,
                intVariable);

            var returnVariableBlock = Expression.Return(returnLabelTarget, variableBlock);

            var returnBlock = Expression.Block(returnVariableBlock);

            const string EXPECTED = @"
return 
{
    var i = 0;
    i = i + 1;
    i = i + 2;

    return i;
};";

            var translated = returnBlock.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateALabelWithABlockDefaultValue()
        {
            var returnLabelTarget = Expression.Label(typeof(int), "Return");

            var intVariable = Expression.Variable(typeof(int), "i");
            var variableInit = Expression.Assign(intVariable, Expression.Constant(0));
            var variablePlusOne = Expression.Add(intVariable, Expression.Constant(1));
            var variableAdditionOne = Expression.Assign(intVariable, variablePlusOne);
            var variablePlusTwo = Expression.Add(intVariable, Expression.Constant(2));
            var variableAdditionTwo = Expression.Assign(intVariable, variablePlusTwo);

            var variableBlock = Expression.Block(variableAdditionTwo, intVariable);

            var returnVariableBlock = Expression.Label(returnLabelTarget, variableBlock);

            var returnBlock = Expression.Block(
                new[] { intVariable },
                variableInit,
                variableAdditionOne,
                returnVariableBlock);

            const string EXPECTED = @"
var i = 0;
i = i + 1;

return 
{
    i = i + 2;

    return i;
};";

            var translated = returnBlock.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
