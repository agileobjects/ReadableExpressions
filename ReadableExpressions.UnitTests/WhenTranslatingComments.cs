﻿namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using Common;
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

            var translated = comment.ToReadableString();

            translated.ShouldBe("// Not worth commenting on");
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

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateABlockWithAComment()
        {
            var comment = ReadableExpression.Comment("Anyone listening?");
            var beep = CreateLambda(() => Console.Beep());

            var commentedBeep = Block(comment, beep.Body);

            var translated = commentedBeep.ToReadableString();

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

            var translated = ifOneEqualsOneComment.ToReadableString();

            const string EXPECTED = @"
if (1 == 1)
{
    // Maths works
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
