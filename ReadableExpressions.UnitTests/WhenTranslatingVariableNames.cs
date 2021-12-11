namespace AgileObjects.ReadableExpressions.UnitTests
{
    using Common;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingVariableNames : TestClassBase
    {
        // See https://github.com/agileobjects/ReadableExpressions/issues/21
        [Fact]
        public void ShouldNameAnUnnamedVariable()
        {
            var intVariable = Variable(typeof(int));
            var assignDefaultToInt = Assign(intVariable, Default(typeof(int)));

            var translated = assignDefaultToInt.ToReadableString();

            translated.ShouldBe("@int = default(int)");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/100
        [Fact]
        public void ShouldNameARootUnnamedVariable()
        {
            var intVariable = Variable(typeof(int));

            var translated = intVariable.ToReadableString();

            translated.ShouldBe("@int");
        }

        [Fact]
        public void ShouldNameAnUnnamedParameter()
        {
            var stringParameter = Parameter(typeof(string), string.Empty);
            var stringVariable = Variable(typeof(string), "  ");
            var assignVariableToParameter = Assign(stringVariable, stringParameter);

            var translated = assignVariableToParameter.ToReadableString();

            translated.ShouldBe("string1 = string2");
        }
    }
}
