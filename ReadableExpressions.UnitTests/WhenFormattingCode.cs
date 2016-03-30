namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenFormattingCode
    {
        [TestMethod]
        public void ShouldSplitLongArgumentListsOntoMultipleLines()
        {
            var intsMethod = typeof(WhenFormattingCode)
                .GetMethod("GiveMeSomeInts", BindingFlags.NonPublic | BindingFlags.Static);

            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var intsMethodCall = Expression.Call(intsMethod, longVariable, longVariable, longVariable);

            var longArgumentListBlock = Expression.Block(new[] { longVariable }, intsMethodCall);

            var translated = longArgumentListBlock.ToReadableString();

            const string EXPECTED = @"
int thisVariableReallyHasAVeryLongNameIndeed;
WhenFormattingCode.GiveMeSomeInts(
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed,
    thisVariableReallyHasAVeryLongNameIndeed);";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable UnusedParameter.Local
        private static void GiveMeSomeInts(int intOne, int intTwo, int intThree)
        {
        }
        // ReSharper restore UnusedParameter.Local

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
    }
}
