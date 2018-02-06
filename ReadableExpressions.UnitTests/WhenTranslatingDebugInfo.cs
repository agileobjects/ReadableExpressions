namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using System.Linq.Expressions;
    using Xunit;

    public class WhenTranslatingDebugInfo
    {
        [Fact]
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

            Assert.Equal(expected.TrimStart(), translated);
        }

        [Fact]
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

            Assert.Equal(expected.TrimStart(), translated);
        }
    }
}
