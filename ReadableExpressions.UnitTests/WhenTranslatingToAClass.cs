namespace AgileObjects.ReadableExpressions.UnitTests
{
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingToAClass : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAParameterlessLambdaFuncToAMethod()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var translated = returnOneThousand.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedCode
{
    public class GeneratedClass
    {
        public int GetInt()
        {
            return 1000;
        }
    }
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
