namespace AgileObjects.ReadableExpressions.UnitTests
{
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingVariableNames : TestClassBase
    {
        // See https://github.com/agileobjects/ReadableExpressions/issues/21
        [Fact]
        public void ShouldNameAnUnnamedVariable()
        {
            var intVariable = Expression.Variable(typeof(int));
            var assignDefaultToInt = Expression.Assign(intVariable, Expression.Default(typeof(int)));

            var translated = ToReadableString(assignDefaultToInt);

            translated.ShouldBe("@int = default(int)");
        }

        [Fact]
        public void ShouldNameAnUnnamedParameter()
        {
            var stringParameter = Expression.Parameter(typeof(string), string.Empty);
            var stringVariable = Expression.Variable(typeof(string), "  ");
            var assignVariableToParameter = Expression.Assign(stringVariable, stringParameter);

            var translated = ToReadableString(assignVariableToParameter);

            translated.ShouldBe("string1 = string2");
        }
    }
}
