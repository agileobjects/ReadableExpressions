namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingStringConcatenation
    {
        [TestMethod]
        public void ShouldTranslateATwoArgumentConcatenation()
        {
            Expression<Func<string, string, string>> concat = (str1, str2) => str1 + str2;

            var translated = concat.Body.ToReadableString();

            Assert.AreEqual("str1 + str2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAThreeArgumentConcatenation()
        {
            Expression<Func<string, string, string, string>> concat = (str1, str2, str3) => str1 + str2 + str3;

            var translated = concat.Body.ToReadableString();

            Assert.AreEqual("str1 + str2 + str3", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMixedTypeTwoArgumentConcatenation()
        {
            Expression<Func<string, int, string>> concat = (str1, i) => i + str1;

            var translated = concat.Body.ToReadableString();

            Assert.AreEqual("i + str1", translated);
        }

        [TestMethod]
        public void ShouldExcludeAnExplictParameterlessToStringCall()
        {
            Expression<Func<string, int, string>> concat = (str1, i) => i.ToString() + str1;

            var translated = concat.Body.ToReadableString();

            Assert.AreEqual("i + str1", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnExplicitTwoArgumentConcatenation()
        {
            Expression<Func<string, string, string>> concat = (str1, str2) => string.Concat(str1, str2);

            var translated = concat.Body.ToReadableString();

            Assert.AreEqual("str1 + str2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnExplicitThreeArgumentConcatenation()
        {
            Expression<Func<string, string, string, string>> concat =
                (str1, str2, str3) => string.Concat(str1, str2, str3);

            var translated = concat.Body.ToReadableString();

            Assert.AreEqual("str1 + str2 + str3", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnExplicitMixedTypeThreeArgumentConcatenation()
        {
            Expression<Func<string, int, long, string>> concat = (str1, i, l) => string.Concat(str1, i, l);

            var translated = concat.Body.ToReadableString();

            Assert.AreEqual("str1 + i + l", translated);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/12
        [TestMethod]
        public void ShouldMaintainTernaryOperandParentheses()
        {
            Expression<Func<bool, string, string, string>> ternaryResultAdder =
                (condition, ifTrue, ifFalse) => (condition ? ifTrue : ifFalse) + "Hello!";

            var translated = ternaryResultAdder.Body.ToReadableString();

            Assert.AreEqual("(condition ? ifTrue : ifFalse) + \"Hello!\"", translated);
        }

        [TestMethod]
        public void ShouldMaintainNumericOperandParentheses()
        {
            Expression<Func<int, int, int, string>> mathResultAdder =
                (i, j, k) => ((i - j) / k) + " Maths!";

            var translated = mathResultAdder.Body.ToReadableString();

            Assert.AreEqual("((i - j) / k) + \" Maths!\"", translated);
        }
    }
}
