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
    using static System.Linq.Expressions.Expression;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

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

            var consoleBlock = Block(writeLine.Body, read.Body, beep.Body);

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
            var returnReadResult = Return(Label(typeof(int)), read.Body, typeof(int));

            var consoleBlock = Block(writeLine.Body, returnReadResult);
            var consoleLambda = Lambda<Func<int>>(consoleBlock);

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
            var countVariable = Variable(typeof(int), "count");
            var assignZeroToCount = Assign(countVariable, Constant(0));
            var incrementCount = Increment(countVariable);
            var returnVoid = Default(typeof(void));

            var countBlock = Block(
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
            var countVariable = Variable(typeof(short), "count");
            var assignTenToCount = Assign(countVariable, Constant((short)10));
            var decrementCount = Decrement(countVariable);

            var countBlock = Block(
                new[] { countVariable },
                assignTenToCount,
                decrementCount);

            var countLambda = Lambda<Action>(countBlock);

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
            var countVariable = Variable(typeof(ushort), "count");
            var countEqualsZero = Assign(countVariable, Constant((ushort)0));
            var incrementCount = Increment(countVariable);
            var returnCount = countVariable;

            var countBlock = Block(
                new[] { countVariable },
                countEqualsZero,
                incrementCount,
                returnCount);

            var countLambda = Lambda<Func<ushort>>(countBlock);

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
            var listVariable = Variable(typeof(List<int>), "list");
            var createList = CreateLambda(() => new List<int> { 1, 2, 3 });

            var listAssignment = Assign(listVariable, createList.Body);

            var toArrayMethod = typeof(Enumerable).GetPublicStaticMethod("ToArray");
            var typedToArrayMethod = toArrayMethod.MakeGenericMethod(typeof(int));
            var listToArray = Call(typedToArrayMethod, listVariable);

            var listBlock = Block(new[] { listVariable }, listAssignment, listToArray);
            var listLambda = Lambda<Func<int[]>>(listBlock);

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
            var countVariable = Variable(typeof(ulong), "count");
            var assignTenToCount = Assign(countVariable, Constant((ulong)10));
            var addTwoToCount = AddAssign(countVariable, Constant((ulong)2));

            var countBlock = Block(
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
        public void ShouldUseALocalVariableExplicitTypeName()
        {
            var countVariable = Variable(typeof(ushort), "count");
            var assignTenToCount = Assign(countVariable, Constant((ushort)10));

            var countBlock = Block(new[] { countVariable }, assignTenToCount);

            var translated = ToReadableString(countBlock, s => s.UseExplicitTypeNames);

            const string EXPECTED = @"ushort count = 10;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultipleVariableBlockWithNoReturnValue()
        {
            var countOneVariable = Variable(typeof(int), "countOne");
            var countTwoVariable = Variable(typeof(int), "countTwo");
            var countThreeVariable = Variable(typeof(byte), "countThree");
            var assignOneToCountOne = Assign(countOneVariable, Constant(1));
            var assignTwoToCountTwo = Assign(countTwoVariable, Constant(2));
            var sumCounts = Add(countOneVariable, countTwoVariable);
            var castSumToBye = Convert(sumCounts, typeof(byte));
            var assignSumToCountThree = Assign(countThreeVariable, castSumToBye);

            var countBlock = Block(
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
            var countVariable = Variable(typeof(int), "count");
            var assignFiveToCount = Assign(countVariable, Constant(5));
            var isResultLessThanTen = LessThan(assignFiveToCount, Constant(10));
            var ifResultIsLessThanTenDoNothing = IfThen(isResultLessThanTen, Default(typeof(void)));

            var countBlock = Block(new[] { countVariable }, ifResultIsLessThanTenDoNothing);
            var countLambda = Lambda<Action>(countBlock);

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

            var innerBlock = Block(writeLine.Body, beep.Body);
            var outerBlock = Block(innerBlock, writeLine.Body);

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
            var countVariable = Variable(typeof(int), "count");
            var @false = Constant(false, typeof(bool));

            var countBlock = Block(countVariable, @false);

            var countLambda = Lambda<Action>(countBlock);

            var translated = ToReadableString(countLambda);

            const string EXPECTED = "() => false";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldNotTerminateMethodCallArguments()
        {
            var objectVariable = Variable(typeof(object), "o");
            var objectCastToInt = Convert(objectVariable, typeof(int));
            var intToStringMethod = typeof(int).GetPublicInstanceMethod("ToString", parameterCount: 0);
            var intToStringCall = Call(objectCastToInt, intToStringMethod);
            var intToStringBlock = Block(intToStringCall);
            var openTextFile = CreateLambda((string str) => File.OpenText(str));
            var openTextFileMethod = ((MethodCallExpression)openTextFile.Body).Method;
            var openTextFileCall = Call(openTextFileMethod, intToStringBlock);

            var translated = ToReadableString(openTextFileCall);

            translated.ShouldBe("File.OpenText(((int)o).ToString())");
        }

        [Fact]
        public void ShouldTerminateAMultipleLineMemberInitAssignment()
        {
            var writeWat = CreateLambda(() => Console.WriteLine("Wat"));
            var read = CreateLambda<long>(() => Console.Read());

            var newMemoryStream = New(typeof(MemoryStream));
            var positionProperty = newMemoryStream.Type.GetProperty("Position");
            var valueBlock = Block(writeWat.Body, read.Body);
            // ReSharper disable once AssignNullToNotNullAttribute
            var positionInit = Bind(positionProperty, valueBlock);
            var memoryStreamInit = MemberInit(newMemoryStream, positionInit);

            var streamVariable = Variable(typeof(Stream), "stream");

            var assignStream = Assign(streamVariable, memoryStreamInit);

            var streamIsNull = Equal(streamVariable, Default(typeof(Stream)));

            var ifNullAssign = IfThen(streamIsNull, assignStream);

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
            var countVariable = Variable(typeof(int), "count");
            var intVariable = Variable(typeof(int), "i");

            var switchStatement = Switch(
                intVariable,
                Assign(countVariable, Constant(0)),
                Enumerable
                    .Range(1, 2)
                    .Select(i => SwitchCase(
                        Assign(countVariable, Constant(i * 2)),
                        Constant(i)))
                    .ToArray());

            var switchBlock = Block(new[] { countVariable }, switchStatement);

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
            var company = Variable(typeof(Company), "c");
            var ceo = Variable(typeof(Employee), "ceo");
            var ceoAddress = Property(ceo, "Address");

            var assignCeo = Assign(ceo, Property(company, "Ceo"));

            var newAddress = MemberInit(
                New(typeof(Address).GetPublicInstanceConstructor()),
                Bind(
                    typeof(Address).GetPublicInstanceMember("Line1"),
                    Property(ceoAddress, "Line1")));

            var newEmployee = MemberInit(
                New(typeof(Employee).GetPublicInstanceConstructor()),
                Bind(
                    typeof(Employee).GetPublicInstanceMember("Address"),
                    newAddress)
                );

            var block = Block(assignCeo, newEmployee);

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
            var intVariable = Variable(typeof(int), "i");
            var intAssignment = Assign(intVariable, Constant(0));

            var labelTarget = Label(typeof(void), "LabelTarget");

            var intAssignmentBlock = Block(
                new[] { intVariable },
                intAssignment,
                Label(labelTarget));

            var translated = ToReadableString(intAssignmentBlock);

            translated.ShouldBe("var i = 0;");
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForACoalesce()
        {
            var stringVariable1 = Variable(typeof(string), "myString");
            var stringVariable2 = Variable(typeof(string), "yourString");
            var assignStrings = Assign(stringVariable1, stringVariable2);

            var stringEmpty = Field(null, typeof(string), "Empty");
            var variableOrNull = Coalesce(stringVariable1, stringEmpty);

            var coalesceBlock = Block(assignStrings, variableOrNull);

            var translated = ToReadableString(coalesceBlock);

            const string EXPECTED = @"
var myString = yourString;

return myString ?? string.Empty;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForANewObjectStatement()
        {
            var exception = Variable(typeof(Exception), "ex");
            var newList = New(typeof(List<string>).GetConstructors().First());
            var rethrow = Rethrow(newList.Type);
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(newList, globalCatchAndRethrow);

            var tryCatchBlock = Block(tryCatch);

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
            var exception = Variable(typeof(Exception), "ex");
            var newAddress = New(typeof(Address).GetConstructors().First());
            var line1Property = newAddress.Type.GetMember("Line1").First();
            var line1Value = Constant("Over here");
            var line1Init = Bind(line1Property, line1Value);
            var addressInit = MemberInit(newAddress, line1Init);
            var rethrow = Rethrow(newAddress.Type);
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(addressInit, globalCatchAndRethrow);

            var tryCatchBlock = Block(tryCatch);

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
            var exception = Variable(typeof(Exception), "ex");
            var listConstructor = typeof(List<int>).GetConstructor(new[] { typeof(int) });
            var one = Constant(1);
            // ReSharper disable once AssignNullToNotNullAttribute
            var newList = New(listConstructor, one);
            var newListInit = ListInit(newList, one);
            var rethrow = Rethrow(newListInit.Type);
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(newListInit, globalCatchAndRethrow);

            var tryCatchBlock = Block(tryCatch);

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
            var exception = Variable(typeof(Exception), "ex");
            var zero = Constant(0, typeof(int));
            var newArray = NewArrayBounds(typeof(int), zero);
            var rethrow = Rethrow(newArray.Type);
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(newArray, globalCatchAndRethrow);

            var tryCatchBlock = Block(tryCatch);

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
            var exception = Variable(typeof(Exception), "ex");
            var zero = Constant(0, typeof(int));
            var newArray = NewArrayInit(typeof(int), zero);
            var rethrow = Rethrow(newArray.Type);
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(newArray, globalCatchAndRethrow);

            var tryCatchBlock = Block(tryCatch);

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

        // See https://github.com/agileobjects/ReadableExpressions/issues/78
        [Fact]
        public void ShouldHandleAnEmptyBlock()
        {
            var emptyBlock = Block(Default(typeof(void)));

            var translated = ToReadableString(emptyBlock);
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
