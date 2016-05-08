namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingComments
    {
        [TestMethod]
        public void ShouldTranslateASingleLineComment()
        {
            var comment = ReadableExpression.Comment("Not worth commenting on");

            var translated = comment.ToReadableString();

            Assert.AreEqual("// Not worth commenting on", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleLineComment()
        {
            var comment = ReadableExpression.Comment(@"
Not worth commenting on
but I will anyway");

            var translated = comment.ToReadableString();

            const string EXPECTED = @"
// Not worth commenting on
// but I will anyway";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateABlockWithAComment()
        {
            var comment = ReadableExpression.Comment("Anyone listening?");
            Expression<Action> beep = () => Console.Beep();

            var commentedBeep = Expression.Block(comment, beep.Body);

            var translated = commentedBeep.ToReadableString();

            const string EXPECTED = @"
// Anyone listening?
Console.Beep();";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
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
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
