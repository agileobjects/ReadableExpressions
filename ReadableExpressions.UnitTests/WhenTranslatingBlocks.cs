namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingBlocks
    {
        [TestMethod]
        public void ShouldTranslateANoVariableBlockWithNoReturnValue()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            Expression<Func<int>> read = () => Console.Read();
            Expression<Action> beep = () => Console.Beep();

            var consoleBlock = Expression.Block(writeLine.Body, read.Body, beep.Body);

            var translated = consoleBlock.ToReadableString();

            const string EXPECTED = @"
Console.WriteLine();
Console.Read();
Console.Beep();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateANoVariableBlockLambdaWithAReturnValue()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            Expression<Func<int>> read = () => Console.Read();
            var returnReadResult = Expression.Return(Expression.Label(typeof(int)), read.Body, typeof(int));

            var consoleBlock = Expression.Block(writeLine.Body, returnReadResult);
            var consoleLambda = Expression.Lambda<Func<int>>(consoleBlock);

            var translated = consoleLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    Console.WriteLine();
    return Console.Read();
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var assignZeroToCount = Expression.Assign(countVariable, Expression.Constant(0));
            var incrementCount = Expression.Increment(countVariable);
            var returnVoid = Expression.Default(typeof(void));

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignZeroToCount,
                incrementCount,
                returnVoid);

            var translated = countBlock.ToReadableString();

            const string EXPECTED = @"
var count = 0;
++count;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockLambdaWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(short), "count");
            var assignTenToCount = Expression.Assign(countVariable, Expression.Constant((short)10));
            var decrementCount = Expression.Decrement(countVariable);

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignTenToCount,
                decrementCount);

            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    var count = 10;
    --count;
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockLambdaWithAReturnValue()
        {
            var countVariable = Expression.Variable(typeof(ushort), "count");
            var countEqualsZero = Expression.Assign(countVariable, Expression.Constant((ushort)0));
            var incrementCount = Expression.Increment(countVariable);
            var returnCount = countVariable;

            var countBlock = Expression.Block(
                new[] { countVariable },
                countEqualsZero,
                incrementCount,
                returnCount);

            var countLambda = Expression.Lambda<Func<ushort>>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    var count = 0;
    ++count;

    return count;
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableBlockLambdaWithAReturnExpression()
        {
            var listVariable = Expression.Variable(typeof(List<int>), "list");
            Expression<Func<List<int>>> createList = () => new List<int> { 1, 2, 3 };

            var listAssignment = Expression.Assign(listVariable, createList.Body);

            var toArrayMethod = typeof(Enumerable).GetMethod("ToArray");
            var typedToArrayMethod = toArrayMethod.MakeGenericMethod(typeof(int));
            var listToArray = Expression.Call(typedToArrayMethod, listVariable);

            var listBlock = Expression.Block(new[] { listVariable }, listAssignment, listToArray);
            var listLambda = Expression.Lambda<Func<int[]>>(listBlock);

            var translated = listLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    var list = new List<int> { 1, 2, 3 };

    return list.ToArray();
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleAccessVariableBlockWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(ulong), "count");
            var assignTenToCount = Expression.Assign(countVariable, Expression.Constant((ulong)10));
            var addTwoToCount = Expression.AddAssign(countVariable, Expression.Constant((ulong)2));

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignTenToCount,
                addTwoToCount);

            var translated = countBlock.ToReadableString();

            const string EXPECTED = @"
