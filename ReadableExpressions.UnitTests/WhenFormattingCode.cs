namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common;
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
    public class WhenFormattingCode : TestClassBase
    {
        [Fact]
        public void ShouldSplitLongConstructorArgumentListsOntoMultipleLines()
        {
            var helperVariable = Variable(typeof(HelperClass), "helper");
            var helperConstructor = helperVariable.Type.GetPublicInstanceConstructors().First();
            var longVariable = Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var newHelper = New(helperConstructor, longVariable, longVariable, longVariable);
            var helperAssignment = Assign(helperVariable, newHelper);

            var longArgumentListBlock = Block(new[] { helperVariable }, helperAssignment);

            var translated = longArgumentListBlock.ToReadableString();

            const string EXPECTED = @"
var helper = new HelperClass(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed);";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSplitMultipleArgumentListsOntoMultipleLines()
        {
            var intsMethod = typeof(HelperClass)
                .GetPublicInstanceMethod("GiveMeFourInts");

            var helperVariable = Variable(typeof(HelperClass), "helper");
            var intVariable = Variable(typeof(int), "intVariable");

            var intsMethodCall = Call(
                helperVariable,
                intsMethod,
                intVariable,
                intVariable,
                intVariable,
                intVariable);

            var translated = intsMethodCall.ToReadableString();

            const string EXPECTED = @"
helper.GiveMeFourInts(
    intVariable,
    intVariable,
    intVariable,
    intVariable)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSplitLongArgumentListsOntoMultipleLines()
        {
            var intsMethod = typeof(HelperClass)
                .GetPublicInstanceMethod("GiveMeSomeInts");

            var helperVariable = Variable(typeof(HelperClass), "helper");
            var longVariable = Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var intsMethodCall = Call(helperVariable, intsMethod, longVariable, longVariable, longVariable);

            var translated = intsMethodCall.ToReadableString();

            const string EXPECTED = @"
helper.GiveMeSomeInts(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSplitLongInvokeArgumentListsOntoMultipleLines()
        {
            var longVariable = Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsAction = Variable(typeof(Action<int, int, int>), "threeIntsAction");
            var threeIntsCall = Invoke(threeIntsAction, longVariable, longVariable, longVariable);

            var longArgumentListBlock = Block(new[] { longVariable, threeIntsAction }, threeIntsCall);

            var translated = longArgumentListBlock.ToReadableString();

            const string EXPECTED = @"
int thisVariableReallyHasAVeryLongNameIndeed;
Action<int, int, int> threeIntsAction;
threeIntsAction.Invoke(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed);";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSplitLongTernariesOntoMultipleLines()
        {
            var longTernary = CreateLambda((int veryLongNamedVariable) =>
                veryLongNamedVariable > 10
                    ? veryLongNamedVariable * veryLongNamedVariable
                    : veryLongNamedVariable * veryLongNamedVariable * veryLongNamedVariable);

            var translated = longTernary.ToReadableString();

            const string EXPECTED = @"
veryLongNamedVariable => (veryLongNamedVariable > 10)
    ? veryLongNamedVariable * veryLongNamedVariable
    : (veryLongNamedVariable * veryLongNamedVariable) * veryLongNamedVariable";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSplitLongNestedTernariesOntoMultipleLines()
        {
            var longTernary = CreateLambda((int veryLongNamedVariable) =>
                (veryLongNamedVariable > 10)
                    ? (veryLongNamedVariable > 100)
                        ? veryLongNamedVariable * veryLongNamedVariable
                        : veryLongNamedVariable - veryLongNamedVariable
                    : veryLongNamedVariable * veryLongNamedVariable + veryLongNamedVariable);

            var translated = longTernary.ToReadableString();

            const string EXPECTED = @"
veryLongNamedVariable => (veryLongNamedVariable > 10)
    ? (veryLongNamedVariable > 100)
        ? veryLongNamedVariable * veryLongNamedVariable
        : veryLongNamedVariable - veryLongNamedVariable
    : (veryLongNamedVariable * veryLongNamedVariable) + veryLongNamedVariable";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSplitLongTernaryOptionsOntoMultipleLines()
        {
            var oneEqualsTwo = Equal(Constant(1), Constant(2));

            var defaultInt = Default(typeof(int));

            var threeIntsFunc = Variable(typeof(Func<int, int, int, int>), "threeIntsFunc");
            var longVariable = Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsCall = Invoke(threeIntsFunc, longVariable, longVariable, longVariable);

            var ternary = Condition(oneEqualsTwo, defaultInt, threeIntsCall);

            var translated = ternary.ToReadableString();

            const string EXPECTED = @"
(1 == 2)
    ? default(int)
    : threeIntsFunc.Invoke(
        thisVariableReallyHasAVeryLongNameIndeed,
        thisVariableReallyHasAVeryLongNameIndeed,
        thisVariableReallyHasAVeryLongNameIndeed)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSplitLongAssignmentsOntoMultipleLines()
        {
            var intVariable = Variable(typeof(int), "value");
            var threeIntsFunc = Variable(typeof(Func<int, int, int, int>), "threeIntsFunc");
            var longVariable = Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsSubCall = Invoke(threeIntsFunc, Constant(10), Constant(1), longVariable);
            var threeIntsCall = Invoke(threeIntsFunc, longVariable, threeIntsSubCall, longVariable);

            var assignment = Assign(intVariable, threeIntsCall);

            var translated = assignment.ToReadableString();

            const string EXPECTED = @"
value = threeIntsFunc.Invoke(
    thisVariableReallyHasAVeryLongNameIndeed,
    threeIntsFunc.Invoke(10, 1, thisVariableReallyHasAVeryLongNameIndeed),
    thisVariableReallyHasAVeryLongNameIndeed)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldSupportCustomIndentsUsingSpaces()
        {
            var anonType = new { String1 = default(string), String2 = default(string), String3 = default(string) }.GetType();
            var constructor = anonType.GetPublicInstanceConstructor(typeof(string), typeof(string), typeof(string));

            var longArgument = Constant("My, what a long argument value!");

            // ReSharper disable once AssignNullToNotNullAttribute
            var creation = New(constructor, longArgument, longArgument, longArgument);

            var translated = creation.ToReadableString(stgs => stgs.IndentUsing("  "));

            const string EXPECTED = @"
new 
{
  String1 = ""My, what a long argument value!"",
  String2 = ""My, what a long argument value!"",
  String3 = ""My, what a long argument value!""
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldDeclareAVariableIfUsedBeforeInitialisation()
        {
            var nameVariable = Variable(typeof(string), "name");
            var getNameVariable = Variable(typeof(Func<string>), "getName");
            var getNameLambda = Lambda(nameVariable);
            var getNameAssignment = Assign(getNameVariable, getNameLambda);
            var nameAssignment = Assign(nameVariable, Constant("Fred"));
            var getNameCall = Invoke(getNameVariable);

            var block = Block(
                new[] { nameVariable, getNameVariable },
                getNameAssignment,
                nameAssignment,
                getNameCall);

            var translated = block.ToReadableString();

            const string EXPECTED = @"
string name;
Func<string> getName = () => name;
name = ""Fred"";

return getName.Invoke();";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotVarAssignAVariableOfNonImpliedType()
        {
            var intsVariable = Variable(typeof(IEnumerable<int>), "ints");
            var newArray = NewArrayBounds(typeof(int), Constant(2));
            var assignment = Assign(intsVariable, newArray);

            var block = Block(new[] { intsVariable }, assignment);

            var translated = block.ToReadableString();

            translated.ShouldBe("IEnumerable<int> ints = new int[2];");
        }

        [Fact]
        public void ShouldNotVarAssignATernaryValueWithDifferingTypeBranches()
        {
            var intVariable = Variable(typeof(int), "i");
            var intVariableEqualsOne = Equal(intVariable, Constant(1));
            var newArray = NewArrayBounds(typeof(int?), Constant(0));
            var newList = New(typeof(List<int?>));

            var newArrayOrList = Condition(
                intVariableEqualsOne,
                newArray,
                newList,
                typeof(ICollection<int?>));

            var resultVariable = Variable(typeof(ICollection<int?>), "result");
            var assignResult = Assign(resultVariable, newArrayOrList);
            var assignBlock = Block(new[] { resultVariable }, assignResult);

            var translated = assignBlock.ToReadableString();

            translated.ShouldBe("ICollection<int?> result = (i == 1) ? new int?[0] : new List<int?>();");
        }

        [Fact]
        public void ShouldNotVarAssignAnOuterBlockDeclaredVariable()
        {
            var nameVariable = Variable(typeof(string), "name");
            var writeNameTwiceVariable = Variable(typeof(Action), "writeNameTwice");
            var writeLine = CreateLambda(() => Console.WriteLine(default(string)));
            var writeLineMethod = ((MethodCallExpression)writeLine.Body).Method;
            var writeLineCall = Call(writeLineMethod, nameVariable);
            var writeNameTwice = Block(writeLineCall, writeLineCall);
            var writeNameTwiceLambda = Lambda(writeNameTwice);
            var writeNameTwiceAssignment = Assign(writeNameTwiceVariable, writeNameTwiceLambda);
            var nameAssignment = Assign(nameVariable, Constant("Alice"));
            var writeNameTwiceCall = Invoke(writeNameTwiceVariable);

            var block = Block(
                new[] { nameVariable, writeNameTwiceVariable },
                Block(writeNameTwiceAssignment),
                Block(nameAssignment, writeNameTwiceCall));

            var translated = block.ToReadableString();

            const string EXPECTED = @"
string name;
Action writeNameTwice = () =>
{
    Console.WriteLine(name);
    Console.WriteLine(name);
};

name = ""Alice"";
writeNameTwice.Invoke();";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldVarAssignVariablesInSiblingBlocks()
        {
            var intVariable1 = Variable(typeof(int), "i");
            var assignVariable1 = Assign(intVariable1, Constant(1));
            var variable1Block = Block(new[] { intVariable1 }, assignVariable1);

            var intVariable2 = Variable(typeof(int), "j");
            var assignVariable2 = Assign(intVariable2, Constant(2));
            var variable2Block = Block(new[] { intVariable2 }, assignVariable2);

            var assign1Or2 = IfThenElse(
                Constant(true),
                variable1Block,
                variable2Block);

            var translated = assign1Or2.ToReadableString();

            const string EXPECTED = @"
if (true)
{
    var i = 1;
}
else
{
    var j = 2;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotVarAssignAVariableAssignedInATryButUsedInACatch()
        {
            var exceptionFactory = CreateLambda((int number) => new Exception(number.ToString()));
            var intVariable = exceptionFactory.Parameters.First();
            var newException = exceptionFactory.Body;

            var assignment = Assign(intVariable, Constant(10));
            var assignmentBlock = Block(assignment, Default(typeof(void)));

            var catchBlock = Catch(typeof(Exception), Throw(newException));
            var tryCatch = TryCatch(assignmentBlock, catchBlock);
            var tryCatchBlock = Block(new[] { intVariable }, tryCatch);

            var translated = tryCatchBlock.ToReadableString();

            const string EXPECTED = @"
int number;
try
{
    number = 10;
}
catch
{
    throw new Exception(number.ToString());
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldVarAssignAVariableUsedInNestedConstructs()
        {
            var returnLabel = Label(typeof(long), "Return");
            var streamVariable = Variable(typeof(Stream), "stream");

            var memoryStreamVariable = Variable(typeof(MemoryStream), "memoryStream");
            var streamAsMemoryStream = TypeAs(streamVariable, typeof(MemoryStream));
            var memoryStreamAssignment = Assign(memoryStreamVariable, streamAsMemoryStream);
            var nullMemoryStream = Default(memoryStreamVariable.Type);
            var memoryStreamNotNull = NotEqual(memoryStreamVariable, nullMemoryStream);
            var msLengthVariable = Variable(typeof(long), "msLength");
            var memoryStreamLength = Property(memoryStreamVariable, "Length");
            var msLengthAssignment = Assign(msLengthVariable, memoryStreamLength);

            var msTryBlock = Block(new[] { msLengthVariable }, msLengthAssignment, msLengthVariable);
            var newNotSupportedException = New(typeof(NotSupportedException));
            var throwMsException = Throw(newNotSupportedException, typeof(long));
            var msCatchBlock = Catch(typeof(Exception), throwMsException);
            var memoryStreamTryCatch = TryCatch(msTryBlock, msCatchBlock);
            var returnMemoryStreamResult = Return(returnLabel, memoryStreamTryCatch);
            var ifMemoryStreamTryCatch = IfThen(memoryStreamNotNull, returnMemoryStreamResult);

            var fileStreamVariable = Variable(typeof(FileStream), "fileStream");
            var streamAsFileStream = TypeAs(streamVariable, typeof(FileStream));
            var fileStreamAssignment = Assign(fileStreamVariable, streamAsFileStream);
            var nullFileStream = Default(fileStreamVariable.Type);
            var fileStreamNotNull = NotEqual(fileStreamVariable, nullFileStream);
            var fsLengthVariable = Variable(typeof(long), "fsLength");
            var fileStreamLength = Property(fileStreamVariable, "Length");
            var fsLengthAssignment = Assign(fsLengthVariable, fileStreamLength);

            var fsTryBlock = Block(new[] { fsLengthVariable }, fsLengthAssignment, fsLengthVariable);
            var newIoException = New(typeof(IOException));
            var throwIoException = Throw(newIoException, typeof(long));
            var fsCatchBlock = Catch(typeof(Exception), throwIoException);
            var fileStreamTryCatch = TryCatch(fsTryBlock, fsCatchBlock);
            var returnFileStreamResult = Return(returnLabel, fileStreamTryCatch);
            var ifFileStreamTryCatch = IfThen(fileStreamNotNull, returnFileStreamResult);

            var overallBlock = Block(
                new[] { memoryStreamVariable, fileStreamVariable },
                memoryStreamAssignment,
                ifMemoryStreamTryCatch,
                fileStreamAssignment,
                ifFileStreamTryCatch,
                Label(returnLabel, Constant(0L)));

            var overallCatchBlock = Catch(typeof(Exception), Constant(-1L));
            var overallTryCatch = TryCatch(overallBlock, overallCatchBlock);

            var translated = overallTryCatch.ToReadableString();

            const string EXPECTED = @"
try
{
    var memoryStream = stream as MemoryStream;

    if (memoryStream != null)
    {
        return 
        {
            try
            {
                var msLength = memoryStream.Length;

                return msLength;
            }
            catch
            {
                throw new NotSupportedException();
            }
        };
    }

    var fileStream = stream as FileStream;

    if (fileStream != null)
    {
        return 
        {
            try
            {
                var fsLength = fileStream.Length;

                return fsLength;
            }
            catch
            {
                throw new IOException();
            }
        };
    }

    return 0L;
}
catch
{
    return -1L;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldVarAssignAVariableReusedInASiblingBlock()
        {
            var intVariable = Variable(typeof(int), "i");
            var assignVariable1 = Assign(intVariable, Constant(1));
            var assignmentBlock = Block(new[] { intVariable }, assignVariable1);

            var assignVariable2 = Assign(intVariable, Constant(2));
            var assignment2Block = Block(new[] { intVariable }, assignVariable2);

            var assign1Or2 = IfThenElse(
                Constant(true),
                assignmentBlock,
                assignment2Block);

            var translated = assign1Or2.ToReadableString();

            const string EXPECTED = @"
if (true)
{
    var i = 1;
}
else
{
    var i = 2;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotIndentParamsArrayArguments()
        {
            var stringJoiner = CreateLambda(() =>
                JoinStrings(",", "[", "i", "]", "[", "j", "]", "[", "k", "]"));

            const string EXPECTED = @"
WhenFormattingCode.JoinStrings(
    "","",
    ""["",
    ""i"",
    ""]"",
    ""["",
    ""j"",
    ""]"",
    ""["",
    ""k"",
    ""]"")";

            var translated = stringJoiner.Body.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIndentParamsArrayArgumentsInAnIfTest()
        {
            var stringTest = CreateLambda(() =>
                JoinStrings(",", "[", "i", "]", "[", "j", "]", "[", "k", "]") != string.Empty);

            var doNothing = Default(typeof(void));
            var ifTestDoNothing = IfThen(stringTest.Body, doNothing);

            const string EXPECTED = @"
if (WhenFormattingCode.JoinStrings(
    "","",
    ""["",
    ""i"",
    ""]"",
    ""["",
    ""j"",
    ""]"",
    ""["",
    ""k"",
    ""]"") != string.Empty)
{
}";
            var translated = ifTestDoNothing.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldOnlyRemoveParenthesesIfNecessary()
        {
            var intVariable = Variable(typeof(int), "i");
            var intVariableIsOne = Equal(intVariable, Constant(1));

            var objectVariable = Variable(typeof(object), "o");
            var objectCastToInt = Convert(objectVariable, typeof(int));
            var intToStringMethod = typeof(object).GetPublicInstanceMethod("ToString");
            var intToStringCall = Call(objectCastToInt, intToStringMethod);

            var emptyString = CreateLambda(() => string.Empty);

            var toStringOrEmptyString = Condition(
                intVariableIsOne,
                emptyString.Body,
                intToStringCall);

            var translated = toStringOrEmptyString.ToReadableString();

            translated.ShouldBe("(i == 1) ? string.Empty : ((int)o).ToString()");
        }

        [Fact]
        public void ShouldNotRemoveParenthesesFromACastObjectChainedMethodCall()
        {
            var intArrayConverter = CreateLambda(
                (IList<int> ints) => ((int[])ints)
                    .ToString()
                    .Split(new[] { ',' }, 3, StringSplitOptions.RemoveEmptyEntries));

            var stringArrayVariable = Variable(typeof(string[]), "strings");
            var assignment = Assign(stringArrayVariable, intArrayConverter.Body);

            var translated = assignment.ToReadableString();

            translated.ShouldBe(
                "strings = ((int[])ints)" +
                    ".ToString()" +
                    ".Split(new[] { ',' }, 3, StringSplitOptions.RemoveEmptyEntries)");
        }

        [Fact]
        public void ShouldNotRemoveParenthesesFromMultiParameterLambdaArguments()
        {
            var stringsConverter = CreateLambda(
                (IEnumerable<string> strings) => strings.Select((str, i) => string.Join(i + ": ", new[] { str })).ToArray());

#if NET35
            // string.Join()'s set of arguments is not a params array in .NET 3.5:
            const string EXPECTED = "strings.Select((str, i) => string.Join(i + \": \", new[] { str })).ToArray()";
#else
            const string EXPECTED = "strings.Select((str, i) => string.Join(i + \": \", str)).ToArray()";
#endif

            var translated = stringsConverter.Body.ToReadableString();

            translated.ShouldBe(EXPECTED);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/9
        [Fact]
        public void ShouldNotRemoveParenthesesFromALambdaInvokeResultAssignment()
        {
            var intsAdder = CreateLambda((int a, int b) => a + b);
            var one = Constant(1);
            var two = Constant(2);
            var lambdaInvocation = Invoke(intsAdder, one, two);
            var result = Variable(typeof(int), "result");
            var assignInvokeResult = Assign(result, lambdaInvocation);

            const string EXPECTED = "result = ((a, b) => a + b).Invoke(1, 2)";
            var translated = assignInvokeResult.ToReadableString();

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldNotIndentSingleArgumentBlockParameters()
        {
            var writeString = CreateLambda(() => Console.WriteLine("String!")).Body;

            var stringsBlock = Block(
                writeString,
                writeString,
                writeString,
                Constant("All done!"));

            var listParameter = Parameter(typeof(List<string>), "strings");

            var addString = Call(
                listParameter,
                listParameter.Type.GetPublicMethod("Add"),
                stringsBlock);

            var addStringLambda = Lambda<Action<List<string>>>(addString, listParameter);

            var translated = addStringLambda.ToReadableString();

            const string EXPECTED = @"
strings => strings.Add(
{
    Console.WriteLine(""String!"");
    Console.WriteLine(""String!"");
    Console.WriteLine(""String!"");

    return ""All done!"";
})";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldPlaceSingleArgumentLambdaParametersOnMethodNameLine()
        {
            var stringParam1 = Parameter(typeof(string), "string1");
            var stringParam2 = Parameter(typeof(string), "string2");

            var writeString = CreateLambda(() => Console.WriteLine("String!")).Body;
            var writeStringsBlock = Block(writeString, writeString, writeString);

            var stringLambda = Lambda<Action<string, string>>(
                writeStringsBlock,
                stringParam1,
                stringParam2);

            var lambdaMethod = typeof(HelperClass).GetPublicStaticMethod("GiveMeALambda");
            var lambdaMethodCall = Call(lambdaMethod, stringLambda);

            var translated = lambdaMethodCall.ToReadableString();

            const string EXPECTED = @"
HelperClass.GiveMeALambda((string1, string2) =>
{
    Console.WriteLine(""String!"");
    Console.WriteLine(""String!"");
    Console.WriteLine(""String!"");
})";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseMethodGroupsForStaticMethods()
        {
            var selectTimeSpans = CreateLambda(() => new[] { 1d, 2d, 3d }.Select(TimeSpan.FromDays));

            var translated = selectTimeSpans.Body.ToReadableString();

            translated.ShouldBe("new[] { 1d, 2d, 3d }.Select(TimeSpan.FromDays)");
        }

        [Fact]
        public void ShouldUseMethodGroupsForInstanceMethods()
        {
            var selectStrings = CreateLambda((IntConverter converter)
                => new[] { 1, 2, 3 }.Select(converter.Convert));

            var translated = selectStrings.Body.ToReadableString();

            translated.ShouldBe("new[] { 1, 2, 3 }.Select(converter.Convert)");
        }

        [Fact]
        public void ShouldUseMethodGroupsForLocalFuncs()
        {
            Func<int, string> intConverter = i => i.ToString();

            var selectStrings = CreateLambda(() => new[] { 1, 2, 3 }.Select(intConverter));

            var translated = selectStrings.Body.ToReadableString();

            translated.ShouldBe("new[] { 1, 2, 3 }.Select(intConverter)");
        }

        [Fact]
        public void ShouldConvertAnExtensionMethodArgumentToAMethodGroup()
        {
            var allItemsContained = CreateLambda((List<int> list, IEnumerable<int> items)
                => list.All(i => items.Contains(i)));

            var translated = allItemsContained.Body.ToReadableString();

            translated.ShouldBe("list.All(items.Contains)");
        }

        [Fact]
        public void ShouldConvertAStaticMethodArgumentToAMethodGroup()
        {
            var parseTimeSpans = CreateLambda((List<double> list)
                 => list.Select(i => TimeSpan.FromDays(i)));

            var translated = parseTimeSpans.Body.ToReadableString();

            translated.ShouldBe("list.Select(TimeSpan.FromDays)");
        }

        [Fact]
        public void ShouldConvertAnInstanceMethodArgumentToAMethodGroup()
        {
            var copy = CreateLambda((List<int> list, ICollection<int> items) =>
                list.ForEach(i => items.Add(i)));

            var translated = copy.Body.ToReadableString();

            translated.ShouldBe("list.ForEach(items.Add)");
        }

        [Fact]
        public void ShouldConvertADelegateMethodArgumentToAMethodGroup()
        {
            Func<IntEvaluator, int, bool> evaluatorInvoker = (evaluator, i) => evaluator.Invoke(i);

            var listContainsEvaluator = CreateLambda((List<int> list, int i)
                => evaluatorInvoker.Invoke(x => list.Contains(x), i));

            var translated = listContainsEvaluator.Body.ToReadableString();

            translated.ShouldBe("evaluatorInvoker.Invoke(list.Contains, i)");
        }

        [Fact]
        public void ShouldNotConvertAModifyingArgumentToAMethodGroup()
        {
            var copy = CreateLambda((List<int> list, ICollection<string> items)
                 => list.ForEach(i => items.Add(i.ToString())));

            var translated = copy.Body.ToReadableString();

            translated.ShouldBe("list.ForEach(i => items.Add(i.ToString()))");
        }

        [Fact]
        public void ShouldNotConvertAModifyingReturnTypeToAMethodGroup()
        {
            Func<IntEvaluatorNullable, int, bool?> evaluatorInvoker = (evaluator, i) => evaluator.Invoke(i);

            var listContainsEvaluator = CreateLambda((List<int> list, int i)
                => evaluatorInvoker.Invoke(x => list.Contains(x), i));

            var translated = listContainsEvaluator.Body.ToReadableString();

            translated.ShouldBe("evaluatorInvoker.Invoke(x => (bool?)list.Contains(x), i)");
        }

        [Fact]
        public void ShouldNotConvertAnIgnoredReturnTypeToAMethodGroup()
        {
            var removeItems = CreateLambda((List<int> items, List<int> itemsToRemove)
                => itemsToRemove.ForEach(item => items.Remove(item)));

            var translated = removeItems.Body.ToReadableString();

            translated.ShouldBe("itemsToRemove.ForEach(item => items.Remove(item))");
        }

        [Fact]
        public void ShouldSplitLongChainedMethodsOntoMultipleLines()
        {
            var longMethodChain = CreateLambda(() =>
                Enumerable
                    .Range(1, 10)
                    .Select(System.Convert.ToInt64)
                    .ToArray()
                    .Select(System.Convert.ToInt32)
                    .OrderByDescending(i => i)
                    .ToList());

            var translated = longMethodChain.Body.ToReadableString();

            const string EXPECTED = @"
Enumerable
    .Range(1, 10)
    .Select(Convert.ToInt64)
    .ToArray()
    .Select(Convert.ToInt32)
    .OrderByDescending(i => i)
    .ToList()";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAComplexMethodArgument()
        {
            var intVariable = Variable(typeof(int), "intValue");
            var dictionaryVariable = Variable(typeof(Dictionary<string, int>), "dictionary_String_IntValues");
            var tryGetValueMethod = dictionaryVariable.Type.GetPublicInstanceMethod("TryGetValue", 2);
            var key = Constant("NumberThatIWantToGet");
            var tryGetValueCall = Call(dictionaryVariable, tryGetValueMethod, key, intVariable);

            var defaultInt = Default(typeof(int));
            var valueOrDefault = Condition(tryGetValueCall, intVariable, defaultInt);
            var valueOrDefaultBlock = Block(new[] { intVariable }, valueOrDefault);

            var helperCtor = typeof(HelperClass).GetPublicInstanceConstructors().First();
            var helper = New(helperCtor, defaultInt, defaultInt, defaultInt);
            var intsMethod = helper.Type.GetPublicInstanceMethod(nameof(HelperClass.GiveMeSomeInts));
            var methodCall = Call(helper, intsMethod, defaultInt, valueOrDefaultBlock, defaultInt);

            var translated = methodCall.ToReadableString();

            const string EXPECTED = @"
new HelperClass(default(int), default(int), default(int)).GiveMeSomeInts(
    default(int),
    {
        int intValue;
        return dictionary_String_IntValues.TryGetValue(""NumberThatIWantToGet"", out intValue)
            ? intValue
            : default(int);
    },
    default(int))";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateNullToNull()
        {
            var translated = default(Expression).ToReadableString();

            translated.ShouldBeNull();
        }

        [Fact]
        public void ShouldLeaveABlankLineAfterAMultipleLineExpression()
        {
            var longCallChain = CreateLambda((List<int> list) => list
                .Select(i => i * 2)
                .Select(i => i * 3)
                .Select(i => i * 4)
                .ToArray());

            var longChainblock = Block(longCallChain.Body, longCallChain.Body);

            var translated = longChainblock.ToReadableString();

            const string EXPECTED = @"
list
    .Select(i => i * 2)
    .Select(i => i * 3)
    .Select(i => i * 4)
    .ToArray();

return list
    .Select(i => i * 2)
    .Select(i => i * 3)
    .Select(i => i * 4)
    .ToArray();";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldLeaveABlankLineBeforeAnIfStatement()
        {
            var intVariable = Variable(typeof(int), "i");
            var zero = Constant(0);
            var intVariableEqualsZero = Equal(intVariable, zero);
            var doNothing = Default(typeof(void));
            var ifIntEqualsZeroDoNothing = IfThen(intVariableEqualsZero, doNothing);

            var block = Block(new[] { intVariable }, ifIntEqualsZeroDoNothing);

            var translated = block.ToReadableString();

            const string EXPECTED = @"
int i;

if (i == 0)
{
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotLeaveDoubleBlankLinesBetweenIfStatements()
        {
            var intVariable = Variable(typeof(int), "i");
            var one = Constant(1);
            var intVariableEqualsOne = Equal(intVariable, one);
            var doNothing = Default(typeof(void));
            var ifIntEqualsOneDoNothing = IfThen(intVariableEqualsOne, doNothing);

            var block = Block(
                new[] { intVariable },
                ifIntEqualsOneDoNothing,
                ifIntEqualsOneDoNothing);

            var translated = block.ToReadableString();

            const string EXPECTED = @"
int i;

if (i == 1)
{
}

if (i == 1)
{
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotLeaveDoubleBlankLinesBetweenInitAndIfStatements()
        {
            var writeWat = CreateLambda(() => Console.WriteLine("Wat"));
            var read = CreateLambda<long>(() => Console.Read());

            var newMemoryStream = New(typeof(MemoryStream));
            var positionProperty = newMemoryStream.Type.GetPublicInstanceProperty("Position");
            var valueBlock = Block(writeWat.Body, read.Body);
            var positionInit = Bind(positionProperty, valueBlock);
            var memoryStreamInit = MemberInit(newMemoryStream, positionInit);

            var intVariable = Variable(typeof(int), "i");
            var one = Constant(1);
            var intVariableEqualsOne = Equal(intVariable, one);
            var doNothing = Default(typeof(void));
            var ifIntEqualsOneDoNothing = IfThen(intVariableEqualsOne, doNothing);

            var block = Block(memoryStreamInit, ifIntEqualsOneDoNothing);

            var translated = block.ToReadableString();

            const string EXPECTED = @"
new MemoryStream
{
    Position = 
    {
        Console.WriteLine(""Wat"");

        return (long)Console.Read();
    }
};

if (i == 1)
{
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateMultilineBlockSingleMethodArguments()
        {
            var intVariable = Variable(typeof(int), "i");
            var variableInit = Assign(intVariable, Constant(3));
            var variableMultiplyFive = Multiply(intVariable, Constant(5));
            var variableAdditionOne = Assign(intVariable, variableMultiplyFive);
            var variableDivideThree = Divide(intVariable, Constant(3));
            var variableAdditionTwo = Assign(intVariable, variableDivideThree);

            var argumentBlock = Block(
                new[] { intVariable },
                variableInit,
                variableAdditionOne,
                variableAdditionTwo,
                intVariable);

            var catchBlock = Catch(
                typeof(Exception),
                Block(ReadableExpression.Comment("So what!"), Constant(0)));

            var tryCatch = TryCatch(argumentBlock, catchBlock);

            var collectionVariable = Variable(typeof(ICollection<int>), "ints");
            var addMethod = collectionVariable.Type.GetPublicInstanceMethod("Add");
            var addMethodCall = Call(collectionVariable, addMethod, tryCatch);

            var translated = addMethodCall.ToReadableString();

            const string EXPECTED = @"
ints.Add(
{
    try
    {
        var i = 3;
        i = i * 5;
        i = i / 3;

        return i;
    }
    catch
    {
        // So what!
        return 0;
    }
})";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // ReSharper disable once UnusedParameter.Local
        private static string JoinStrings(params string[] strings) => null;
    }

    #region Helper Classes

    // ReSharper disable UnusedParameter.Local
    internal class HelperClass
    {
        public HelperClass(int intOne, int intTwo, int intThree)
        {
        }

        public void GiveMeSomeInts(int intOne, int intTwo, int intThree)
        {
        }

        public void GiveMeFourInts(int intOne, int intTwo, int intThree, int intFour)
        {
        }

        public static void GiveMeALambda(Action<string, string> lambda)
        {
        }
    }
    // ReSharper restore UnusedParameter.Local

    internal delegate bool IntEvaluator(int value);

    internal delegate bool? IntEvaluatorNullable(int value);

    internal class IntConverter
    {
        public string Convert(int value)
        {
            return value.ToString();
        }
    }

    internal class OuterClass
    {
        internal class InnerClass
        {
            internal class Nested
            {
            }
        }
    }

    // ReSharper disable once UnusedTypeParameter
    internal class OuterGeneric<TOuter>
    {
        // ReSharper disable once UnusedTypeParameter
        internal class InnerGeneric<TInner>
        {
            internal class Nested
            {
            }
        }
    }

    #endregion
}
