namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingLoopExpressions
    {
        [TestMethod]
        public void ShouldTranslateAnInfiniteWhileLoop()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            var loop = Expression.Loop(writeLine.Body);

            var translated = loop.ToReadableString();

            const string EXPECTED = @"
while (true)
{
    Console.WriteLine();
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
