namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingGotos : TestClassBase
    {
        [Fact]
        public void ShouldTranslateGotoStatements()
        {
            var labelTargetOne = Label(typeof(void), "One");
            var labelOne = Label(labelTargetOne);
            var writeOne = CreateLambda(() => Console.Write("One"));
            var gotoOne = Goto(labelTargetOne);

            var labelTargetTwo = Label(typeof(void), "Two");
            var labelTwo = Label(labelTargetTwo);
            var writeTwo = CreateLambda(() => Console.Write("Two"));
            var gotoTwo = Goto(labelTargetTwo);

            var intVariable = Variable(typeof(int), "i");
            var intEqualsOne = Equal(intVariable, Constant(1));
            var intEqualsTwo = Equal(intVariable, Constant(2));

            var ifTwoGotoTwo = IfThen(intEqualsTwo, gotoTwo);
            var gotoOneOrTwo = IfThenElse(intEqualsOne, gotoOne, ifTwoGotoTwo);

            var writeNeither = CreateLambda(() => Console.Write("Neither"));
            var returnFromBlock = Return(Label());

            var block = Block(
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
        public void ShouldFormatGotoTargetLabels()
        {
            var labelTargetOne = Label(typeof(void), "One");
            var labelOne = Label(labelTargetOne);
            var gotoOne = Goto(labelTargetOne);

            var labelTargetTwo = Label(typeof(void), "Two");
            var labelTwo = Label(labelTargetTwo);
            var gotoTwo = Goto(labelTargetTwo);

            var gotoBlock = Block(labelOne, gotoTwo, labelTwo, gotoOne);

            var ifTrueGoto = IfThen(Constant(true), gotoBlock);

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
            var returnTarget = Label(typeof(int), "Return");

            var numberParameter = Parameter(typeof(string), "i");
            var numberEqualsOne = Equal(numberParameter, Constant("One"));
            var returnOne = Goto(returnTarget, Constant(1));
            var ifOneReturnOne = IfThen(numberEqualsOne, returnOne);

            var returnLabel = Label(returnTarget, Constant(0));
            var gotoBlock = Block(ifOneReturnOne, returnLabel);

            var gotoLambda = Lambda<Func<string, int>>(gotoBlock, numberParameter);
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
            var returnTarget = Label(typeof(int));

            var returnOne = Return(returnTarget, Constant(1));
            var returnTwo = Return(returnTarget, Constant(2));

            var numberParameter = Parameter(typeof(string), "i");
            var numberEqualsOne = Equal(numberParameter, Constant("One"));

            var ifOneReturnOneElseTwo = IfThenElse(numberEqualsOne, returnOne, returnTwo);

            var returnLabel = Label(returnTarget, Constant(0));
            var gotoBlock = Block(ifOneReturnOneElseTwo, returnLabel);

            var gotoLambda = Lambda<Func<string, int>>(gotoBlock, numberParameter);
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
            var returnLabelTarget = Label(typeof(bool), "ReturnTarget");

            var intVariable = Variable(typeof(int), "i");
            var variableLessThanOne = LessThan(intVariable, Constant(1));
            var returnTrue = Return(returnLabelTarget, Constant(true));

            var ifLessThanOneReturnTrue = IfThen(variableLessThanOne, returnTrue);

            var testBlock = Block(
                ifLessThanOneReturnTrue,
                Label(returnLabelTarget, Constant(false)));

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
            var returnLabelTarget = Label(typeof(int));

            var intVariable = Variable(typeof(int), "i");
            var variableInit = Assign(intVariable, Constant(0));
            var variablePlusOne = Add(intVariable, Constant(1));
            var variableAdditionOne = Assign(intVariable, variablePlusOne);
            var variablePlusTwo = Add(intVariable, Constant(2));
            var variableAdditionTwo = Assign(intVariable, variablePlusTwo);

            var variableBlock = Block(
                new[] { intVariable },
                variableInit,
                variableAdditionOne,
                variableAdditionTwo,
                intVariable);

            var returnVariableBlock = Return(returnLabelTarget, variableBlock);

            var returnBlock = Block(returnVariableBlock);

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
            var returnLabelTarget = Label(typeof(int), "Return");

            var intVariable = Variable(typeof(int), "i");
            var variableInit = Assign(intVariable, Constant(0));
            var variablePlusOne = Add(intVariable, Constant(1));
            var variableAdditionOne = Assign(intVariable, variablePlusOne);
            var variablePlusTwo = Add(intVariable, Constant(2));
            var variableAdditionTwo = Assign(intVariable, variablePlusTwo);

            var variableBlock = Block(variableAdditionTwo, intVariable);

            var returnVariableBlock = Label(returnLabelTarget, variableBlock);

            var returnBlock = Block(
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
