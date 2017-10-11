namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenFormattingCode
    {
        [TestMethod]
        public void ShouldSplitLongConstructorArgumentListsOntoMultipleLines()
        {
            var helperVariable = Expression.Variable(typeof(HelperClass), "helper");
            var helperConstructor = helperVariable.Type.GetConstructors().First();
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var newHelper = Expression.New(helperConstructor, longVariable, longVariable, longVariable);
            var helperAssignment = Expression.Assign(helperVariable, newHelper);

            var longArgumentListBlock = Expression.Block(new[] { helperVariable }, helperAssignment);

            var translated = longArgumentListBlock.ToReadableString();

            const string EXPECTED = @"
var helper = new HelperClass(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed);";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldSplitMultipleArgumentListsOntoMultipleLines()
        {
            var intsMethod = typeof(HelperClass)
                .GetMethod("GiveMeFourInts", BindingFlags.Public | BindingFlags.Instance);

            var helperVariable = Expression.Variable(typeof(HelperClass), "helper");
            var intVariable = Expression.Variable(typeof(int), "intVariable");

            var intsMethodCall = Expression.Call(
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

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldSplitLongArgumentListsOntoMultipleLines()
        {
            var intsMethod = typeof(HelperClass)
                .GetMethod("GiveMeSomeInts", BindingFlags.Public | BindingFlags.Instance);

            var helperVariable = Expression.Variable(typeof(HelperClass), "helper");
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var intsMethodCall = Expression.Call(helperVariable, intsMethod, longVariable, longVariable, longVariable);

            var translated = intsMethodCall.ToReadableString();

            const string EXPECTED = @"
helper.GiveMeSomeInts(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed)";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldSplitLongInvokeArgumentListsOntoMultipleLines()
        {
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsAction = Expression.Variable(typeof(Action<int, int, int>), "threeIntsAction");
            var threeIntsCall = Expression.Invoke(threeIntsAction, longVariable, longVariable, longVariable);

            var longArgumentListBlock = Expression.Block(new[] { longVariable, threeIntsAction }, threeIntsCall);

            var translated = longArgumentListBlock.ToReadableString();

            const string EXPECTED = @"
int thisVariableReallyHasAVeryLongNameIndeed;
Action<int, int, int> threeIntsAction;
threeIntsAction.Invoke(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed);";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldSplitLongTernariesOntoMultipleLines()
        {
            Expression<Func<int, int>> longTernary =
                veryLongNamedVariable => veryLongNamedVariable > 10
                    ? veryLongNamedVariable * veryLongNamedVariable
                    : veryLongNamedVariable * veryLongNamedVariable * veryLongNamedVariable;

            var translated = longTernary.ToReadableString();

            const string EXPECTED = @"
veryLongNamedVariable => (veryLongNamedVariable > 10)
    ? veryLongNamedVariable * veryLongNamedVariable
    : (veryLongNamedVariable * veryLongNamedVariable) * veryLongNamedVariable";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldSplitLongNestedTernariesOntoMultipleLines()
        {
            Expression<Func<int, int>> longTernary =
                veryLongNamedVariable => (veryLongNamedVariable > 10)
                    ? (veryLongNamedVariable > 100)
                        ? veryLongNamedVariable * veryLongNamedVariable
                        : veryLongNamedVariable - veryLongNamedVariable
                    : veryLongNamedVariable * veryLongNamedVariable + veryLongNamedVariable;

            var translated = longTernary.ToReadableString();

            const string EXPECTED = @"
veryLongNamedVariable => (veryLongNamedVariable > 10)
    ? (veryLongNamedVariable > 100)
        ? veryLongNamedVariable * veryLongNamedVariable
        : veryLongNamedVariable - veryLongNamedVariable
    : (veryLongNamedVariable * veryLongNamedVariable) + veryLongNamedVariable";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldSplitLongTernaryOptionsOntoMultipleLines()
        {
            var oneEqualsTwo = Expression.Equal(Expression.Constant(1), Expression.Constant(2));

            var defaultInt = Expression.Default(typeof(int));

            var threeIntsFunc = Expression.Variable(typeof(Func<int, int, int, int>), "threeIntsFunc");
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsCall = Expression.Invoke(threeIntsFunc, longVariable, longVariable, longVariable);

            var ternary = Expression.Condition(oneEqualsTwo, defaultInt, threeIntsCall);

            var translated = ternary.ToReadableString();

            const string EXPECTED = @"
(1 == 2)
    ? default(int)
    : threeIntsFunc.Invoke(
        thisVariableReallyHasAVeryLongNameIndeed,
        thisVariableReallyHasAVeryLongNameIndeed,
        thisVariableReallyHasAVeryLongNameIndeed)";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldSplitLongAssignmentsOntoMultipleLines()
        {
            var intVariable = Expression.Variable(typeof(int), "value");
            var threeIntsFunc = Expression.Variable(typeof(Func<int, int, int, int>), "threeIntsFunc");
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsSubCall = Expression.Invoke(threeIntsFunc, Expression.Constant(10), Expression.Constant(1), longVariable);
            var threeIntsCall = Expression.Invoke(threeIntsFunc, longVariable, threeIntsSubCall, longVariable);

            var assignment = Expression.Assign(intVariable, threeIntsCall);

            var translated = assignment.ToReadableString();

            const string EXPECTED = @"
value = threeIntsFunc.Invoke(
    thisVariableReallyHasAVeryLongNameIndeed,
    threeIntsFunc.Invoke(10, 1, thisVariableReallyHasAVeryLongNameIndeed),
    thisVariableReallyHasAVeryLongNameIndeed)";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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

            var translated = block.ToReadableString();

            const string EXPECTED = @"
string name;
Func<string> getName = () => name;
name = ""Fred"";

return getName.Invoke();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldNotVarAssignAVariableOfNonImpliedType()
        {
            var intsVariable = Expression.Variable(typeof(IEnumerable<int>), "ints");
            var newArray = Expression.NewArrayBounds(typeof(int), Expression.Constant(2));
            var assignment = Expression.Assign(intsVariable, newArray);

            var block = Expression.Block(new[] { intsVariable }, assignment);

            var translated = block.ToReadableString();

            Assert.AreEqual("IEnumerable<int> ints = new int[2];", translated);
        }

        [TestMethod]
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

            var translated = assignBlock.ToReadableString();

            Assert.AreEqual("ICollection<int?> result = (i == 1) ? new int?[0] : new List<int?>();", translated);
        }

        [TestMethod]
        public void ShouldNotVarAssignAnOuterBlockDeclaredVariable()
        {
            var nameVariable = Expression.Variable(typeof(string), "name");
            var writeNameTwiceVariable = Expression.Variable(typeof(Action), "writeNameTwice");
            Expression<Action> writeLine = () => Console.WriteLine(default(string));
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

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldNotVarAssignAVariableAssignedInATryButUsedInACatch()
        {
            Expression<Func<int, Exception>> exceptionFactory = number => new Exception(number.ToString());
            var intVariable = exceptionFactory.Parameters.First();
            var newException = exceptionFactory.Body;

            var assignment = Expression.Assign(intVariable, Expression.Constant(10));
            var assignmentBlock = Expression.Block(assignment, Expression.Default(typeof(void)));

            var catchBlock = Expression.Catch(typeof(Exception), Expression.Throw(newException));
            var tryCatch = Expression.TryCatch(assignmentBlock, catchBlock);
            var tryCatchBlock = Expression.Block(new[] { intVariable }, tryCatch);

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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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
        }
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
        }
    }

    return 0L;
}
catch
{
    return -1L;
}";

            var translated = overallTryCatch.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldNotIndentParamsArrayArguments()
        {
            Expression<Func<string>> stringJoiner = () =>
                string.Join(",", "[", "i", "]", "[", "j", "]", "[", "k", "]");

            const string EXPECTED = @"
string.Join(
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

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAnExtensionExpressionType()
        {
            var extension = new ExtensionExpression();
            var translated = extension.ToReadableString();

            Assert.AreEqual(extension.ToString(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAnUnknownExpressionType()
        {
            var unknown = new UnknownExpression();
            var translated = unknown.ToReadableString();

            Assert.AreEqual(unknown.ToString(), translated);
        }

        [TestMethod]
        public void ShouldOnlyRemoveParenthesesIfNecessary()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intVariableIsOne = Expression.Equal(intVariable, Expression.Constant(1));

            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectCastToInt = Expression.Convert(objectVariable, typeof(int));
            var intToStringMethod = typeof(object).GetMethod("ToString");
            var intToStringCall = Expression.Call(objectCastToInt, intToStringMethod);

            Expression<Func<string>> emptyString = () => string.Empty;

            var toStringOrEmptyString = Expression.Condition(
                intVariableIsOne,
                emptyString.Body,
                intToStringCall);

            var translated = toStringOrEmptyString.ToReadableString();

            Assert.AreEqual("(i == 1) ? string.Empty : ((int)o).ToString()", translated);
        }

        [TestMethod]
        public void ShouldNotRemoveParenthesesFromACastObjectChainedMethodCall()
        {
            Expression<Func<IList<int>, string[]>> intArrayConverter =
                ints => ((int[])ints).ToString().Split(',');

            var stringArrayVariable = Expression.Variable(typeof(string[]), "strings");
            var assignment = Expression.Assign(stringArrayVariable, intArrayConverter.Body);

            var translated = assignment.ToReadableString();

            Assert.AreEqual("strings = ((int[])ints).ToString().Split(',')", translated);
        }

        [TestMethod]
        public void ShouldNotRemoveParenthesesFromMultiParameterLambdaArguments()
        {
            Expression<Func<IEnumerable<string>, string[]>> stringsConverter =
                strings => strings.Select((str, i) => string.Join(i + ": ", str)).ToArray();

            const string EXPECTED = "strings.Select((str, i) => string.Join(i + \": \", str)).ToArray()";
            var translated = stringsConverter.Body.ToReadableString();

            Assert.AreEqual(EXPECTED, translated);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/9
        [TestMethod]
        public void ShouldNotRemoveParenthesesFromALambdaInvokeResultAssignment()
        {
            Expression<Func<int, int, int>> intsAdder = (a, b) => a + b;
            var one = Expression.Constant(1);
            var two = Expression.Constant(2);
            var lambdaInvocation = Expression.Invoke(intsAdder, one, two);
            var result = Expression.Variable(typeof(int), "result");
            var assignInvokeResult = Expression.Assign(result, lambdaInvocation);

            const string EXPECTED = "result = ((a, b) => a + b).Invoke(1, 2)";
            var translated = assignInvokeResult.ToReadableString();

            Assert.AreEqual(EXPECTED, translated);
        }

        [TestMethod]
        public void ShouldUseMethodGroupsForStaticMethods()
        {
            Expression<Func<IEnumerable<TimeSpan>>> selectTimeSpans =
                () => new[] { 1d, 2d, 3d }.Select(TimeSpan.FromDays);

            var translated = selectTimeSpans.Body.ToReadableString();

            Assert.AreEqual("new[] { 1d, 2d, 3d }.Select(TimeSpan.FromDays)", translated);
        }

        [TestMethod]
        public void ShouldUseMethodGroupsForInstanceMethods()
        {
            Expression<Func<IntConverter, IEnumerable<string>>> selectStrings =
                converter => new[] { 1, 2, 3 }.Select(converter.Convert);

            var translated = selectStrings.Body.ToReadableString();

            Assert.AreEqual("new[] { 1, 2, 3 }.Select(converter.Convert)", translated);
        }

        [TestMethod]
        public void ShouldUseMethodGroupsForLocalFuncs()
        {
            Func<int, string> intConverter = i => i.ToString();

            Expression<Func<IEnumerable<string>>> selectStrings =
                () => new[] { 1, 2, 3 }.Select(intConverter);

            var translated = selectStrings.Body.ToReadableString();

            Assert.AreEqual("new[] { 1, 2, 3 }.Select(intConverter)", translated);
        }

        [TestMethod]
        public void ShouldConvertAnExtensionMethodArgumentToAMethodGroup()
        {
            Expression<Func<List<int>, IEnumerable<int>, bool>> allItemsContained =
                (list, items) => list.All(i => items.Contains(i));

            var translated = allItemsContained.Body.ToReadableString();

            Assert.AreEqual("list.All(items.Contains)", translated);
        }

        [TestMethod]
        public void ShouldConvertAStaticMethodArgumentToAMethodGroup()
        {
            Expression<Func<List<double>, IEnumerable<TimeSpan>>> parseTimeSpans =
                list => list.Select(i => TimeSpan.FromDays(i));

            var translated = parseTimeSpans.Body.ToReadableString();

            Assert.AreEqual("list.Select(TimeSpan.FromDays)", translated);
        }

        [TestMethod]
        public void ShouldConvertAnInstanceMethodArgumentToAMethodGroup()
        {
            Expression<Action<List<int>, ICollection<int>>> copy =
                (list, items) => list.ForEach(i => items.Add(i));

            var translated = copy.Body.ToReadableString();

            Assert.AreEqual("list.ForEach(items.Add)", translated);
        }

        [TestMethod]
        public void ShouldConvertADelegateMethodArgumentToAMethodGroup()
        {
            Func<IntEvaluator, int, bool> evaluatorInvoker = (evaluator, i) => evaluator.Invoke(i);

            Expression<Func<List<int>, int, bool>> listContainsEvaluator =
                (list, i) => evaluatorInvoker.Invoke(x => list.Contains(x), i);

            var translated = listContainsEvaluator.Body.ToReadableString();

            Assert.AreEqual("evaluatorInvoker.Invoke(list.Contains, i)", translated);
        }

        [TestMethod]
        public void ShouldNotConvertAModifyingArgumentToAMethodGroup()
        {
            Expression<Action<List<int>, ICollection<string>>> copy =
                (list, items) => list.ForEach(i => items.Add(i.ToString()));

            var translated = copy.Body.ToReadableString();

            Assert.AreEqual("list.ForEach(i => items.Add(i.ToString()))", translated);
        }

        [TestMethod]
        public void ShouldNotConvertAModifyingReturnTypeToAMethodGroup()
        {
            Func<IntEvaluatorNullable, int, bool?> evaluatorInvoker = (evaluator, i) => evaluator.Invoke(i);

            Expression<Func<List<int>, int, bool?>> listContainsEvaluator =
                (list, i) => evaluatorInvoker.Invoke(x => list.Contains(x), i);

            var translated = listContainsEvaluator.Body.ToReadableString();

            Assert.AreEqual("evaluatorInvoker.Invoke(x => (bool?)list.Contains(x), i)", translated);
        }

        [TestMethod]
        public void ShouldSplitLongChainedMethodsOntoMultipleLines()
        {
            Expression<Func<IEnumerable<int>>> longMethodChain = () =>
                Enumerable
                    .Range(1, 10)
                    .Select(Convert.ToInt64)
                    .ToArray()
                    .Select(Convert.ToInt32)
                    .OrderByDescending(i => i)
                    .ToList();

            var translated = longMethodChain.Body.ToReadableString();

            const string EXPECTED = @"
Enumerable
    .Range(1, 10)
    .Select(Convert.ToInt64)
    .ToArray()
    .Select(Convert.ToInt32)
    .OrderByDescending(i => i)
    .ToList()";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateNullToNull()
        {
            var translated = default(Expression).ToReadableString();

            Assert.IsNull(translated);
        }

        [TestMethod]
        public void ShouldLeaveABlankLineAfterAMultipleLineExpression()
        {
            Expression<Func<List<int>, IEnumerable<int>>> longCallChain = list => list
                .Select(i => i * 2)
                .Select(i => i * 3)
                .Select(i => i * 4)
                .ToArray();

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

            var translated = longChainblock.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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

            var translated = block.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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

            var translated = block.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldNotLeaveDoubleBlankLinesBetweenInitAndIfStatements()
        {
            Expression<Action> writeWat = () => Console.WriteLine("Wat");
            Expression<Func<long>> read = () => Console.Read();

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

        return ((long)Console.Read());
    }
};

if (i == 1)
{
}";

            var translated = block.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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
            var addMethod = collectionVariable.Type.GetMethod("Add");
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

            var translated = addMethodCall.ToReadableString();

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
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
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // See CommentExpression for why this is necessary:
            return this;
        }

        public override string ToString()
        {
            return "Exteeennndddiiiinnngg";
        }
    }

    internal class UnknownExpression : Expression
    {
        public override ExpressionType NodeType => (ExpressionType)5346372;

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
