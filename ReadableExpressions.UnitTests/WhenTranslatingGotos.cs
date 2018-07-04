namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingGotos : TestClassBase
    {
        [Fact]
        public void ShouldTranslateGotoStatements()
        {
            var labelTargetOne = Expression.Label(typeof(void), "One");
            var labelOne = Expression.Label(labelTargetOne);
            var writeOne = CreateLambda(() => Console.Write("One"));
            var gotoOne = Expression.Goto(labelTargetOne);

            var labelTargetTwo = Expression.Label(typeof(void), "Two");
            var labelTwo = Expression.Label(labelTargetTwo);
            var writeTwo = CreateLambda(() => Console.Write("Two"));
            var gotoTwo = Expression.Goto(labelTargetTwo);

            var intVariable = Expression.Variable(typeof(int), "i");
            var intEqualsOne = Expression.Equal(intVariable, Expression.Constant(1));
            var intEqualsTwo = Expression.Equal(intVariable, Expression.Constant(2));

            var ifTwoGotoTwo = Expression.IfThen(intEqualsTwo, gotoTwo);
            var gotoOneOrTwo = Expression.IfThenElse(intEqualsOne, gotoOne, ifTwoGotoTwo);

            var writeNeither = CreateLambda(() => Console.Write("Neither"));
            var returnFromBlock = Expression.Return(Expression.Label());

            var block = Expression.Block(
                gotoOneOrTwo,
                writeNeither.Body,
                returnFromBlock,
                labelOne,
                writeOne.Body,
                labelTwo,
                writeTwo.Body);

            var translated = ToReadableString(block);

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
            translated.ShouldBe(EXPECTED.Trim());
        }

        [Fact]
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

            var translated = ToReadableString(ifTrueGoto);

            const string EXPECTED = @"
if (true)
{
One:
    goto Two;

Two:
    goto One;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAGotoReturnStatement()
        {
            var returnTarget = Expression.Label(typeof(int), "Return");

            var numberParameter = Expression.Parameter(typeof(string), "i");
            var numberEqualsOne = Expression.Equal(numberParameter, Expression.Constant("One"));
            var returnOne = Expression.Goto(returnTarget, Expression.Constant(1));
            var ifOneReturnOne = Expression.IfThen(numberEqualsOne, returnOne);

            var returnLabel = Expression.Label(returnTarget, Expression.Constant(0));
            var gotoBlock = Expression.Block(ifOneReturnOne, returnLabel);

            var gotoLambda = Expression.Lambda<Func<string, int>>(gotoBlock, numberParameter);
            gotoLambda.Compile();

            var translated = ToReadableString(gotoLambda);

            const string EXPECTED = @"
i =>
{
    if (i == ""One"")
    {
        return 1;
    }

    return 0;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(gotoLambda);

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
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(testBlock);

            const string EXPECTED = @"
if (i < 1)
{
    return true;
}

return false;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(returnBlock);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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
            var translated = ToReadableString(returnBlock);

            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
