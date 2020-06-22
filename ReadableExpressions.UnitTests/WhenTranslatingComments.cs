namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

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
        public void ShouldUseATranslatedCommentInToString()
        {
            var comment = ReadableExpression.Comment("ToString me");

            var translated = comment.ToString();

            translated.ShouldBe("// ToString me");
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

            var commentedBeep = Block(comment, beep.Body);

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
            var one = Constant(1);
            var oneEqualsOne = Equal(one, one);
            var ifOneEqualsOneComment = IfThen(oneEqualsOne, comment);

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
