namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using NetStandardPolyfills;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
    using static Issue22;
    using static Issue22.InheritanceTests;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingConditionals : TestClassBase
    {
        [Fact]
        public void ShouldTranslateASingleLineIfStatement()
        {
            var intVariable = Variable(typeof(int), "i");
            var one = Constant(1);
            var intVariableLessThanOne = LessThan(intVariable, one);
            var writeLessThan = CreateLambda(() => Console.Write("Less than"));
            var ifLessThanOneThenWrite = IfThen(intVariableLessThanOne, writeLessThan.Body);

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
            var oneCastToDouble = Convert(Constant(1), typeof(double?));

            var ifTrueOne = IfThen(Constant(true), oneCastToDouble);

            var nullDouble = Constant(null, typeof(double?));

            var block = Block(ifTrueOne, nullDouble);

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
            var guidVariable = Variable(typeof(Guid), "guid");
            var defaultGuid = Default(typeof(Guid));
            var guidNotDefault = NotEqual(guidVariable, defaultGuid);
            var guidEmpty = Field(null, typeof(Guid), "Empty");
            var guidNotEmpty = NotEqual(guidVariable, guidEmpty);
            var falseConstant = Constant(false);
            var guidNotEmptyOrFalse = Condition(guidNotDefault, guidNotEmpty, falseConstant);
            var writeGuidFun = CreateLambda(() => Console.Write("GUID FUN!"));
            var ifNotEmptyThenWrite = IfThen(guidNotEmptyOrFalse, writeGuidFun.Body);

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
            var intVariable = Variable(typeof(int), "i");
            var one = Constant(1);
            var intVariableLessThanOne = LessThan(intVariable, one);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeThere = CreateLambda(() => Console.WriteLine("There"));
            var writeBlock = Block(writeHello.Body, writeThere.Body);
            var ifLessThanOneThenWrite = IfThen(intVariableLessThanOne, writeBlock);

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
            var intVariable = Variable(typeof(int), "i");
            var one = Constant(1);
            var intVariableLessThanOne = LessThan(intVariable, one);
            var intVariableMoreThanOne = GreaterThan(intVariable, one);
            var intVariableInRange = AndAlso(intVariableLessThanOne, intVariableMoreThanOne);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var ifLessThanOneThenWrite = IfThen(intVariableInRange, writeHello.Body);

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
            var intVariable = Variable(typeof(int), "i");
            var one = Constant(1);
            var intVariableLessThanOne = LessThan(intVariable, one);
            var returnLabel = Label(typeof(bool), "Return");
            var returnTrue = Return(returnLabel, Constant(true));
            var ifLessThanOneReturnTrue = IfThen(intVariableLessThanOne, returnTrue);
            var five = Constant(5);
            var intVariableMoreThanFive = GreaterThan(intVariable, five);
            var returnMoreThanFive = Label(returnLabel, intVariableMoreThanFive);
            var testBlock = Block(ifLessThanOneReturnTrue, returnMoreThanFive);

            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeVariable = Variable(writeHello.Type, "write");
            var assignWrite = Assign(writeVariable, writeHello);
            var ifTestPassesThenWrite = IfThen(testBlock, assignWrite);

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
            var intVariable = Variable(typeof(int), "i");
            var one = Constant(1);
            var intVariableLessThanOne = LessThan(intVariable, one);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeGoodbye = CreateLambda(() => Console.WriteLine("Goodbye"));
            var writeHelloOrGoodbye = IfThenElse(intVariableLessThanOne, writeHello.Body, writeGoodbye.Body);

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
            var intVariable = Variable(typeof(int), "i");
            var intEqualsOne = Equal(intVariable, Constant(1));
            var intEqualsTwo = Equal(intVariable, Constant(2));
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var ifTwoWriteTwo = IfThen(intEqualsTwo, writeTwo.Body);
            var writeOneOrTwo = IfThenElse(intEqualsOne, writeOne.Body, ifTwoWriteTwo);

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
            var intVariable = Variable(typeof(int), "i");
            var zero = Constant(0);
            var intVariableEqualsZero = Equal(intVariable, zero);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeGoodbye = CreateLambda(() => Console.WriteLine("Goodbye"));
            var helloThenGoodbye = Block(writeHello.Body, writeGoodbye.Body);
            var goodbyeThenHello = Block(writeGoodbye.Body, writeHello.Body);
            var writeHelloAndGoodbye = IfThenElse(intVariableEqualsZero, helloThenGoodbye, goodbyeThenHello);

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
            var intVariable = Variable(typeof(int), "i");
            var zero = Constant(0);
            var intVariableEqualsZero = Equal(intVariable, zero);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var writeGoodbye = CreateLambda(() => Console.WriteLine("Goodbye"));
            var helloThenGoodbye = Block(writeHello.Body, writeGoodbye.Body, intVariable);
            var goodbyeThenHello = Block(writeGoodbye.Body, writeHello.Body, intVariable);
            var writeHelloAndGoodbye = IfThenElse(intVariableEqualsZero, helloThenGoodbye, goodbyeThenHello);

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
            var ternary = Condition(
                Constant(false),
                Constant(1),
                Constant(2));

            var translated = ToReadableString(ternary);

            translated.ShouldBe("false ? 1 : 2");
        }

        [Fact]
        public void ShouldNotWrapMethodCallTernaryConditionsInParentheses()
        {
            var method = typeof(MethodCallHelper).GetPublicInstanceMethod("MultipleParameterMethod");

            var methodCall = Call(
                Variable(typeof(MethodCallHelper), "helper"),
                method,
                Constant("hello"),
                Constant(123));

            var ternary = Condition(
                methodCall,
                Constant(1),
                Constant(2));

            var translated = ToReadableString(ternary);

            translated.ShouldBe("helper.MultipleParameterMethod(\"hello\", 123) ? 1 : 2");
        }

        [Fact]
        public void ShouldTranslateASwitchStatement()
        {
            var intVariable = Variable(typeof(int), "i");
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var writeThree = CreateLambda(() => Console.WriteLine("Three"));

            var switchStatement = Switch(
                intVariable,
                SwitchCase(writeOne.Body, Constant(1)),
                SwitchCase(writeTwo.Body, Constant(2)),
                SwitchCase(writeThree.Body, Constant(3)));

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
        public void ShouldTranslateASwitchStatementWithMultipleCaseTestValues()
        {
            var intVariable = Variable(typeof(int), "i");

            var writeOneOrTwo = CreateLambda(() => Console.WriteLine("One or Two"));
            var writeOneOrTwoCase = SwitchCase(writeOneOrTwo.Body, Constant(1), Constant(2));

            var writeThree = CreateLambda(() => Console.WriteLine("Three"));
            var writeThreeCase = SwitchCase(writeThree.Body, Constant(3));

            var switchStatement = Switch(intVariable, writeOneOrTwoCase, writeThreeCase);

            var translated = ToReadableString(switchStatement);

            const string EXPECTED = @"
switch (i)
{
    case 1:
    case 2:
        Console.WriteLine(""One or Two"");
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
            var intVariable = Variable(typeof(int), "i");
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var writeThree = CreateLambda(() => Console.WriteLine("Three"));

            var writeOneTwoThree = Block(writeOne.Body, writeTwo.Body, writeThree.Body);

            var switchStatement = Switch(
                intVariable,
                writeOneTwoThree,
                SwitchCase(writeOne.Body, Constant(1)),
                SwitchCase(writeTwo.Body, Constant(2)),
                SwitchCase(writeThree.Body, Constant(3)));

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
            var intVariable = Variable(typeof(int), "i");
            var writeOne = CreateLambda(() => Console.WriteLine("One"));
            var writeTwo = CreateLambda(() => Console.WriteLine("Two"));
            var writeThree = CreateLambda(() => Console.WriteLine("Three"));

            var writeOneTwo = Block(writeOne.Body, writeTwo.Body);
            var writeTwoThree = Block(writeTwo.Body, writeThree.Body);

            var switchStatement = Switch(
                intVariable,
                SwitchCase(writeOneTwo, Constant(12)),
                SwitchCase(writeTwoThree, Constant(23)));

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
            var nullLong = Constant(null, typeof(long?));

            var writeOne = CreateLambda(() => Console.WriteLine("One!"));
            var oneCastToLong = Convert(Constant(1), typeof(long?));
            var elseBlock = Block(writeOne.Body, writeOne.Body, oneCastToLong);

            var nullOrOne = IfThenElse(Constant(true), nullLong, elseBlock);

            var translated = ToReadableString(nullOrOne);

            const string EXPECTED = @"
if (true)
{
    return null;
}

Console.WriteLine(""One!"");
Console.WriteLine(""One!"");

return (long?)1;";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldBreakLongMultipleConditionsOntoMultipleLines()
        {
            var intVariable1 = Variable(typeof(int), "thisVariableHasALongName");
            var intVariable2 = Variable(typeof(int), "thisOtherVariableHasALongNameToo");
            var int1IsGreaterThanInt2 = GreaterThan(intVariable1, intVariable2);
            var int1IsNotEqualToInt2 = NotEqual(intVariable1, intVariable2);

            var intIsInRange = AndAlso(int1IsGreaterThanInt2, int1IsNotEqualToInt2);
            var writeYo = CreateLambda(() => Console.WriteLine("Yo!"));
            var ifInRangeWriteYo = IfThen(intIsInRange, writeYo.Body);


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
            var intVariable = Variable(typeof(int), "i");
            var intValue = Constant(123);
            var intAssignment = Assign(intVariable, intValue);
            var intDefault = Default(typeof(int));
            var assignmentResultNotDefault = NotEqual(intAssignment, intDefault);
            var doNothing = Default(typeof(void));
            var ifNotdefaultDoNothing = IfThen(assignmentResultNotDefault, doNothing);

            var translated = ToReadableString(ifNotdefaultDoNothing);

            const string EXPECTED = @"
if ((i = 123) != default(int))
{
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/22
        [Fact]
        public void ShouldTranslateEnumComparisonTests()
        {
            var flagParameter = Parameter(typeof(bool), "flag");
            var one = Constant(Test.One);
            var two = Constant(Test.Two);
            var oneOrTwo = Condition(flagParameter, one, two);
            var oneOrTwoEqualsTwo = Equal(oneOrTwo, two);
            var testLambda = Lambda<Func<bool, bool>>(oneOrTwoEqualsTwo, flagParameter);

            var translated = ToReadableString(testLambda);

            translated.ShouldBe("flag => (flag ? Test.One : Test.Two) == Test.Two");
        }

#if !NET35
        [Fact]
        public void ShouldTranslateConditionalWithConditionalTest()
        {
            var dataContext = Parameter(typeof(IDataContext), "dctx");
            var dataReader = Parameter(typeof(IDataReader), "rd");

            var ldr = Variable(typeof(SqLiteDataReader), "ldr");

            var onEntityCreatedMethod = typeof(TableContext)
                .GetPublicStaticMethod(nameof(TableContext.OnEntityCreated));

            var mapperBody = Block(
                new[] { ldr },
                Assign(ldr, Convert(dataReader, typeof(SqLiteDataReader))),
                Condition(
                    Equal(
                        Condition(
                            Call(ldr, nameof(SqLiteDataReader.IsDbNull), null, Constant(0)),
                            Constant(TypeCodeEnum.Base),
                            Convert(
                                Call(ldr, nameof(SqLiteDataReader.GetInt32), null, Constant(0)),
                                typeof(TypeCodeEnum))),
                        Constant(TypeCodeEnum.A1)),
                    Convert(
                        Convert(
                            Call(
                                onEntityCreatedMethod,
                                dataContext,
                                MemberInit(
                                    New(typeof(InheritanceA1)),
                                    Bind(
                                        typeof(InheritanceA1).GetPublicInstanceProperty(nameof(InheritanceBase.GuidValue)),
                                        Condition(
                                            Call(ldr, nameof(SqLiteDataReader.IsDbNull), null, Constant(1)),
                                            Constant(Guid.Empty),
                                            Call(ldr, nameof(SqLiteDataReader.GetGuid), null, Constant(1))))
                                )
                            ),
                            typeof(InheritanceA1)),
                        typeof(InheritanceA)),
                    Convert(
                        Convert(
                            Call(
                                onEntityCreatedMethod,
                                dataContext,
                                MemberInit(
                                    New(typeof(InheritanceA2)),
                                    Bind(
                                        typeof(InheritanceA2).GetPublicInstanceProperty(nameof(InheritanceBase.GuidValue)),
                                        Condition(
                                            Call(ldr, nameof(SqLiteDataReader.IsDbNull), null, Constant(1)),
                                            Constant(Guid.Empty),
                                            Call(ldr, nameof(SqLiteDataReader.GetGuid), null, Constant(1))))
                                )
                            ),
                            typeof(InheritanceA2)),
                        typeof(InheritanceA))));

            var mapper = Lambda<Func<IDataContext, IDataReader, InheritanceA>>(
                mapperBody,
                dataContext,
                dataReader);

            var body = Invoke(mapper, dataContext, dataReader);

            var lambda = Lambda<Func<IDataContext, IDataReader, InheritanceA>>(body, dataContext, dataReader);

            const string EXPECTED = @"
(dctx, rd) => ((dctx, rd) =>
{
    var ldr = (Issue22.SqLiteDataReader)rd;

    return ((ldr.IsDbNull(0)
        ? Issue22.InheritanceTests.TypeCodeEnum.Base
        : (Issue22.InheritanceTests.TypeCodeEnum)ldr.GetInt32(0)) == Issue22.InheritanceTests.TypeCodeEnum.A1)
        ? (Issue22.InheritanceTests.InheritanceA)((Issue22.InheritanceTests.InheritanceA1)Issue22.TableContext.OnEntityCreated(
            dctx,
            new Issue22.InheritanceTests.InheritanceA1
            {
                GuidValue = ldr.IsDbNull(1) ? default(Guid) : ldr.GetGuid(1)
            }))
        : (Issue22.InheritanceTests.InheritanceA)((Issue22.InheritanceTests.InheritanceA2)Issue22.TableContext.OnEntityCreated(
            dctx,
            new Issue22.InheritanceTests.InheritanceA2
            {
                GuidValue = ldr.IsDbNull(1) ? default(Guid) : ldr.GetGuid(1)
            }));
}).Invoke(dctx, rd)";

            var translated = ToReadableString(lambda);

            translated.ShouldBe(EXPECTED.TrimStart());
        }
#endif
    }

    #region Helpers

    internal class MethodCallHelper
    {
        public bool MultipleParameterMethod(string stringValue, int intValue)
        {
            return true;
        }
    }

    internal enum Test { One, Two };

#if !NET35
    internal static class Issue22
    {
        public interface IDataContext
        {
        }

        public interface IDataReader
        {
        }

        public class SqLiteDataReader : IDataReader
        {
            public bool IsDbNull(int idx) => default(bool);

            public int GetInt32(int idx) => default(int);

            public Guid GetGuid(int idx) => default(Guid);
        }

        public static class InheritanceTests
        {
            public enum TypeCodeEnum
            {
                Base,
                A,
                A1,
                A2,
            }

            public abstract class InheritanceBase
            {
                public Guid GuidValue { get; set; }
            }

            public abstract class InheritanceA : InheritanceBase
            {
                public List<InheritanceB> Bs { get; set; }
            }

            public class InheritanceB : InheritanceBase
            {
            }

            public class InheritanceA2 : InheritanceA
            {
            }

            public class InheritanceA1 : InheritanceA
            {
            }
        }

        public class TableContext
        {
            public static object OnEntityCreated(IDataContext context, object entity) => entity;
        }

        public enum Test
        {
            One,
            Two
        }
    }
#endif

    #endregion
}
