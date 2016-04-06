namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Translators;

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
getName.Invoke();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldNotVarAssignADeclaredVariable()
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
                writeNameTwiceAssignment,
                nameAssignment,
                writeNameTwiceCall);

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

        #region Helper Class

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

        #endregion
    }
}
