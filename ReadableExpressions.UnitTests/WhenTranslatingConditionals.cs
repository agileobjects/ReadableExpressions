namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using NetStandardPolyfills;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingConditionals : TestClassBase
    {
        [Fact]
        public void ShouldTranslateASingleLineIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            var writeLessThan = CreateLambda(() => Console.Write("Less than"));
            var ifLessThanOneThenWrite = Expression.IfThen(intVariableLessThanOne, writeLessThan.Body);

            var translated = ToReadableString(ifLessThanOneThenWrite);

            const string EXPECTED = @"
if (i < 1)
{
    Console.Write(""Less than"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAShortCircuitingIfStatement()
        {
            var oneCastToDouble = Expression.Convert(Expression.Constant(1), typeof(double?));

            var ifTrueOne = Expression.IfThen(Expression.Constant(true), oneCastToDouble);

            var nullDouble = Expression.Constant(null, typeof(double?));

            var block = Expression.Block(ifTrueOne, nullDouble);

            var translated = ToReadableString(block);

            const string EXPECTED = @"
if (true)
{
    return (double?)1;
}

return null;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnIfStatementWithAConditional()
        {
            var guidVariable = Expression.Variable(typeof(Guid), "guid");
            var defaultGuid = Expression.Default(typeof(Guid));
            var guidNotDefault = Expression.NotEqual(guidVariable, defaultGuid);
            var guidEmpty = Expression.Field(null, typeof(Guid), "Empty");
            var guidNotEmpty = Expression.NotEqual(guidVariable, guidEmpty);
            var falseConstant = Expression.Constant(false);
            var guidNotEmptyOrFalse = Expression.Condition(guidNotDefault, guidNotEmpty, falseConstant);
            var writeGuidFun = CreateLambda(() => Console.Write("GUID FUN!"));
            var ifNotEmptyThenWrite = Expression.IfThen(guidNotEmptyOrFalse, writeGuidFun.Body);

            var translated = ToReadableString(ifNotEmptyThenWrite);

            const string EXPECTED = @"
if ((guid != default(Guid)) ? guid != Guid.Empty : false)
{
    Console.Write(""GUID FUN!"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultipleLineIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeThere = CreateLambda(() => Console.WriteLine("There"));
            var writeBlock = Expression.Block(writeHello.Body, writeThere.Body);
            var ifLessThanOneThenWrite = Expression.IfThen(intVariableLessThanOne, writeBlock);

            var translated = ToReadableString(ifLessThanOneThenWrite);

            const string EXPECTED = @"
if (i < 1)
{
    Console.WriteLine(""Hello"");
    Console.WriteLine(""There"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultipleClauseIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            var intVariableMoreThanOne = Expression.GreaterThan(intVariable, one);
            var intVariableInRange = Expression.AndAlso(intVariableLessThanOne, intVariableMoreThanOne);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var ifLessThanOneThenWrite = Expression.IfThen(intVariableInRange, writeHello.Body);

            var translated = ToReadableString(ifLessThanOneThenWrite);

            const string EXPECTED = @"
if ((i < 1) && (i > 1))
{
    Console.WriteLine(""Hello"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultipleLineIfStatementTest()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            var returnLabel = Expression.Label(typeof(bool), "Return");
            var returnTrue = Expression.Return(returnLabel, Expression.Constant(true));
            var ifLessThanOneReturnTrue = Expression.IfThen(intVariableLessThanOne, returnTrue);
            var five = Expression.Constant(5);
            var intVariableMoreThanFive = Expression.GreaterThan(intVariable, five);
            var returnMoreThanFive = Expression.Label(returnLabel, intVariableMoreThanFive);
            var testBlock = Expression.Block(ifLessThanOneReturnTrue, returnMoreThanFive);

            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeVariable = Expression.Variable(writeHello.Type, "write");
            var assignWrite = Expression.Assign(writeVariable, writeHello);
            var ifTestPassesThenWrite = Expression.IfThen(testBlock, assignWrite);

            var translated = ToReadableString(ifTestPassesThenWrite);

            const string EXPECTED = @"
if ({
        if (i < 1)
        {
            return true;
        }

        return i > 5;
    })
{
    write = () => Console.WriteLine(""Hello"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnIfElseStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
            var intVariableLessThanOne = Expression.LessThan(intVariable, one);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeGoodbye = CreateLambda(() => Console.WriteLine("Goodbye"));
            var writeHelloOrGoodbye = Expression.IfThenElse(intVariableLessThanOne, writeHello.Body, writeGoodbye.Body);

            var translated = ToReadableString(writeHelloOrGoodbye);

            const string EXPECTED = @"
if (i < 1)
{
    Console.WriteLine(""Hello"");
}
else
{
    Console.WriteLine(""Goodbye"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnIfElseIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intEqualsOne = Expression.Equal(intVariable, Expression.Constant(1));
            var intEqualsTwo = Expression.Equal(intVariable, Expression.Constant(2));
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var ifTwoWriteTwo = Expression.IfThen(intEqualsTwo, writeTwo.Body);
            var writeOneOrTwo = Expression.IfThenElse(intEqualsOne, writeOne.Body, ifTwoWriteTwo);

            var translated = ToReadableString(writeOneOrTwo);

            const string EXPECTED = @"
if (i == 1)
{
    Console.WriteLine(""One"");
}
else if (i == 2)
{
    Console.WriteLine(""Two"");
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateMultipleLineVoidIfElseStatements()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var zero = Expression.Constant(0);
            var intVariableEqualsZero = Expression.Equal(intVariable, zero);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeGoodbye = CreateLambda(() => Console.WriteLine("Goodbye"));
            var helloThenGoodbye = Expression.Block(writeHello.Body, writeGoodbye.Body);
            var goodbyeThenHello = Expression.Block(writeGoodbye.Body, writeHello.Body);
            var writeHelloAndGoodbye = Expression.IfThenElse(intVariableEqualsZero, helloThenGoodbye, goodbyeThenHello);

            var translated = ToReadableString(writeHelloAndGoodbye);

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
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultipleLineNonVoidIfElseStatements()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var zero = Expression.Constant(0);
            var intVariableEqualsZero = Expression.Equal(intVariable, zero);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeGoodbye = CreateLambda(() => Console.WriteLine("Goodbye"));
            var helloThenGoodbye = Expression.Block(writeHello.Body, writeGoodbye.Body, intVariable);
            var goodbyeThenHello = Expression.Block(writeGoodbye.Body, writeHello.Body, intVariable);
            var writeHelloAndGoodbye = Expression.IfThenElse(intVariableEqualsZero, helloThenGoodbye, goodbyeThenHello);

            var translated = ToReadableString(writeHelloAndGoodbye);

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
            translated.ShouldBe(EXPECTED.Trim());
        }

        [Fact]
        public void ShouldNotWrapSingleExpressionTernaryConditionsInParentheses()
        {
            var ternary = Expression.Condition(
                Expression.Constant(false),
                Expression.Constant(1),
                Expression.Constant(2));

            var translated = ToReadableString(ternary);

            translated.ShouldBe("false ? 1 : 2");
        }

        [Fact]
        public void ShouldNotWrapMethodCallTernaryConditionsInParentheses()
        {
            var method = typeof(MethodCallHelper).GetPublicInstanceMethod("MultipleParameterMethod");

            var methodCall = Expression.Call(
                Expression.Variable(typeof(MethodCallHelper), "helper"),
                method,
                Expression.Constant("hello"),
                Expression.Constant(123));

            var ternary = Expression.Condition(
                methodCall,
                Expression.Constant(1),
                Expression.Constant(2));

            var translated = ToReadableString(ternary);

            translated.ShouldBe("helper.MultipleParameterMethod(\"hello\", 123) ? 1 : 2");
        }

        [Fact]
        public void ShouldTranslateASwitchStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var writeThree = CreateLambda(() => Console.WriteLine("Three"));

            var switchStatement = Expression.Switch(
                intVariable,
                Expression.SwitchCase(writeOne.Body, Expression.Constant(1)),
                Expression.SwitchCase(writeTwo.Body, Expression.Constant(2)),
                Expression.SwitchCase(writeThree.Body, Expression.Constant(3)));

            var translated = ToReadableString(switchStatement);

            const string EXPECTED = @"
switch (i)
{
    case 1:
        Console.WriteLine(""One"");
        break;

    case 2:
        Console.WriteLine(""Two"");
        break;

    case 3:
        Console.WriteLine(""Three"");
        break;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateASwitchStatementWithADefault()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var writeThree = CreateLambda(() => Console.WriteLine("Three"));

            var writeOneTwoThree = Expression.Block(writeOne.Body, writeTwo.Body, writeThree.Body);

            var switchStatement = Expression.Switch(
                intVariable,
                writeOneTwoThree,
                Expression.SwitchCase(writeOne.Body, Expression.Constant(1)),
                Expression.SwitchCase(writeTwo.Body, Expression.Constant(2)),
                Expression.SwitchCase(writeThree.Body, Expression.Constant(3)));

            var translated = ToReadableString(switchStatement);

            const string EXPECTED = @"
switch (i)
{
    case 1:
        Console.WriteLine(""One"");
        break;

    case 2:
        Console.WriteLine(""Two"");
        break;

    case 3:
        Console.WriteLine(""Three"");
        break;

    default:
        Console.WriteLine(""One"");
        Console.WriteLine(""Two"");
        Console.WriteLine(""Three"");
        break;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateASwitchStatementWithMultiLineCases()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var writeThree = CreateLambda(() => Console.WriteLine("Three"));

            var writeOneTwo = Expression.Block(writeOne.Body, writeTwo.Body);
            var writeTwoThree = Expression.Block(writeTwo.Body, writeThree.Body);

            var switchStatement = Expression.Switch(
                intVariable,
                Expression.SwitchCase(writeOneTwo, Expression.Constant(12)),
                Expression.SwitchCase(writeTwoThree, Expression.Constant(23)));

            var translated = ToReadableString(switchStatement);

            const string EXPECTED = @"
switch (i)
{
    case 12:
        Console.WriteLine(""One"");
        Console.WriteLine(""Two"");
        break;

    case 23:
        Console.WriteLine(""Two"");
        Console.WriteLine(""Three"");
        break;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeReturnKeywordsForConstantsAndCasts()
        {
            var nullLong = Expression.Constant(null, typeof(long?));

            var writeOne = CreateLambda(() => Console.WriteLine("One!"));
            var oneCastToLong = Expression.Convert(Expression.Constant(1), typeof(long?));
            var elseBlock = Expression.Block(writeOne.Body, writeOne.Body, oneCastToLong);

            var nullOrOne = Expression.IfThenElse(Expression.Constant(true), nullLong, elseBlock);

            var translated = ToReadableString(nullOrOne);

            const string EXPECTED = @"
if (true)
{
    return null;
}

Console.WriteLine(""One!"");
Console.WriteLine(""One!"");

return ((long?)1);";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldBreakLongMultipleConditionsOntoMultipleLines()
        {
            var intVariable1 = Expression.Variable(typeof(int), "thisVariableHasALongName");
            var intVariable2 = Expression.Variable(typeof(int), "thisOtherVariableHasALongNameToo");
            var int1IsGreaterThanInt2 = Expression.GreaterThan(intVariable1, intVariable2);
            var int1IsNotEqualToInt2 = Expression.NotEqual(intVariable1, intVariable2);

            var intIsInRange = Expression.AndAlso(int1IsGreaterThanInt2, int1IsNotEqualToInt2);
            var writeYo = CreateLambda(() => Console.WriteLine("Yo!"));
            var ifInRangeWriteYo = Expression.IfThen(intIsInRange, writeYo.Body);


            var translated = ToReadableString(ifInRangeWriteYo);

            const string EXPECTED = @"
if ((thisVariableHasALongName > thisOtherVariableHasALongNameToo) &&
    (thisVariableHasALongName != thisOtherVariableHasALongNameToo))
{
    Console.WriteLine(""Yo!"");
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAssignmentOutcomeTests()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intValue = Expression.Constant(123);
            var intAssignment = Expression.Assign(intVariable, intValue);
            var intDefault = Expression.Default(typeof(int));
            var assignmentResultNotDefault = Expression.NotEqual(intAssignment, intDefault);
            var doNothing = Expression.Default(typeof(void));
            var ifNotdefaultDoNothing = Expression.IfThen(assignmentResultNotDefault, doNothing);

            var translated = ToReadableString(ifNotdefaultDoNothing);

            const string EXPECTED = @"
if ((i = 123) != default(int))
{
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }

    #region Helpers

    internal class MethodCallHelper
    {
        public bool MultipleParameterMethod(string stringValue, int intValue)
        {
            return true;
        }
    }

    #endregion
}
