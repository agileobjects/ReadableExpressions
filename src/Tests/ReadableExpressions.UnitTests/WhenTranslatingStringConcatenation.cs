namespace AgileObjects.ReadableExpressions.UnitTests;

using Common;
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

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str1 + str2");
    }

    [Fact]
    public void ShouldTranslateAThreeArgumentConcatenation()
    {
        var concat = CreateLambda((string str1, string str2, string str3) => str1 + str2 + str3);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str1 + str2 + str3");
    }

    [Fact]
    public void ShouldTranslateAMixedTypeTwoArgumentConcatenation()
    {
        var concat = CreateLambda((string str1, int i) => i + str1);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("i + str1");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/123
    [Fact]
    public void ShouldTranslateAnObjectToStringIntConcatenation()
    {
        var concat = CreateLambda((object obj, int i)
            => obj.ToString() + i);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("obj.ToString() + i");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/123
    [Fact]
    public void ShouldTranslateAnObjectToStringIntToStringConcatenation()
    {
        var concat = CreateLambda((object obj, int i)
            => obj.ToString() + i.ToString());

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("obj.ToString() + i");
    }

    [Fact]
    public void ShouldTranslateAnObjectCharacterToStringConcatenation()
    {
        var concat = CreateLambda((object obj, char c)
            => obj + c.ToString());

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("obj + c.ToString()");
    }

    [Fact]
    public void ShouldTranslateAnIntStringCharacterConcatenation()
    {
        var concat = CreateLambda((int i, string str, char c)
            => i + str + c);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("i + str + c");
    }

    [Fact]
    public void ShouldTranslateAStringCharacterLongConcatenation()
    {
        var concat = CreateLambda((string str, char c, long l)
            => str + c + l);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str + c + l");
    }

    [Fact]
    public void ShouldTranslateAStringDecimalCharacterConcatenation()
    {
        var concat = CreateLambda((string str, decimal d, char c)
            => str + d + c);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str + d + c");
    }

    [Fact]
    public void ShouldTranslateAnIntIntIntStringConcatenation()
    {
        var concat = CreateLambda((int i1, int i2, int i3, string str)
            => i1 + i2 + i3 + str);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("i1 + i2 + i3 + str");
    }

    [Fact]
    public void ShouldTranslateAnIntIntToStringIntStringConcatenation()
    {
        var concat = CreateLambda((int i1, int i2, int i3, string str)
            => i1 + i2.ToString() + i3 + str);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("i1 + i2.ToString() + i3 + str");
    }

    [Fact]
    public void ShouldTranslateAnIntCharacterIntToStringIntConcatenation()
    {
        var concat = CreateLambda((int i1, char c, int i2, int i3)
            => i1 + c + i2.ToString() + i3);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("i1 + c + i2.ToString() + i3");
    }

    [Fact]
    public void ShouldRemoveARedundantParameterlessToStringCall()
    {
        var concat = CreateLambda((string str1, int i) => i.ToString() + str1);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("i + str1");
    }

    [Fact]
    public void ShouldTranslateAnExplicitTwoStringsConcatenation()
    {
        var concat = CreateLambda((string str1, string str2)
            => string.Concat(str1, str2));

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str1 + str2");
    }

    [Fact]
    public void ShouldTranslateAnExplicitThreeStringsConcatenation()
    {
        var concat = CreateLambda((string str1, string str2, string str3)
            => string.Concat(str1, str2, str3));

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str1 + str2 + str3");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/89
    [Fact]
    public void ShouldTranslateAnExplicitParamsArgumentConcatenation()
    {
        var concat = CreateLambda(() => string.Concat("1", "2", "3", "4", "5"));

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("\"1\" + \"2\" + \"3\" + \"4\" + \"5\"");
    }

    [Fact]
    public void ShouldTranslateAnExplicitThreeObjectsConcatenation()
    {
        var concat = CreateLambda((string str1, int i, long l)
            => string.Concat(str1, i, l));

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str1 + i + l");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/12
    [Fact]
    public void ShouldMaintainTernaryOperandParentheses()
    {
        var ternaryResultAdder = CreateLambda((bool condition, string ifTrue, string ifFalse)
            => (condition ? ifTrue : ifFalse) + "Hello!");

        var translated = ternaryResultAdder.Body.ToReadableString();

        translated.ShouldBe("(condition ? ifTrue : ifFalse) + \"Hello!\"");
    }

    [Fact]
    public void ShouldMaintainNumericOperandParentheses()
    {
        var mathResultAdder = CreateLambda((int i, int j, int k)
            => (i - j) / k + " Maths!");

        var translated = mathResultAdder.Body.ToReadableString();

        translated.ShouldBe("(i - j) / k + \" Maths!\"");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/120
    [Fact]
    public void ShouldMaintainNestedConcatenationParentheses()
    {
        var lambda = CreateLambda((string str1, string str2)
            => (str1 + str2).StartsWith("What?!"));

        var translated = lambda.Body.ToReadableString();

        translated.ShouldBe("(str1 + str2).StartsWith(\"What?!\")");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/43
    [Fact]
    public void ShouldHandleANullTerminatingCharacter()
    {
        var concat = CreateLambda((string str1, string str2) => str1 + '\0' + str2);

        var translated = concat.Body.ToReadableString();

        translated.ShouldBe("str1 + '\\0' + str2");
    }
}