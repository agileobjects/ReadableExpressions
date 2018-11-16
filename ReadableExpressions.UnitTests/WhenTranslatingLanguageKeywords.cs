namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingLanguageKeywords : TestClassBase
    {
        [Fact]
        public void ShouldTranslateADefaultExpression()
        {
            var defaultInt = Expression.Default(typeof(uint));
            var translated = ToReadableString(defaultInt);

            translated.ShouldBe("default(uint)");
        }

        [Fact]
        public void ShouldIgnoreADefaultVoidExpression()
        {
            var defaultVoid = Expression.Default(typeof(void));
            var translated = ToReadableString(defaultVoid);

            translated.ShouldBeNull();
        }

        [Fact]
        public void ShouldEscapeAKeywordVariable()
        {
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
            var variable = Expression.Variable(typeof(bool), keyword);
            var translated = ToReadableString(variable);

            translated.ShouldBe("@" + keyword);
        }

        [Fact]
        public void ShouldTranslateADeclaredBlockVariableKeyword()
        {
            var variable = Expression.Variable(typeof(string), "string");
            var writeLine = CreateLambda(() => Console.WriteLine("La la la"));
            var block = Expression.Block(new[] { variable }, writeLine.Body);
            var translated = ToReadableString(block);

            const string EXPECTED = @"
string @string;
Console.WriteLine(""La la la"");";

            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
