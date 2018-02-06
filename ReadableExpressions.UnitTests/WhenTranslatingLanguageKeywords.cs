namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Xunit;

    public class WhenTranslatingLanguageKeywords
    {
        [Fact]
        public void ShouldTranslateADefaultExpression()
        {
            var defaultInt = Expression.Default(typeof(uint));
            var translated = defaultInt.ToReadableString();

            Assert.Equal("default(uint)", translated);
        }

        [Fact]
        public void ShouldIgnoreADefaultVoidExpression()
        {
            var defaultVoid = Expression.Default(typeof(void));
            var translated = defaultVoid.ToReadableString();

            Assert.Null(translated);
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
            var translated = variable.ToReadableString();

            Assert.Equal("@" + keyword, translated);
        }

        [Fact]
        public void ShouldTranslateADeclaredBlockVariableKeyword()
        {
            var variable = Expression.Variable(typeof(string), "string");
            Expression<Action> writeLine = () => Console.WriteLine("La la la");
            var block = Expression.Block(new[] { variable }, writeLine.Body);
            var translated = block.ToReadableString();

            const string EXPECTED = @"
string @string;
Console.WriteLine(""La la la"");";

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }
    }
}
