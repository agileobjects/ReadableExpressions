namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingDebugInfo
    {
        [TestMethod]
        public void ShouldTranslateDebugInfo()
        {
            var tempFileName = Path.GetTempFileName();
            var debugInfoFile = Expression.SymbolDocument(tempFileName);
            var debugInfo = Expression.DebugInfo(debugInfoFile, 1, 1, 2, 100);
            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            var debuggedBlock = Expression.Block(debugInfo, writeHello.Body);

            var translated = debuggedBlock.ToReadableString();

            var expected = $@"
// Debug to {tempFileName}, 1, 1 -> 2, 100
Console.WriteLine(""Hello"");";

            Assert.AreEqual(expected.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateClearDebugInfo()
        {
            var tempFileName = Path.GetTempFileName();
            var debugInfoFile = Expression.SymbolDocument(tempFileName);
            var clearDebugInfo = Expression.ClearDebugInfo(debugInfoFile);
            Expression<Action> writeHello = () => Console.WriteLine("Hello");
            var debuggedBlock = Expression.Block(writeHello.Body, clearDebugInfo);

            var translated = debuggedBlock.ToReadableString();

            var expected = $@"
Console.WriteLine(""Hello"");
// Clear debug info from {tempFileName}";

            Assert.AreEqual(expected.TrimStart(), translated);
        }
    }
}
