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
            var one = Expression.Constant(1);
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
        public void ShouldTranslateAShortCircuitingIfStatement()
        {
            var oneCastToDouble = Expression.Convert(Expression.Constant(1), typeof(double?));

            var ifTrueOne = Expression.IfThen(Expression.Constant(true), oneCastToDouble);

            var nullDouble = Expression.Constant(null, typeof(double?));

            var block = Expression.Block(ifTrueOne, nullDouble);

            var translated = block.ToReadableString();

            const string EXPECTED = @"
if (true)
{
    return (double?)1;
}

return null;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIfStatementWithAConditional()
        {
            var guidVariable = Expression.Variable(typeof(Guid), "guid");
            var defaultGuid = Expression.Default(typeof(Guid));
            var guidNotDefault = Expression.NotEqual(guidVariable, defaultGuid);
            var guidEmpty = Expression.Field(null, typeof(Guid), "Empty");
            var guidNotEmpty = Expression.NotEqual(guidVariable, guidEmpty);
            var falseConstant = Expression.Constant(false);
            var guidNotEmptyOrFalse = Expression.Condition(guidNotDefault, guidNotEmpty, falseConstant);
            Expression<Action> writeGuidFun = () => Console.Write("GUID FUN!");
            var ifNotEmptyThenWrite = Expression.IfThen(guidNotEmptyOrFalse, writeGuidFun.Body);

            var translated = ifNotEmptyThenWrite.ToReadableString();

            const string EXPECTED = @"
if ((guid != default(Guid)) ? guid != Guid.Empty : false)
{
    Console.Write(""GUID FUN!"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleLineIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
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

            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            var writeVariable = Expression.Variable(writeHello.Type, "write");
            var assignWrite = Expression.Assign(writeVariable, writeHello);
            var ifTestPassesThenWrite = Expression.IfThen(testBlock, assignWrite);

            var translated = ifTestPassesThenWrite.ToReadableString();

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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIfElseStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
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
        public void ShouldTranslateAnIfElseIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intEqualsOne = Expression.Equal(intVariable, Expression.Constant(1));
            var intEqualsTwo = Expression.Equal(intVariable, Expression.Constant(2));
            Expression<Action> writeOne = () => Console.WriteLine("One");
            Expression<Action> writeTwo = () => Console.WriteLine("Two");
            var ifTwoWriteTwo = Expression.IfThen(intEqualsTwo, writeTwo.Body);
            var writeOneOrTwo = Expression.IfThenElse(intEqualsOne, writeOne.Body, ifTwoWriteTwo);

            var translated = writeOneOrTwo.ToReadableString();

            const string EXPECTED = @"
if (i == 1)
{
    Console.WriteLine(""One"");
}
else if (i == 2)
{
    Console.WriteLine(""Two"");
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateMultipleLineVoidIfElseStatements()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var zero = Expression.Constant(0);
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
        public void ShouldTranslateAMultipleLineNonVoidIfElseStatements()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var zero = Expression.Constant(0);
            var intVariableEqualsZero = Expression.Equal(intVariable, zero);
            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            Expression<Action> writeGoodbye = () => Console.WriteLine("Goodbye");
            var helloThenGoodbye = Expression.Block(writeHello.Body, writeGoodbye.Body, intVariable);
            var goodbyeThenHello = Expression.Block(writeGoodbye.Body, writeHello.Body, intVariable);
            var writeHelloAndGoodbye = Expression.IfThenElse(intVariableEqualsZero, helloThenGoodbye, goodbyeThenHello);

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
        public void ShouldNotWrapSingleExpressionTernaryConditionsInParentheses()
        {
            var ternary = Expression.Condition(
                Expression.Constant(false),
                Expression.Constant(1),
                Expression.Constant(2));

            var translated = ternary.ToReadableString();

            Assert.AreEqual("false ? 1 : 2", translated);
        }

        [TestMethod]
        public void ShouldNotWrapMethodCallTernaryConditionsInParentheses()
        {
            var method = typeof(MethodCallHelper).GetMethod("MultipleParameterMethod");

            var methodCall = Expression.Call(
                Expression.Variable(typeof(MethodCallHelper), "helper"),
                method,
                Expression.Constant("hello"),
                Expression.Constant(123));

            var ternary = Expression.Condition(
                methodCall,
                Expression.Constant(1),
                Expression.Constant(2));

            var translated = ternary.ToReadableString();

            Assert.AreEqual("helper.MultipleParameterMethod(\"hello\", 123) ? 1 : 2", translated);
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
        break;

    case 2:
        Console.WriteLine(""Two"");
        break;

    case 3:
        Console.WriteLine(""Three"");
        break;
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
        break;

    case 23:
        Console.WriteLine(""Two"");
        Console.WriteLine(""Three"");
        break;
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldIncludeReturnKeywordsForConstantsAndCasts()
        {
            var nullLong = Expression.Constant(null, typeof(long?));

            Expression<Action> writeOne = () => Console.WriteLine("One!");
            var oneCastToLong = Expression.Convert(Expression.Constant(1), typeof(long?));
            var elseBlock = Expression.Block(writeOne.Body, writeOne.Body, oneCastToLong);

            var nullOrOne = Expression.IfThenElse(Expression.Constant(true), nullLong, elseBlock);

            var translated = nullOrOne.ToReadableString();

            const string EXPECTED = @"
if (true)
{
    return null;
}

Console.WriteLine(""One!"");
Console.WriteLine(""One!"");

return ((long?)1);";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldBreakLongMultipleConditionsOntoMultipleLines()
        {
            var intVariable1 = Expression.Variable(typeof(int), "thisVariableHasALongName");
            var intVariable2 = Expression.Variable(typeof(int), "thisOtherVariableHasALongNameToo");
            var int1IsGreaterThanInt2 = Expression.GreaterThan(intVariable1, intVariable2);
            var int1IsNotEqualToInt2 = Expression.NotEqual(intVariable1, intVariable2);

            var intIsInRange = Expression.AndAlso(int1IsGreaterThanInt2, int1IsNotEqualToInt2);
            Expression<Action> writeYo = () => Console.WriteLine("Yo!");
            var ifInRangeWriteYo = Expression.IfThen(intIsInRange, writeYo.Body);


            var translated = ifInRangeWriteYo.ToReadableString();

            const string EXPECTED = @"
if ((thisVariableHasALongName > thisOtherVariableHasALongNameToo) &&
    (thisVariableHasALongName != thisOtherVariableHasALongNameToo))
{
    Console.WriteLine(""Yo!"");
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
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
