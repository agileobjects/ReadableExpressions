namespace AgileObjects.ReadableExpressions.UnitTests;

using System;
using Common;
#if !NET35
using Xunit;
using static System.Linq.Expressions.Expression;
#else
using static Microsoft.Scripting.Ast.Expression;
using Fact = NUnit.Framework.TestAttribute;

[NUnit.Framework.TestFixture]
#endif
public class WhenTranslatingLogicalOperations : TestClassBase
{
    [Fact]
    public void ShouldTranslateAnAndOperation()
    {
        var bothBoolsAreTheSame = CreateLambda((bool b1, bool b2) => b1 && b2);

        var translated = bothBoolsAreTheSame.Body.ToReadableString();

        translated.ShouldBe("b1 && b2");
    }

    [Fact]
    public void ShouldTranslateABitwiseAndOperation()
    {
        var bitwiseAnd = CreateLambda((bool b1, bool b2) => b1 & b2);

        var translated = bitwiseAnd.Body.ToReadableString();

        translated.ShouldBe("b1 & b2");
    }

    [Fact]
    public void ShouldTranslateAnOrOperation()
    {
        var eitherBoolsIsTrue = CreateLambda((bool b1, bool b2) => b1 || b2);

        var translated = eitherBoolsIsTrue.Body.ToReadableString();

        translated.ShouldBe("b1 || b2");
    }

    [Fact]
    public void ShouldTranslateABitwiseOrOperation()
    {
        var bitwiseOr = CreateLambda((bool b1, bool b2) => b1 | b2);

        var translated = bitwiseOr.Body.ToReadableString();

        translated.ShouldBe("b1 | b2");
    }

    [Fact]
    public void ShouldTranslateABitwiseExclusiveOrOperation()
    {
        var bitwiseExclusiveOr = CreateLambda((bool b1, bool b2) => b1 ^ b2);

        var translated = bitwiseExclusiveOr.Body.ToReadableString();

        translated.ShouldBe("b1 ^ b2");
    }

    [Fact]
    public void ShouldTranslateABitwiseLeftShiftOperation()
    {
        var bitwiseLeftShift = CreateLambda((int i1, int i2) => i1 << i2);

        var translated = bitwiseLeftShift.Body.ToReadableString();

        translated.ShouldBe("i1 << i2");
    }

    [Fact]
    public void ShouldTranslateABitwiseRightShiftOperation()
    {
        var bitwiseRightShift = CreateLambda((int i1, int i2) => i1 >> i2);

        var translated = bitwiseRightShift.Body.ToReadableString();

        translated.ShouldBe("i1 >> i2");
    }

    [Fact]
    public void ShouldTranslateAUnaryPlusOperation()
    {
        var intVariable = Variable(typeof(int), "i");
        var unaryPlus = UnaryPlus(intVariable);

        var translated = unaryPlus.ToReadableString();

        translated.ShouldBe("+i");
    }

    [Fact]
    public void ShouldTranslateAOnesComplementOperation()
    {
        var intVariable = Variable(typeof(int), "i");
        var onesComplement = OnesComplement(intVariable);

        var translated = onesComplement.ToReadableString();

        translated.ShouldBe("~i");
    }

    [Fact]
    public void ShouldTranslateACoalesceOperation()
    {
        var oneOrTwo = CreateLambda((bool? b1, bool b2) => b1 ?? b2);

        var translated = oneOrTwo.Body.ToReadableString();

        translated.ShouldBe("b1 ?? b2");
    }

    [Fact]
    public void ShouldTranslateAConditionalOperation()
    {
        var whatSize = CreateLambda((int i) => (i < 8) ? "Too small" : "Too big");

        var translated = whatSize.Body.ToReadableString();

        translated.ShouldBe("(i < 8) ? \"Too small\" : \"Too big\"");
    }

    [Fact]
    public void ShouldTranslateAValueTypeTypeEqualExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var intIsLong = TypeEqual(intVariable, typeof(long));

        var translated = intIsLong.ToReadableString();

        translated.ShouldBe("false");
    }

    [Fact]
    public void ShouldTranslateANullableValueTypeTypeEqualExpression()
    {
        var nullableLongVariable = Variable(typeof(long?), "l");
        var nullableLongIsNullableLong = TypeEqual(nullableLongVariable, typeof(long?));

        var translated = nullableLongIsNullableLong.ToReadableString();

        translated.ShouldBe("l != null");
    }

    [Fact]
    public void ShouldTranslateAConstantTypeEqualExpression()
    {
        var intConstant = Constant(123, typeof(int));
        var intConstantIsInt = TypeEqual(intConstant, typeof(int));

        var translated = intConstantIsInt.ToReadableString();

        translated.ShouldBe("true");
    }

    [Fact]
    public void ShouldTranslateAnObjectTypeEqualExpression()
    {
        var objectVariable = Variable(typeof(object), "o");
        var objectIsString = TypeEqual(objectVariable, typeof(string));

        var translated = objectIsString.ToReadableString();

        translated.ShouldBe("(o != null) && (o.GetType() == typeof(string))");
    }

    [Fact]
    public void ShouldTranslateAnIsTrueExpression()
    {
        var boolVariable = Variable(typeof(bool), "b");
        var boolIsTrue = IsTrue(boolVariable);

        var translated = boolIsTrue.ToReadableString();

        translated.ShouldBe("b");
    }

    [Fact]
    public void ShouldTranslateAnIsFalseExpression()
    {
        var boolVariable = Variable(typeof(bool), "b");
        var boolIsFalse = IsFalse(boolVariable);

        var translated = boolIsFalse.ToReadableString();

        translated.ShouldBe("!b");
    }
}