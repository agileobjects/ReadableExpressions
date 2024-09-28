namespace AgileObjects.ReadableExpressions.UnitTests;

using System;
using System.Collections.Generic;
using System.IO;

#if NET35
[NUnitTestFixture]
#endif
public class WhenTranslatingConversions : TestClassBase
{
    [Fact]
    public void ShouldTranslateACastExpression()
    {
        var intToDouble = CreateLambda((int i) => (double)i);

        var translated = intToDouble.ToReadableString();

        translated.ShouldBe("i => (double)i");
    }

    [Fact]
    public void ShouldTranslateACheckedCastExpression()
    {
        var intParameter = Parameter(typeof(int), "i");
        var checkedCast = ConvertChecked(intParameter, typeof(short));

        var checkedCastLambda = Lambda<Func<int, short>>(checkedCast, intParameter);

        var translated = checkedCastLambda.ToReadableString();

        translated.ShouldBe("i => (short)i");
    }

    [Fact]
    public void ShouldTranslateACastToNullableExpression()
    {
        var longToNullable = CreateLambda((long l) => (long?)l);

        var translated = longToNullable.ToReadableString();

        translated.ShouldBe("l => (long?)l");
    }

    [Fact]
    public void ShouldUseParenthesisInCasting()
    {
        var castDateTimeHour = CreateLambda((object o) => ((DateTime)o).Hour);

        var translated = castDateTimeHour.ToReadableString();

        translated.ShouldBe("o => ((DateTime)o).Hour");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/122
    [Fact]
    public void ShouldUseParenthesisInACastMemberAccess()
    {
        var castDictionaryValueAccess = CreateLambda((object o)
            => ((Dictionary<string, string>)o)["key"]);

        var translated =
            castDictionaryValueAccess.Body.ToReadableString();

        translated.ShouldBe("((Dictionary<string, string>)o)[\"key\"]");
    }

    [Fact]
    public void ShouldTranslateANegationExpression()
    {
        var negator = CreateLambda((bool b) => !b);

        var translated = negator.ToReadableString();

        translated.ShouldBe("b => !b");
    }

    [Fact]
    public void ShouldTranslateAnAsCastExpression()
    {
        var streamAsDisposable = CreateLambda((Stream stream) => stream as IDisposable);

        var translated = streamAsDisposable.Body.ToReadableString();

        translated.ShouldBe("stream as IDisposable");
    }

    [Fact]
    public void ShouldTranslateAnIsTypeExpression()
    {
        var objectIsDisposable = CreateLambda((object o) => o is IDisposable);

        var translated = objectIsDisposable.Body.ToReadableString();

        translated.ShouldBe("o is IDisposable");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/142
    [Fact]
    public void ShouldTranslateAnIsTypeExpressionOperand()
    {
        var intValueAndIsTypeChecks =
            CreateLambda((int i, object o) => i == 1 && o is string);

        var translated = intValueAndIsTypeChecks.Body.ToReadableString();

        translated.ShouldBe("(i == 1) && (o is string)");
    }

    [Fact]
    public void ShouldTranslateABoxingExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var boxInt = Convert(intVariable, typeof(object));

        var translated = boxInt.ToReadableString();

        translated.ShouldBe("i");
    }

    [Fact]
    public void ShouldTranslateAnUpcastClassToObjectExpression()
    {
        var streamVariable = Variable(typeof(Stream), "stream");
        var upcastStreamToObject = Convert(streamVariable, typeof(object));

        var translated = upcastStreamToObject.ToReadableString();

        translated.ShouldBe("(object)stream");
    }

    [Fact]
    public void ShouldTranslateAnUpcastStringToObjectExpression()
    {
        var stringVariable = Variable(typeof(string), "str");
        var upcastStringToObject = Convert(stringVariable, typeof(object));

        var translated = upcastStringToObject.ToReadableString();

        translated.ShouldBe("str");
    }

    [Fact]
    public void ShouldTranslateAnUnboxingExpression()
    {
        var objectVariable = Variable(typeof(object), "o");
        var unboxObjectToInt = Unbox(objectVariable, typeof(int));

        var translated = unboxObjectToInt.ToReadableString();

        translated.ShouldBe("(int)o");
    }

    // https://github.com/agileobjects/ReadableExpressions/issues/20
    [Fact]
    public void ShouldTranslateConversionWithCustomStaticMethod()
    {
        var stringParameter = Parameter(typeof(string), "str");
        var targetType = typeof(int);

        var body = Convert(
            stringParameter,
            targetType,
            targetType.GetPublicStaticMethod(nameof(int.Parse), stringParameter.Type));

        var stringToIntParseLambda = Lambda<Func<string, int>>(body, stringParameter);

        var translated = stringToIntParseLambda.Body.ToReadableString();

        translated.ShouldBe("int.Parse(str)");
    }

    // https://github.com/agileobjects/ReadableExpressions/issues/117
    [Fact]
    public void ShouldTranslateExplicitUnaryConversionWithCustomStaticMethod()
    {
        var objectToBoolWithIsDbNullLambda = Lambda(
            MakeUnary(
                ExpressionType.Convert,
                Default(typeof(object)),
                typeof(bool),
                typeof(Convert).GetMethod("IsDBNull")));

        var translated =
            objectToBoolWithIsDbNullLambda.ToReadableString();

        translated.ShouldBe("() => Convert.IsDBNull(null)");
    }
}