namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NetStandardPolyfills;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingBlocks : TestClassBase
    {
        [Fact]
        public void ShouldTranslateANoVariableBlockWithNoReturnValue()
        {
            var writeLine = CreateLambda(() => Console.WriteLine());
            var beep = CreateLambda(() => Console.Beep());
            var read = CreateLambda(() => Console.Read());

            var consoleBlock = Expression.Block(writeLine.Body, read.Body, beep.Body);

            var translated = ToReadableString(consoleBlock);

            const string EXPECTED = @"
Console.WriteLine();
Console.Read();
Console.Beep();";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateANoVariableBlockLambdaWithAReturnValue()
        {
            var writeLine = CreateLambda(() => Console.WriteLine());
            var read = CreateLambda(() => Console.Read());
            var returnReadResult = Expression.Return(Expression.Label(typeof(int)), read.Body, typeof(int));

            var consoleBlock = Expression.Block(writeLine.Body, returnReadResult);
            var consoleLambda = Expression.Lambda<Func<int>>(consoleBlock);

            var translated = ToReadableString(consoleLambda);

            const string EXPECTED = @"() =>
{
    Console.WriteLine();
    return Console.Read();
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(countBlock);

            const string EXPECTED = @"
var count = 0;
++count;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(countLambda);

            const string EXPECTED = @"() =>
{
    var count = 10;
    --count;
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(countLambda);

            const string EXPECTED = @"() =>
{
    var count = 0;
    ++count;

    return count;
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAVariableBlockLambdaWithAReturnExpression()
        {
            var listVariable = Expression.Variable(typeof(List<int>), "list");
            var createList = CreateLambda(() => new List<int> { 1, 2, 3 });

            var listAssignment = Expression.Assign(listVariable, createList.Body);

            var toArrayMethod = typeof(Enumerable).GetPublicStaticMethod("ToArray");
            var typedToArrayMethod = toArrayMethod.MakeGenericMethod(typeof(int));
            var listToArray = Expression.Call(typedToArrayMethod, listVariable);

            var listBlock = Expression.Block(new[] { listVariable }, listAssignment, listToArray);
            var listLambda = Expression.Lambda<Func<int[]>>(listBlock);

            var translated = ToReadableString(listLambda);

            const string EXPECTED = @"() =>
{
    var list = new List<int> { 1, 2, 3 };

    return list.ToArray();
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultipleAccessVariableBlockWithNoReturnValue()
        {
            var countVariable = Expression.Variable(typeof(ulong), "count");
            var assignTenToCount = Expression.Assign(countVariable, Expression.Constant((ulong)10));
            var addTwoToCount = Expression.AddAssign(countVariable, Expression.Constant((ulong)2));

            var countBlock = Expression.Block(
                new[] { countVariable },
                assignTenToCount,
                addTwoToCount);

            var translated = ToReadableString(countBlock);

            const string EXPECTED = @"
var count = 10;
count += 2;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(countBlock);

            const string EXPECTED = @"
var countOne = 1;
var countTwo = 2;
var countThree = (byte)(countOne + countTwo);";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAVariableAssignmentWithinACondition()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var assignFiveToCount = Expression.Assign(countVariable, Expression.Constant(5));
            var isResultLessThanTen = Expression.LessThan(assignFiveToCount, Expression.Constant(10));
            var ifResultIsLessThanTenDoNothing = Expression.IfThen(isResultLessThanTen, Expression.Default(typeof(void)));

            var countBlock = Expression.Block(new[] { countVariable }, ifResultIsLessThanTenDoNothing);
            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = ToReadableString(countLambda);

            const string EXPECTED = @"() =>
{
    int count;

    if ((count = 5) < 10)
    {
    }
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateNestedBlocks()
        {
            var writeLine = CreateLambda(() => Console.WriteLine());
            var beep = CreateLambda(() => Console.Beep());

            var innerBlock = Expression.Block(writeLine.Body, beep.Body);
            var outerBlock = Expression.Block(innerBlock, writeLine.Body);

            var translated = ToReadableString(outerBlock);

            const string EXPECTED = @"
Console.WriteLine();
Console.Beep();
Console.WriteLine();";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIgnoreAVariableOnlyBlockStatement()
        {
            var countVariable = Expression.Variable(typeof(int), "count");
            var @false = Expression.Constant(false, typeof(bool));

            var countBlock = Expression.Block(countVariable, @false);

            var countLambda = Expression.Lambda<Action>(countBlock);

            var translated = ToReadableString(countLambda);

            const string EXPECTED = "() => false";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldNotTerminateMethodCallArguments()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectCastToInt = Expression.Convert(objectVariable, typeof(int));
            var intToStringMethod = typeof(int).GetPublicInstanceMethod("ToString", parameterCount: 0);
            var intToStringCall = Expression.Call(objectCastToInt, intToStringMethod);
            var intToStringBlock = Expression.Block(intToStringCall);
            var openTextFile = CreateLambda((string str) => File.OpenText(str));
            var openTextFileMethod = ((MethodCallExpression)openTextFile.Body).Method;
            var openTextFileCall = Expression.Call(openTextFileMethod, intToStringBlock);

            var translated = ToReadableString(openTextFileCall);

            translated.ShouldBe("File.OpenText(((int)o).ToString())");
        }

        [Fact]
        public void ShouldTerminateAMultipleLineMemberInitAssignment()
        {
            var writeWat = CreateLambda(() => Console.WriteLine("Wat"));
            var read = CreateLambda<long>(() => Console.Read());

            var newMemoryStream = Expression.New(typeof(MemoryStream));
            var positionProperty = newMemoryStream.Type.GetProperty("Position");
            var valueBlock = Expression.Block(writeWat.Body, read.Body);
            // ReSharper disable once AssignNullToNotNullAttribute
            var positionInit = Expression.Bind(positionProperty, valueBlock);
            var memoryStreamInit = Expression.MemberInit(newMemoryStream, positionInit);

            var streamVariable = Expression.Variable(typeof(Stream), "stream");

            var assignStream = Expression.Assign(streamVariable, memoryStreamInit);

            var streamIsNull = Expression.Equal(streamVariable, Expression.Default(typeof(Stream)));

            var ifNullAssign = Expression.IfThen(streamIsNull, assignStream);

            var translated = ToReadableString(ifNullAssign);

            const string EXPECTED = @"
if (stream == null)
{
    stream = new MemoryStream
    {
        Position = 
        {
            Console.WriteLine(""Wat"");

            return (long)Console.Read();
        }
    };
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(switchBlock);

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
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMemberInitReturnValue()
        {
            var company = Expression.Variable(typeof(Company), "c");
            var ceo = Expression.Variable(typeof(Employee), "ceo");
            var ceoAddress = Expression.Property(ceo, "Address");

            var assignCeo = Expression.Assign(ceo, Expression.Property(company, "Ceo"));

            var newAddress = Expression.MemberInit(
                Expression.New(typeof(Address).GetPublicInstanceConstructor()),
                Expression.Bind(
                    typeof(Address).GetPublicInstanceMember("Line1"),
                    Expression.Property(ceoAddress, "Line1")));

            var newEmployee = Expression.MemberInit(
                Expression.New(typeof(Employee).GetPublicInstanceConstructor()),
                Expression.Bind(
                    typeof(Employee).GetPublicInstanceMember("Address"),
                    newAddress)
                );

            var block = Expression.Block(assignCeo, newEmployee);

            var translated = ToReadableString(block);

            const string EXPECTED = @"
var ceo = c.Ceo;

return new WhenTranslatingBlocks.Employee
{
    Address = new WhenTranslatingBlocks.Address
    {
        Line1 = ceo.Address.Line1
    }
};";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIgnoreABlankLabelTargetLine()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intAssignment = Expression.Assign(intVariable, Expression.Constant(0));

            var labelTarget = Expression.Label(typeof(void), "LabelTarget");

            var intAssignmentBlock = Expression.Block(
                new[] { intVariable },
                intAssignment,
                Expression.Label(labelTarget));

            var translated = ToReadableString(intAssignmentBlock);

            translated.ShouldBe("var i = 0;");
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForACoalesce()
        {
            var stringVariable1 = Expression.Variable(typeof(string), "myString");
            var stringVariable2 = Expression.Variable(typeof(string), "yourString");
            var assignStrings = Expression.Assign(stringVariable1, stringVariable2);

            var stringEmpty = Expression.Field(null, typeof(string), "Empty");
            var variableOrNull = Expression.Coalesce(stringVariable1, stringEmpty);

            var coalesceBlock = Expression.Block(assignStrings, variableOrNull);

            var translated = ToReadableString(coalesceBlock);

            const string EXPECTED = @"
var myString = yourString;

return myString ?? string.Empty;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForANewObjectStatement()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var newList = Expression.New(typeof(List<string>).GetConstructors().First());
            var rethrow = Expression.Rethrow(newList.Type);
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(newList, globalCatchAndRethrow);

            var tryCatchBlock = Expression.Block(tryCatch);

            var translated = ToReadableString(tryCatchBlock);

            const string EXPECTED = @"
try
{
    return new List<string>();
}
catch
{
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForAnObjectInitStatement()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var newAddress = Expression.New(typeof(Address).GetConstructors().First());
            var line1Property = newAddress.Type.GetMember("Line1").First();
            var line1Value = Expression.Constant("Over here");
            var line1Init = Expression.Bind(line1Property, line1Value);
            var addressInit = Expression.MemberInit(newAddress, line1Init);
            var rethrow = Expression.Rethrow(newAddress.Type);
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(addressInit, globalCatchAndRethrow);

            var tryCatchBlock = Expression.Block(tryCatch);

            var translated = ToReadableString(tryCatchBlock);

            const string EXPECTED = @"
try
{
    return new WhenTranslatingBlocks.Address
    {
        Line1 = ""Over here""
    };
}
catch
{
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForANewListInitStatement()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var listConstructor = typeof(List<int>).GetConstructor(new[] { typeof(int) });
            var one = Expression.Constant(1);
            // ReSharper disable once AssignNullToNotNullAttribute
            var newList = Expression.New(listConstructor, one);
            var newListInit = Expression.ListInit(newList, one);
            var rethrow = Expression.Rethrow(newListInit.Type);
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(newListInit, globalCatchAndRethrow);

            var tryCatchBlock = Expression.Block(tryCatch);

            var translated = ToReadableString(tryCatchBlock);

            const string EXPECTED = @"
try
{
    return new List<int>(1) { 1 };
}
catch
{
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForANewArrayStatement()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var zero = Expression.Constant(0, typeof(int));
            var newArray = Expression.NewArrayBounds(typeof(int), zero);
            var rethrow = Expression.Rethrow(newArray.Type);
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(newArray, globalCatchAndRethrow);

            var tryCatchBlock = Expression.Block(tryCatch);

            var translated = ToReadableString(tryCatchBlock);

            const string EXPECTED = @"
try
{
    return new int[0];
}
catch
{
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForANewArrayInitStatement()
        {
            var exception = Expression.Variable(typeof(Exception), "ex");
            var zero = Expression.Constant(0, typeof(int));
            var newArray = Expression.NewArrayInit(typeof(int), zero);
            var rethrow = Expression.Rethrow(newArray.Type);
            var globalCatchAndRethrow = Expression.Catch(exception, rethrow);
            var tryCatch = Expression.TryCatch(newArray, globalCatchAndRethrow);

            var tryCatchBlock = Expression.Block(tryCatch);

            var translated = ToReadableString(tryCatchBlock);

            const string EXPECTED = @"
try
{
    return new[] { 0 };
}
catch
{
    throw;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        #region Helper Classes

        private class Company
        {
            // ReSharper disable once UnusedMember.Local
            public Employee Ceo { get; set; }
        }

        private class Employee
        {
            // ReSharper disable once UnusedMember.Local
            public Address Address { get; set; }
        }

        private class Address
        {
            // ReSharper disable once UnusedMember.Local
            public string Line1 { get; set; }
        }

        #endregion
    }
}
