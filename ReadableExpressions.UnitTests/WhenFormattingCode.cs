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
    public class WhenFormattingCode : TestClassBase
    {
        [Fact]
        public void ShouldSplitLongConstructorArgumentListsOntoMultipleLines()
        {
            var helperVariable = Expression.Variable(typeof(HelperClass), "helper");
            var helperConstructor = helperVariable.Type.GetConstructors().First();
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var newHelper = Expression.New(helperConstructor, longVariable, longVariable, longVariable);
            var helperAssignment = Expression.Assign(helperVariable, newHelper);

            var longArgumentListBlock = Expression.Block(new[] { helperVariable }, helperAssignment);

            var translated = ToReadableString(longArgumentListBlock);

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

            var helperVariable = Expression.Variable(typeof(HelperClass), "helper");
            var intVariable = Expression.Variable(typeof(int), "intVariable");

            var intsMethodCall = Expression.Call(
                helperVariable,
                intsMethod,
                intVariable,
                intVariable,
                intVariable,
                intVariable);

            var translated = ToReadableString(intsMethodCall);

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

            var helperVariable = Expression.Variable(typeof(HelperClass), "helper");
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var intsMethodCall = Expression.Call(helperVariable, intsMethod, longVariable, longVariable, longVariable);

            var translated = ToReadableString(intsMethodCall);

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
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsAction = Expression.Variable(typeof(Action<int, int, int>), "threeIntsAction");
            var threeIntsCall = Expression.Invoke(threeIntsAction, longVariable, longVariable, longVariable);

            var longArgumentListBlock = Expression.Block(new[] { longVariable, threeIntsAction }, threeIntsCall);

            var translated = ToReadableString(longArgumentListBlock);

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

            var translated = ToReadableString(longTernary);

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

            var translated = ToReadableString(longTernary);

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
            var oneEqualsTwo = Expression.Equal(Expression.Constant(1), Expression.Constant(2));

            var defaultInt = Expression.Default(typeof(int));

            var threeIntsFunc = Expression.Variable(typeof(Func<int, int, int, int>), "threeIntsFunc");
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsCall = Expression.Invoke(threeIntsFunc, longVariable, longVariable, longVariable);

            var ternary = Expression.Condition(oneEqualsTwo, defaultInt, threeIntsCall);

            var translated = ToReadableString(ternary);

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
            var intVariable = Expression.Variable(typeof(int), "value");
            var threeIntsFunc = Expression.Variable(typeof(Func<int, int, int, int>), "threeIntsFunc");
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsSubCall = Expression.Invoke(threeIntsFunc, Expression.Constant(10), Expression.Constant(1), longVariable);
            var threeIntsCall = Expression.Invoke(threeIntsFunc, longVariable, threeIntsSubCall, longVariable);

            var assignment = Expression.Assign(intVariable, threeIntsCall);

            var translated = ToReadableString(assignment);

            const string EXPECTED = @"
value = threeIntsFunc.Invoke(
    thisVariableReallyHasAVeryLongNameIndeed,
    threeIntsFunc.Invoke(10, 1, thisVariableReallyHasAVeryLongNameIndeed),
    thisVariableReallyHasAVeryLongNameIndeed)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldDeclareAVariableIfUsedBeforeInitialisation()
        {
            var nameVariable = Expression.Variable(typeof(string), "name");
            var getNameVariable = Expression.Variable(typeof(Func<string>), "getName");
            var getNameLambda = Expression.Lambda(nameVariable);
            var getNameAssignment = Expression.Assign(getNameVariable, getNameLambda);
            var nameAssignment = Expression.Assign(nameVariable, Expression.Constant("Fred"));
            var getNameCall = Expression.Invoke(getNameVariable);

            var block = Expression.Block(
                new[] { nameVariable, getNameVariable },
                getNameAssignment,
                nameAssignment,
                getNameCall);

            var translated = ToReadableString(block);

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
            var intsVariable = Expression.Variable(typeof(IEnumerable<int>), "ints");
            var newArray = Expression.NewArrayBounds(typeof(int), Expression.Constant(2));
            var assignment = Expression.Assign(intsVariable, newArray);

            var block = Expression.Block(new[] { intsVariable }, assignment);

            var translated = ToReadableString(block);

            translated.ShouldBe("IEnumerable<int> ints = new int[2];");
        }

        [Fact]
        public void ShouldNotVarAssignATernaryValueWithDifferingTypeBranches()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intVariableEqualsOne = Expression.Equal(intVariable, Expression.Constant(1));
            var newArray = Expression.NewArrayBounds(typeof(int?), Expression.Constant(0));
            var newList = Expression.New(typeof(List<int?>));

            var newArrayOrList = Expression.Condition(
                intVariableEqualsOne,
                newArray,
                newList,
                typeof(ICollection<int?>));

            var resultVariable = Expression.Variable(typeof(ICollection<int?>), "result");
            var assignResult = Expression.Assign(resultVariable, newArrayOrList);
            var assignBlock = Expression.Block(new[] { resultVariable }, assignResult);

            var translated = ToReadableString(assignBlock);

            translated.ShouldBe("ICollection<int?> result = (i == 1) ? new int?[0] : new List<int?>();");
        }

        [Fact]
        public void ShouldNotVarAssignAnOuterBlockDeclaredVariable()
        {
            var nameVariable = Expression.Variable(typeof(string), "name");
            var writeNameTwiceVariable = Expression.Variable(typeof(Action), "writeNameTwice");
            var writeLine = CreateLambda(() => Console.WriteLine(default(string)));
            var writeLineMethod = ((MethodCallExpression)writeLine.Body).Method;
            var writeLineCall = Expression.Call(writeLineMethod, nameVariable);
            var writeNameTwice = Expression.Block(writeLineCall, writeLineCall);
            var writeNameTwiceLambda = Expression.Lambda(writeNameTwice);
            var writeNameTwiceAssignment = Expression.Assign(writeNameTwiceVariable, writeNameTwiceLambda);
            var nameAssignment = Expression.Assign(nameVariable, Expression.Constant("Alice"));
            var writeNameTwiceCall = Expression.Invoke(writeNameTwiceVariable);

            var block = Expression.Block(
                new[] { nameVariable, writeNameTwiceVariable },
                Expression.Block(writeNameTwiceAssignment),
                Expression.Block(nameAssignment, writeNameTwiceCall));

            var translated = ToReadableString(block);

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
            var intVariable1 = Expression.Variable(typeof(int), "i");
            var assignVariable1 = Expression.Assign(intVariable1, Expression.Constant(1));
            var variable1Block = Expression.Block(new[] { intVariable1 }, assignVariable1);

            var intVariable2 = Expression.Variable(typeof(int), "j");
            var assignVariable2 = Expression.Assign(intVariable2, Expression.Constant(2));
            var variable2Block = Expression.Block(new[] { intVariable2 }, assignVariable2);

            var assign1Or2 = Expression.IfThenElse(
                Expression.Constant(true),
                variable1Block,
                variable2Block);

            var translated = ToReadableString(assign1Or2);

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

            var assignment = Expression.Assign(intVariable, Expression.Constant(10));
            var assignmentBlock = Expression.Block(assignment, Expression.Default(typeof(void)));

            var catchBlock = Expression.Catch(typeof(Exception), Expression.Throw(newException));
            var tryCatch = Expression.TryCatch(assignmentBlock, catchBlock);
            var tryCatchBlock = Expression.Block(new[] { intVariable }, tryCatch);

            var translated = ToReadableString(tryCatchBlock);

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
            var returnLabel = Expression.Label(typeof(long), "Return");
            var streamVariable = Expression.Variable(typeof(Stream), "stream");

            var memoryStreamVariable = Expression.Variable(typeof(MemoryStream), "memoryStream");
            var streamAsMemoryStream = Expression.TypeAs(streamVariable, typeof(MemoryStream));
            var memoryStreamAssignment = Expression.Assign(memoryStreamVariable, streamAsMemoryStream);
            var nullMemoryStream = Expression.Default(memoryStreamVariable.Type);
            var memoryStreamNotNull = Expression.NotEqual(memoryStreamVariable, nullMemoryStream);
            var msLengthVariable = Expression.Variable(typeof(long), "msLength");
            var memoryStreamLength = Expression.Property(memoryStreamVariable, "Length");
            var msLengthAssignment = Expression.Assign(msLengthVariable, memoryStreamLength);

            var msTryBlock = Expression.Block(new[] { msLengthVariable }, msLengthAssignment, msLengthVariable);
            var newNotSupportedException = Expression.New(typeof(NotSupportedException));
            var throwMsException = Expression.Throw(newNotSupportedException, typeof(long));
            var msCatchBlock = Expression.Catch(typeof(Exception), throwMsException);
            var memoryStreamTryCatch = Expression.TryCatch(msTryBlock, msCatchBlock);
            var returnMemoryStreamResult = Expression.Return(returnLabel, memoryStreamTryCatch);
            var ifMemoryStreamTryCatch = Expression.IfThen(memoryStreamNotNull, returnMemoryStreamResult);

            var fileStreamVariable = Expression.Variable(typeof(FileStream), "fileStream");
            var streamAsFileStream = Expression.TypeAs(streamVariable, typeof(FileStream));
            var fileStreamAssignment = Expression.Assign(fileStreamVariable, streamAsFileStream);
            var nullFileStream = Expression.Default(fileStreamVariable.Type);
            var fileStreamNotNull = Expression.NotEqual(fileStreamVariable, nullFileStream);
            var fsLengthVariable = Expression.Variable(typeof(long), "fsLength");
            var fileStreamLength = Expression.Property(fileStreamVariable, "Length");
            var fsLengthAssignment = Expression.Assign(fsLengthVariable, fileStreamLength);

            var fsTryBlock = Expression.Block(new[] { fsLengthVariable }, fsLengthAssignment, fsLengthVariable);
            var newIoException = Expression.New(typeof(IOException));
            var throwIoException = Expression.Throw(newIoException, typeof(long));
            var fsCatchBlock = Expression.Catch(typeof(Exception), throwIoException);
            var fileStreamTryCatch = Expression.TryCatch(fsTryBlock, fsCatchBlock);
            var returnFileStreamResult = Expression.Return(returnLabel, fileStreamTryCatch);
            var ifFileStreamTryCatch = Expression.IfThen(fileStreamNotNull, returnFileStreamResult);

            var overallBlock = Expression.Block(
                new[] { memoryStreamVariable, fileStreamVariable },
                memoryStreamAssignment,
                ifMemoryStreamTryCatch,
                fileStreamAssignment,
                ifFileStreamTryCatch,
                Expression.Label(returnLabel, Expression.Constant(0L)));

            var overallCatchBlock = Expression.Catch(typeof(Exception), Expression.Constant(-1L));
            var overallTryCatch = Expression.TryCatch(overallBlock, overallCatchBlock);

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

            var translated = ToReadableString(overallTryCatch);

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

            var translated = ToReadableString(stringJoiner.Body);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIndentParamsArrayArgumentsInAnIfTest()
        {
            var stringTest = CreateLambda(() =>
                JoinStrings(",", "[", "i", "]", "[", "j", "]", "[", "k", "]") != string.Empty);

            var doNothing = Expression.Default(typeof(void));
            var ifTestDoNothing = Expression.IfThen(stringTest.Body, doNothing);

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
            var translated = ToReadableString(ifTestDoNothing);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnExtensionExpressionType()
        {
            var extension = new ExtensionExpression();
            var translated = ToReadableString(extension);

            extension.ToString().ShouldBe(translated);
        }

        [Fact]
        public void ShouldTranslateAnUnknownExpressionType()
        {
            var unknown = new UnknownExpression();
            var translated = ToReadableString(unknown);

            unknown.ToString().ShouldBe(translated);
        }

        [Fact]
        public void ShouldOnlyRemoveParenthesesIfNecessary()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intVariableIsOne = Expression.Equal(intVariable, Expression.Constant(1));

            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectCastToInt = Expression.Convert(objectVariable, typeof(int));
            var intToStringMethod = typeof(object).GetPublicInstanceMethod("ToString");
            var intToStringCall = Expression.Call(objectCastToInt, intToStringMethod);

            var emptyString = CreateLambda(() => string.Empty);

            var toStringOrEmptyString = Expression.Condition(
                intVariableIsOne,
                emptyString.Body,
                intToStringCall);

            var translated = ToReadableString(toStringOrEmptyString);

            translated.ShouldBe("(i == 1) ? string.Empty : ((int)o).ToString()");
        }

        [Fact]
        public void ShouldNotRemoveParenthesesFromACastObjectChainedMethodCall()
        {
            var intArrayConverter = CreateLambda(
                (IList<int> ints) => ((int[])ints).ToString().Split(','));

            var stringArrayVariable = Expression.Variable(typeof(string[]), "strings");
            var assignment = Expression.Assign(stringArrayVariable, intArrayConverter.Body);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("strings = ((int[])ints).ToString().Split(',')");
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

            var translated = ToReadableString(stringsConverter.Body);

            translated.ShouldBe(EXPECTED);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/9
        [Fact]
        public void ShouldNotRemoveParenthesesFromALambdaInvokeResultAssignment()
        {
            var intsAdder = CreateLambda((int a, int b) => a + b);
            var one = Expression.Constant(1);
            var two = Expression.Constant(2);
            var lambdaInvocation = Expression.Invoke(intsAdder, one, two);
            var result = Expression.Variable(typeof(int), "result");
            var assignInvokeResult = Expression.Assign(result, lambdaInvocation);

            const string EXPECTED = "result = ((a, b) => a + b).Invoke(1, 2)";
            var translated = ToReadableString(assignInvokeResult);

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldUseMethodGroupsForStaticMethods()
        {
            var selectTimeSpans = CreateLambda(() => new[] { 1d, 2d, 3d }.Select(TimeSpan.FromDays));

            var translated = ToReadableString(selectTimeSpans.Body);

            translated.ShouldBe("new[] { 1d, 2d, 3d }.Select(TimeSpan.FromDays)");
        }

        [Fact]
        public void ShouldUseMethodGroupsForInstanceMethods()
        {
            var selectStrings = CreateLambda((IntConverter converter)
                => new[] { 1, 2, 3 }.Select(converter.Convert));

            var translated = ToReadableString(selectStrings.Body);

            translated.ShouldBe("new[] { 1, 2, 3 }.Select(converter.Convert)");
        }

        [Fact]
        public void ShouldUseMethodGroupsForLocalFuncs()
        {
            Func<int, string> intConverter = i => i.ToString();

            var selectStrings = CreateLambda(() => new[] { 1, 2, 3 }.Select(intConverter));

            var translated = ToReadableString(selectStrings.Body);

            translated.ShouldBe("new[] { 1, 2, 3 }.Select(intConverter)");
        }

        [Fact]
        public void ShouldConvertAnExtensionMethodArgumentToAMethodGroup()
        {
            var allItemsContained = CreateLambda((List<int> list, IEnumerable<int> items)
                => list.All(i => items.Contains(i)));

            var translated = ToReadableString(allItemsContained.Body);

            translated.ShouldBe("list.All(items.Contains)");
        }

        [Fact]
        public void ShouldConvertAStaticMethodArgumentToAMethodGroup()
        {
            var parseTimeSpans = CreateLambda((List<double> list)
                 => list.Select(i => TimeSpan.FromDays(i)));

            var translated = ToReadableString(parseTimeSpans.Body);

            translated.ShouldBe("list.Select(TimeSpan.FromDays)");
        }

        [Fact]
        public void ShouldConvertAnInstanceMethodArgumentToAMethodGroup()
        {
            var copy = CreateLambda((List<int> list, ICollection<int> items) =>
                list.ForEach(i => items.Add(i)));

            var translated = ToReadableString(copy.Body);

            translated.ShouldBe("list.ForEach(items.Add)");
        }

        [Fact]
        public void ShouldConvertADelegateMethodArgumentToAMethodGroup()
        {
            Func<IntEvaluator, int, bool> evaluatorInvoker = (evaluator, i) => evaluator.Invoke(i);

            var listContainsEvaluator = CreateLambda((List<int> list, int i)
                => evaluatorInvoker.Invoke(x => list.Contains(x), i));

            var translated = ToReadableString(listContainsEvaluator.Body);

            translated.ShouldBe("evaluatorInvoker.Invoke(list.Contains, i)");
        }

        [Fact]
        public void ShouldNotConvertAModifyingArgumentToAMethodGroup()
        {
            var copy = CreateLambda((List<int> list, ICollection<string> items)
                 => list.ForEach(i => items.Add(i.ToString())));

            var translated = ToReadableString(copy.Body);

            translated.ShouldBe("list.ForEach(i => items.Add(i.ToString()))");
        }

        [Fact]
        public void ShouldNotConvertAModifyingReturnTypeToAMethodGroup()
        {
            Func<IntEvaluatorNullable, int, bool?> evaluatorInvoker = (evaluator, i) => evaluator.Invoke(i);

            var listContainsEvaluator = CreateLambda((List<int> list, int i)
                => evaluatorInvoker.Invoke(x => list.Contains(x), i));

            var translated = ToReadableString(listContainsEvaluator.Body);

            translated.ShouldBe("evaluatorInvoker.Invoke(x => (bool?)list.Contains(x), i)");
        }

        [Fact]
        public void ShouldSplitLongChainedMethodsOntoMultipleLines()
        {
            var longMethodChain = CreateLambda(() =>
                Enumerable
                    .Range(1, 10)
                    .Select(Convert.ToInt64)
                    .ToArray()
                    .Select(Convert.ToInt32)
                    .OrderByDescending(i => i)
                    .ToList());

            var translated = ToReadableString(longMethodChain.Body);

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
            var intVariable = Expression.Variable(typeof(int), "intValue");
            var dictionaryVariable = Expression.Variable(typeof(Dictionary<string, int>), "dictionary_String_IntValues");
            var tryGetValueMethod = dictionaryVariable.Type.GetPublicInstanceMethod("TryGetValue", 2);
            var key = Expression.Constant("NumberThatIWantToGet");
            var tryGetValueCall = Expression.Call(dictionaryVariable, tryGetValueMethod, key, intVariable);

            var defaultInt = Expression.Default(typeof(int));
            var valueOrDefault = Expression.Condition(tryGetValueCall, intVariable, defaultInt);
            var valueOrDefaultBlock = Expression.Block(new[] { intVariable }, valueOrDefault);

            var helperCtor = typeof(HelperClass).GetPublicInstanceConstructors().First();
            var helper = Expression.New(helperCtor, defaultInt, defaultInt, defaultInt);
            var intsMethod = helper.Type.GetPublicInstanceMethod(nameof(HelperClass.GiveMeSomeInts));
            var methodCall = Expression.Call(helper, intsMethod, defaultInt, valueOrDefaultBlock, defaultInt);

            var translated = ToReadableString(methodCall);

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
            var translated = ToReadableString(default(Expression));

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

            var longChainblock = Expression.Block(longCallChain.Body, longCallChain.Body);

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

            var translated = ToReadableString(longChainblock);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldLeaveABlankLineBeforeAnIfStatement()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var zero = Expression.Constant(0);
            var intVariableEqualsZero = Expression.Equal(intVariable, zero);
            var doNothing = Expression.Default(typeof(void));
            var ifIntEqualsZeroDoNothing = Expression.IfThen(intVariableEqualsZero, doNothing);

            var block = Expression.Block(new[] { intVariable }, ifIntEqualsZeroDoNothing);

            const string EXPECTED = @"
int i;

if (i == 0)
{
}";

            var translated = ToReadableString(block);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotLeaveDoubleBlankLinesBetweenIfStatements()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
            var intVariableEqualsOne = Expression.Equal(intVariable, one);
            var doNothing = Expression.Default(typeof(void));
            var ifIntEqualsOneDoNothing = Expression.IfThen(intVariableEqualsOne, doNothing);

            var block = Expression.Block(
                new[] { intVariable },
                ifIntEqualsOneDoNothing,
                ifIntEqualsOneDoNothing);

            const string EXPECTED = @"
int i;

if (i == 1)
{
}

if (i == 1)
{
}";

            var translated = ToReadableString(block);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotLeaveDoubleBlankLinesBetweenInitAndIfStatements()
        {
            var writeWat = CreateLambda(() => Console.WriteLine("Wat"));
            var read = CreateLambda<long>(() => Console.Read());

            var newMemoryStream = Expression.New(typeof(MemoryStream));
            var positionProperty = newMemoryStream.Type.GetProperty("Position");
            var valueBlock = Expression.Block(writeWat.Body, read.Body);
            // ReSharper disable once AssignNullToNotNullAttribute
            var positionInit = Expression.Bind(positionProperty, valueBlock);
            var memoryStreamInit = Expression.MemberInit(newMemoryStream, positionInit);

            var intVariable = Expression.Variable(typeof(int), "i");
            var one = Expression.Constant(1);
            var intVariableEqualsOne = Expression.Equal(intVariable, one);
            var doNothing = Expression.Default(typeof(void));
            var ifIntEqualsOneDoNothing = Expression.IfThen(intVariableEqualsOne, doNothing);

            var block = Expression.Block(memoryStreamInit, ifIntEqualsOneDoNothing);

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

            var translated = ToReadableString(block);

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateMultilineBlockSingleMethodArguments()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var variableInit = Expression.Assign(intVariable, Expression.Constant(3));
            var variableMultiplyFive = Expression.Multiply(intVariable, Expression.Constant(5));
            var variableAdditionOne = Expression.Assign(intVariable, variableMultiplyFive);
            var variableDivideThree = Expression.Divide(intVariable, Expression.Constant(3));
            var variableAdditionTwo = Expression.Assign(intVariable, variableDivideThree);

            var argumentBlock = Expression.Block(
                new[] { intVariable },
                variableInit,
                variableAdditionOne,
                variableAdditionTwo,
                intVariable);

            var catchBlock = Expression.Catch(
                typeof(Exception),
                Expression.Block(ReadableExpression.Comment("So what!"), Expression.Constant(0)));

            var tryCatch = Expression.TryCatch(argumentBlock, catchBlock);

            var collectionVariable = Expression.Variable(typeof(ICollection<int>), "ints");
            var addMethod = collectionVariable.Type.GetPublicInstanceMethod("Add");
            var addMethodCall = Expression.Call(collectionVariable, addMethod, tryCatch);

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

            var translated = ToReadableString(addMethodCall);

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

    internal class ExtensionExpression : Expression
    {
        public ExtensionExpression(Type type = null)
        {
            Type = type ?? typeof(object);
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // The default implementation of VisitChildren falls over 
            // if the Expression is not reducible. Short-circuit that 
            // with this:
            return this;
        }

        public override string ToString() => "Exteeennndddiiiinnngg";
    }

    internal class UnknownExpression : Expression
    {
        public override ExpressionType NodeType => (ExpressionType)5346372;

        public override Type Type => typeof(void);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // See CommentExpression for why this is necessary:
            return this;
        }

        public override string ToString()
        {
            return "You can't know me!";
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
