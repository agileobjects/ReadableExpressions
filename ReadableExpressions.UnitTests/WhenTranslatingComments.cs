namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingComments : TestClassBase
    {
        [Fact]
        public void ShouldTranslateASingleLineComment()
        {
            var comment = ReadableExpression.Comment("Not worth commenting on");

            var translated = ToReadableString(comment);

            translated.ShouldBe("// Not worth commenting on");
        }

        [Fact]
        public void ShouldTranslateAMultipleLineComment()
        {
            var comment = ReadableExpression.Comment(@"
Not worth commenting on
but I will anyway");

            var translated = ToReadableString(comment);

            const string EXPECTED = @"
// Not worth commenting on
// but I will anyway";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateABlockWithAComment()
        {
            var comment = ReadableExpression.Comment("Anyone listening?");
            var beep = CreateLambda(() => Console.Beep());

            var commentedBeep = Expression.Block(comment, beep.Body);

            var translated = ToReadableString(commentedBeep);

            const string EXPECTED = @"
// Anyone listening?
Console.Beep();";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAConditionalBranchWithAComment()
        {
            var comment = ReadableExpression.Comment("Maths works");
            var one = Expression.Constant(1);
            var oneEqualsOne = Expression.Equal(one, one);
            var ifOneEqualsOneComment = Expression.IfThen(oneEqualsOne, comment);

            var translated = ToReadableString(ifOneEqualsOneComment);

            const string EXPECTED = @"
if (1 == 1)
{
    // Maths works
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
