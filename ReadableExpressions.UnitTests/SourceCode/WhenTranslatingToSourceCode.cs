namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
{
    using System;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingToSourceCode : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnEmptyParameterlessLambdaActionToASourceCodeMethod()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public void DoAction()
        {
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAParameterlessLambdaFuncToASourceCodeMethod()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var translated = returnOneThousand.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt()
        {
            return 1000;
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateASingleParameterLambdaFuncToASourceCodeMethod()
        {
            var returnGivenLong = CreateLambda((long arg) => arg);

            var translated = returnGivenLong.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public long GetLong
        (
            long arg
        )
        {
            return arg;
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATwoParameterLambdaActionToASourceCodeMethod()
        {
            var subtractShortFromInt = CreateLambda((int value1, short value2) => value1 - value2);

            var translated = subtractShortFromInt.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt
        (
            int value1,
            short value2
        )
        {
            return value1 - ((int)value2);
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeASystemUsingFromADefaultExpression()
        {
            var getDefaultDate = Lambda<Func<DateTime>>(Default(typeof(DateTime)));

            var translated = getDefaultDate.ToSourceCode();

            const string EXPECTED = @"
using System;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public DateTime GetDateTime()
        {
            return default(DateTime);
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeASystemUsingFromAStaticMemberAccessExpression()
        {
            var dateTimeNow = Property(null, typeof(DateTime), nameof(DateTime.Now));
            var dateTimeTicks = Property(dateTimeNow, nameof(DateTime.Ticks));
            var getDefaultDate = Lambda<Func<long>>(dateTimeTicks);

            var translated = getDefaultDate.ToSourceCode();

            const string EXPECTED = @"
using System;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public long GetLong()
        {
            return DateTime.Now.Ticks;
        }
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
