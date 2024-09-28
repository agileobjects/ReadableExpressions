namespace AgileObjects.ReadableExpressions.UnitTests;

using System;

#if NET35
[NUnitTestFixture]
#endif
public class WhenTranslatingLoops : TestClassBase
{
    [Fact]
    public void ShouldTranslateAnInfiniteLoop()
    {
        var writeLine = CreateLambda(() => Console.WriteLine());
        var loop = Loop(writeLine.Body);

        var translated = loop.ToReadableString();

        const string EXPECTED = @"
while (true)
{
    Console.WriteLine();
}";
        translated.ShouldBe(EXPECTED.TrimStart());
    }

    [Fact]
    public void ShouldTranslateALoopWithABreakStatement()
    {
        var intVariable = Variable(typeof(int), "i");
        var intGreaterThanTwo = GreaterThan(intVariable, Constant(2));
        var breakLoop = Break(Label());
        var ifGreaterThanTwoBreak = IfThen(intGreaterThanTwo, breakLoop);
        var writeLine = CreateLambda(() => Console.WriteLine());
        var incrementVariable = PreIncrementAssign(intVariable);
        var loopBody = Block(ifGreaterThanTwoBreak, writeLine.Body, incrementVariable);
        var loop = Loop(loopBody, breakLoop.Target);

        var translated = loop.ToReadableString();

        const string EXPECTED = @"
while (true)
{
    if (i > 2)
    {
        break;
    }

    Console.WriteLine();
    ++i;
}";
        translated.ShouldBe(EXPECTED.TrimStart());
    }

    [Fact]
    public void ShouldTranslateALoopWithAContinueStatement()
    {
        var intVariable = Variable(typeof(int), "i");
        var intLessThanThree = LessThan(intVariable, Constant(3));
        var incrementVariable = PreIncrementAssign(intVariable);
        var continueLoop = Continue(Label());
        var incrementAndContinue = Block(incrementVariable, continueLoop);
        var ifLessThanThreeContinue = IfThen(intLessThanThree, incrementAndContinue);
        var writeFinished = CreateLambda(() => Console.Write("Finished!"));
        var returnFromLoop = Return(Label());
        var loopBody = Block(ifLessThanThreeContinue, writeFinished.Body, returnFromLoop);
        var loop = Loop(loopBody, returnFromLoop.Target, continueLoop.Target);

        var translated = loop.ToReadableString();

        const string EXPECTED = @"
while (true)
{
    if (i < 3)
    {
        ++i;
        continue;
    }

    Console.Write(""Finished!"");
    return;
}";
        translated.ShouldBe(EXPECTED.TrimStart());
    }
}