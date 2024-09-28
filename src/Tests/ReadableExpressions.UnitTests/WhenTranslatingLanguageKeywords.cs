namespace AgileObjects.ReadableExpressions.UnitTests;

using System;

#if NET35
[NUnitTestFixture]
#endif
public class WhenTranslatingLanguageKeywords : TestClassBase
{
    [Fact]
    public void ShouldTranslateADefaultExpression()
    {
        var defaultInt = Default(typeof(uint));
        var translated = defaultInt.ToReadableString();

        translated.ShouldBe("default(uint)");
    }

    [Fact]
    public void ShouldIgnoreADefaultVoidExpression()
    {
        var defaultVoid = Default(typeof(void));
        var translated = defaultVoid.ToReadableString();

        translated.ShouldBeNull();
    }

    [Fact]
    public void ShouldEscapeAKeywordVariable()
    {
        VerifyIsEscaped("const");
        VerifyIsEscaped("int");
        VerifyIsEscaped("typeof");
        VerifyIsEscaped("default");
        VerifyIsEscaped("void");
        VerifyIsEscaped("readonly");
        VerifyIsEscaped("do");
        VerifyIsEscaped("while");
        VerifyIsEscaped("switch");
        VerifyIsEscaped("if");
        VerifyIsEscaped("else");
        VerifyIsEscaped("try");
        VerifyIsEscaped("catch");
        VerifyIsEscaped("finally");
        VerifyIsEscaped("throw");
        VerifyIsEscaped("for");
        VerifyIsEscaped("foreach");
        VerifyIsEscaped("goto");
        VerifyIsEscaped("return");
        VerifyIsEscaped("implicit");
        VerifyIsEscaped("explicit");
    }

    private static void VerifyIsEscaped(string keyword)
    {
        var variable = Variable(typeof(bool), keyword);
        var translated = variable.ToReadableString();

        translated.ShouldBe("@" + keyword);
    }

    [Fact]
    public void ShouldTranslateADeclaredBlockVariableKeyword()
    {
        var variable = Variable(typeof(string), "string");
        var writeLine = CreateLambda(() => Console.WriteLine("La la la"));
        var block = Block(new[] { variable }, writeLine.Body);
        var translated = block.ToReadableString();

        const string EXPECTED = @"
string @string;
Console.WriteLine(""La la la"");";

        translated.ShouldBe(EXPECTED.TrimStart());
    }
}