var count = 10;
count += 2;";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleVariableBlockWithNoReturnValue()
        {
            var countOneVariable = Expression.Variable(typeof(int), "countOne");
            var countTwoVariable = Expression.Variable(typeof(int), "countTwo");
            var countThreeVariable = Expression.Variable(typeof(byte), "countThree");
            var assignOneToCountOne = Expression.Assign(countOneVariable, Expression.Constant(1));
            var assignTwoToCountTwo = Expression.Assign(countTwoVariable, Expression.Constant(2));
            var sumCounts = Expression.Add(countOneVariable, countTwoVariable);
            var castSumToBye = Expression.Convert(sumCounts, typeof(byte));
            var assignSumToCountThree = Expression.Assign(countThreeVariable, castSumToBye);

            var countBlock = Expression.Block(
                new[] { countOneVariable, countTwoVariable, countThreeVariable },
                assignOneToCountOne,
                assignTwoToCountTwo,
                assignSumToCountThree);

            var translated = countBlock.ToReadableString();

            const string EXPECTED = @"
var countOne = 1;
var countTwo = 2;
var countThree = (byte)(countOne + countTwo);";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAVariableAssignmentWithinACondition()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var assignFiveToCount = Expression.Assign(countVariable, Expression.Constant(5));
            var isResultLessThanTen = Expression.LessThan(assignFiveToCount, Expression.Constant(10));
            var ifResultIsLessThanTenDoNothing = Expression.IfThen(isResultLessThanTen, Expression.Default(typeof(void)));

            var countBlock = Expression.Block(new[] { countVariable }, ifResultIsLessThanTenDoNothing);
            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = @"() =>
{
    int count;

    if ((count = 5) < 10)
    {
    }
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateNestedBlocks()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            Expression<Action> beep = () => Console.Beep();

            var innerBlock = Expression.Block(writeLine.Body, beep.Body);
            var outerBlock = Expression.Block(innerBlock, writeLine.Body);

            var translated = outerBlock.ToReadableString();

            const string EXPECTED = @"
Console.WriteLine();
Console.Beep();
Console.WriteLine();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldIgnoreAVariableOnlyBlockStatement()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var @false = Expression.Constant(false, typeof(bool));

            var countBlock = Expression.Block(countVariable, @false);

            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = countLambda.ToReadableString();

            const string EXPECTED = "() => false";

            Assert.AreEqual(EXPECTED, translated);
        }

        [TestMethod]
        public void ShouldNotTerminateMethodCallArguments()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectCastToInt = Expression.Convert(objectVariable, typeof(int));
            var intToStringMethod = typeof(int).GetMethods().First(m => m.Name == "ToString");
            var intToStringCall = Expression.Call(objectCastToInt, intToStringMethod);
            var intToStringBlock = Expression.Block(intToStringCall);
            Expression<Func<string, StreamReader>> openTextFile = str => File.OpenText(str);
            var openTextFileMethod = ((MethodCallExpression)openTextFile.Body).Method;
            var openTextFileCall = Expression.Call(openTextFileMethod, intToStringBlock);

            var translated = openTextFileCall.ToReadableString();

            Assert.AreEqual("File.OpenText(((int)o).ToString())", translated);
        }

        [TestMethod]
        public void ShouldTranslateASwitchWithMultipleVariableAssignments()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var intVariable = Expression.Variable(typeof(int), "i");

            var switchStatement = Expression.Switch(
                intVariable,
                Expression.Assign(countVariable, Expression.Constant(0)),
                Enumerable
                    .Range(1, 2)
                    .Select(i => Expression.SwitchCase(
                        Expression.Assign(countVariable, Expression.Constant(i * 2)),
                        Expression.Constant(i)))
                    .ToArray());

            var switchBlock = Expression.Block(new[] { countVariable }, switchStatement);

            var translated = switchBlock.ToReadableString();

            const string EXPECTED = @"
int count;

switch (i)
{
    case 1:
        count = 2;
        break;

    case 2:
        count = 4;
        break;

    default:
        count = 0;
        break;
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldIgnoreABlankLabelTargetLine()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intAssignment = Expression.Assign(intVariable, Expression.Constant(0));

            var labelTarget = Expression.Label(typeof(void), "LabelTarget");

            var intAssignmentBlock = Expression.Block(
                new[] { intVariable },
                intAssignment,
                Expression.Label(labelTarget));

            var translated = intAssignmentBlock.ToReadableString();

            const string EXPECTED = @"var i = 0;";


            Assert.AreEqual(EXPECTED, translated);
        }

        [TestMethod]
        public void ShouldIncludeAReturnStatementForACoalesce()
        {
            var stringVariable1 = Expression.Variable(typeof(string), "myString");
            var stringVariable2 = Expression.Variable(typeof(string), "yourString");
            var assignStrings = Expression.Assign(stringVariable1, stringVariable2);

            var stringEmpty = Expression.Field(null, typeof(string), "Empty");
            var variableOrNull = Expression.Coalesce(stringVariable1, stringEmpty);

            var coalesceBlock = Expression.Block(assignStrings, variableOrNull);

            var translated = coalesceBlock.ToReadableString();

            const string EXPECTED = @"
var myString = yourString;

return (myString ?? string.Empty);";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
