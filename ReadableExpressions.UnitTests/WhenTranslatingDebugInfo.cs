namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.IO;
    using Common;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingDebugInfo : TestClassBase
    {
        [Fact]
        public void ShouldTranslateDebugInfo()
        {
            var tempFileName = Path.GetTempFileName();
            var debugInfoFile = SymbolDocument(tempFileName);
            var debugInfo = DebugInfo(debugInfoFile, 1, 1, 2, 100);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var debuggedBlock = Block(debugInfo, writeHello.Body);

            var translated = debuggedBlock.ToReadableString();

            var expected = $@"
// Debug to {tempFileName}, 1, 1 -> 2, 100
Console.WriteLine(""Hello"");";

            translated.ShouldBe(expected.TrimStart());
        }

        [Fact]
        public void ShouldTranslateClearDebugInfo()
        {
            var tempFileName = Path.GetTempFileName();
            var debugInfoFile = SymbolDocument(tempFileName);
            var clearDebugInfo = ClearDebugInfo(debugInfoFile);
            var writeHello = CreateLambda(() => Console.WriteLine("Hello"));
            var debuggedBlock = Block(writeHello.Body, clearDebugInfo);

            var translated = debuggedBlock.ToReadableString();

            var expected = $@"
Console.WriteLine(""Hello"");
// Clear debug info from {tempFileName}";

            translated.ShouldBe(expected.TrimStart());
        }
    }
}
