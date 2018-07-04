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
    public class WhenTranslatingLoops : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnInfiniteLoop()
        {
            var writeLine = CreateLambda(() => Console.WriteLine());
            var loop = Expression.Loop(writeLine.Body);

            var translated = ToReadableString(loop);

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
            var intVariable = Expression.Variable(typeof(int), "i");
            var intGreaterThanTwo = Expression.GreaterThan(intVariable, Expression.Constant(2));
            var breakLoop = Expression.Break(Expression.Label());
            var ifGreaterThanTwoBreak = Expression.IfThen(intGreaterThanTwo, breakLoop);
            var writeLine = CreateLambda(() => Console.WriteLine());
            var incrementVariable = Expression.Increment(intVariable);
            var loopBody = Expression.Block(ifGreaterThanTwoBreak, writeLine.Body, incrementVariable);
            var loop = Expression.Loop(loopBody, breakLoop.Target);

            var translated = ToReadableString(loop);

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
            var intVariable = Expression.Variable(typeof(int), "i");
            var intLessThanThree = Expression.LessThan(intVariable, Expression.Constant(3));
            var incrementVariable = Expression.Increment(intVariable);
            var continueLoop = Expression.Continue(Expression.Label());
            var incrementAndContinue = Expression.Block(incrementVariable, continueLoop);
            var ifLessThanThreeContinue = Expression.IfThen(intLessThanThree, incrementAndContinue);
            var writeFinished = CreateLambda(() => Console.Write("Finished!"));
            var returnFromLoop = Expression.Return(Expression.Label());
            var loopBody = Expression.Block(ifLessThanThreeContinue, writeFinished.Body, returnFromLoop);
            var loop = Expression.Loop(loopBody, returnFromLoop.Target, continueLoop.Target);

            var translated = ToReadableString(loop);

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
}
