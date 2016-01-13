namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingAssignments
    {
        [TestMethod]
        public void ShouldTranslateAVariableAssignment()
        {
            var helloVariable = Expression.Variable(typeof(string), "hello");
            var worldString = Expression.Constant("World!", typeof(string));
            var helloWorldAssignment = Expression.Assign(helloVariable, worldString);

            var translated = helloWorldAssignment.ToReadableString();

            Assert.AreEqual("var hello = \"World!\"", translated);
        }
    }
}
