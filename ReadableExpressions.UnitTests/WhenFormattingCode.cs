namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenFormattingCode
    {
        [TestMethod]
        public void ShouldSplitLongArgumentListsOntoMultipleLines()
        {
            var longVariable = Expression.Variable(typeof(int), "thisVariableReallyHasAVeryLongNameIndeed");
            var threeIntsAction = Expression.Variable(typeof(Action<int, int, int>), "threeIntsAction");
            var threeIntsCall = Expression.Invoke(threeIntsAction, longVariable, longVariable, longVariable);

            var longArgumentListBlock = Expression.Block(new[] { longVariable, threeIntsAction }, threeIntsCall);

            var translated = longArgumentListBlock.ToReadableString();

            var expected = $@"
Int32 {longVariable.Name};
Action<Int32, Int32, Int32> threeIntsAction;
threeIntsAction.Invoke(
    {longVariable.Name},
    {longVariable.Name},
    {longVariable.Name});";

            Assert.AreEqual(expected.TrimStart(), translated);
        }
    }
}
