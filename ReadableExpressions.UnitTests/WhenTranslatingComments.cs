namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Xunit;

    public class WhenTranslatingComments
    {
        [Fact]
        public void ShouldTranslateASingleLineComment()
        {
            var comment = ReadableExpression.Comment("Not worth commenting on");

            var translated = comment.ToReadableString();

            Assert.Equal("// Not worth commenting on", translated);
        }

        [Fact]
        public void ShouldTranslateAMultipleLineComment()
        {
            var comment = ReadableExpression.Comment(@"
Not worth commenting on
but I will anyway");

            var translated = comment.ToReadableString();

            const string EXPECTED = @"
// Not worth commenting on
// but I will anyway";

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateABlockWithAComment()
        {
            var comment = ReadableExpression.Comment("Anyone listening?");
            Expression<Action> beep = () => Console.Beep();

            var commentedBeep = Expression.Block(comment, beep.Body);

            var translated = commentedBeep.ToReadableString();

            const string EXPECTED = @"
// Anyone listening?
Console.Beep();";

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateAConditionalBranchWithAComment()
        {
            var comment = ReadableExpression.Comment("Maths works");
            var one = Expression.Constant(1);
            var oneEqualsOne = Expression.Equal(one, one);
            var ifOneEqualsOneComment = Expression.IfThen(oneEqualsOne, comment);

            var translated = ifOneEqualsOneComment.ToReadableString();

            const string EXPECTED = @"
if (1 == 1)
{
    // Maths works
}";
            Assert.Equal(EXPECTED.TrimStart(), translated);
        }
    }
}
