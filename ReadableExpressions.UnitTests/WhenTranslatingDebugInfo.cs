namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingDebugInfo : TestClassBase
    {
        [Fact]
        public void ShouldTranslateDebugInfo()
        {
            var tempFileName = Path.GetTempFileName();
            var debugInfoFile = Expression.SymbolDocument(tempFileName);
            var debugInfo = Expression.DebugInfo(debugInfoFile, 1, 1, 2, 100);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var debuggedBlock = Expression.Block(debugInfo, writeHello.Body);

            var translated = ToReadableString(debuggedBlock);

            var expected = $@"
// Debug to {tempFileName}, 1, 1 -> 2, 100
Console.WriteLine(""Hello"");";

            translated.ShouldBe(expected.TrimStart());
        }

        [Fact]
        public void ShouldTranslateClearDebugInfo()
        {
            var tempFileName = Path.GetTempFileName();
            var debugInfoFile = Expression.SymbolDocument(tempFileName);
            var clearDebugInfo = Expression.ClearDebugInfo(debugInfoFile);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var debuggedBlock = Expression.Block(writeHello.Body, clearDebugInfo);

            var translated = ToReadableString(debuggedBlock);

            var expected = $@"
Console.WriteLine(""Hello"");
// Clear debug info from {tempFileName}";

            translated.ShouldBe(expected.TrimStart());
        }
    }
}
