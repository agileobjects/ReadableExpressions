namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
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
            var assignHelper = Expression.Assign(helperVariable, newHelper);

            var longArgumentListBlock = Expression.Block(new[] { helperVariable }, assignHelper);

            var translated = longArgumentListBlock.ToReadableString();

            const string EXPECTED = @"
var helper = new HelperClass(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed);";

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
            var intToStringMethod = typeof(int).GetMethods().First(m => m.Name == "ToString");
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
        public void ShouldUseFriendlyNamesForArrays()
        {
            var intArrayVariable = Expression.Variable(typeof(int[]), "ints");
            var assignNull = Expression.Assign(intArrayVariable, Expression.Default(intArrayVariable.Type));
            var assignNullBlock = Expression.Block(new[] { intArrayVariable }, assignNull);

            var translated = assignNullBlock.ToReadableString();

            Assert.AreEqual("var ints = default(int[]);", translated);
        }

        [TestMethod]
        public void ShouldUseFriendlyNamesForCharacters()
        {
            Expression<Func<char, double>> characterToNumeric = c => char.GetNumericValue(c);

            var translated = characterToNumeric.ToReadableString();

            Assert.AreEqual("c => char.GetNumericValue(c)", translated);
        }

        #region Helper Classes

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        private class HelperClass
        {
            public HelperClass(int intOne, int intTwo, int intThree)
            {

            }

            public void GiveMeSomeInts(int intOne, int intTwo, int intThree)
            {
            }
        }
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Local

        private delegate bool IntEvaluator(int value);

        private delegate bool? IntEvaluatorNullable(int value);

        // ReSharper disable once ClassNeverInstantiated.Local
        private class IntConverter
        {
            public string Convert(int value)
            {
                return value.ToString();
            }
        }

        private class ExtensionExpression : Expression
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

        private class UnknownExpression : Expression
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

        #endregion
    }
}
