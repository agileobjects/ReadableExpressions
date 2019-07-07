namespace AgileObjects.ReadableExpressions.UnitTests
{
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingStringConcatenation : TestClassBase
    {
        [Fact]
        public void ShouldTranslateATwoArgumentConcatenation()
        {
            var concat = CreateLambda((string str1, string str2) => str1 + str2);

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("str1 + str2");
        }

        [Fact]
        public void ShouldTranslateAThreeArgumentConcatenation()
        {
            var concat = CreateLambda((string str1, string str2, string str3) => str1 + str2 + str3);

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("str1 + str2 + str3");
        }

        [Fact]
        public void ShouldTranslateAMixedTypeTwoArgumentConcatenation()
        {
            var concat = CreateLambda((string str1, int i) => i + str1);

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("i + str1");
        }

        [Fact]
        public void ShouldExcludeAnExplictParameterlessToStringCall()
        {
            var concat = CreateLambda((string str1, int i) => i.ToString() + str1);

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("i + str1");
        }

        [Fact]
        public void ShouldTranslateAnExplicitTwoArgumentConcatenation()
        {
            var concat = CreateLambda((string str1, string str2) => string.Concat(str1, str2));

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("str1 + str2");
        }

        [Fact]
        public void ShouldTranslateAnExplicitThreeArgumentConcatenation()
        {
            var concat = CreateLambda((string str1, string str2, string str3)
                => string.Concat(str1, str2, str3));

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("str1 + str2 + str3");
        }

        [Fact]
        public void ShouldTranslateAnExplicitMixedTypeThreeArgumentConcatenation()
        {
            var concat = CreateLambda((string str1, int i, long l) => string.Concat(str1, i, l));

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("str1 + i + l");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/12
        [Fact]
        public void ShouldMaintainTernaryOperandParentheses()
        {
            var ternaryResultAdder = CreateLambda((bool condition, string ifTrue, string ifFalse)
                => (condition ? ifTrue : ifFalse) + "Hello!");

            var translated = ToReadableString(ternaryResultAdder.Body);

            translated.ShouldBe("(condition ? ifTrue : ifFalse) + \"Hello!\"");
        }

        [Fact]
        public void ShouldMaintainNumericOperandParentheses()
        {
            var mathResultAdder = CreateLambda((int i, int j, int k) => ((i - j) / k) + " Maths!");

            var translated = ToReadableString(mathResultAdder.Body);

            translated.ShouldBe("((i - j) / k) + \" Maths!\"");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/43
        [Fact]
        public void ShouldChandleANullTerminatingCharacter()
        {
            var concat = CreateLambda((string str1, string str2) => str1 + '\0' + str2);

            var translated = ToReadableString(concat.Body);

            translated.ShouldBe("str1 + '\\0' + str2");
        }
    }
}
