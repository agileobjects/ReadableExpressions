namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
{
    using System;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
    using static ReadableExpression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;
    using static ReadableExpression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenBuildingSourceCode
    {
        [Fact]
        public void ShouldBuildFromAnEmptyParameterlessLambdaAction()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = SourceCode(cfg => cfg
                .WithNamespace("GeneratedStuffs.Yo")
                .WithClass(cls => cls
                    .WithMethod(doNothing)))
                .ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedStuffs.Yo
{
    public class GeneratedExpressionClass
    {
        public void DoAction()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldBuildANamedClassAndMethod()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = SourceCode(cfg => cfg
                .WithClass("MyClass", cls => cls
                    .WithMethod("MyMethod", doNothing)))
                .ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class MyClass
    {
        public void MyMethod()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldBuildNamedClassesAndMethodsWithSummaries()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            const string CLASS_SUMMARY = @"
This is my class!
Isn't it great?";

            const string METHOD_SUMMARY = @"
This is my method!
It's even better.";

            var translated = SourceCode(cfg => cfg
                .WithClass("MyClass", CLASS_SUMMARY.TrimStart(), cls => cls
                    .WithMethod("MyMethod", METHOD_SUMMARY.TrimStart(), doNothing)))
                .ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    /// <summary>
    /// This is my class!
    /// Isn't it great?
    /// </summary>
    public class MyClass
    {
        /// <summary>
        /// This is my method!
        /// It's even better.
        /// </summary>
        public void MyMethod()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldBuildMultipleClassesAndMethods()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));
            var getBlah = Lambda<Func<string>>(Constant("Blah"));
            var getOne = Lambda<Func<int>>(Constant(1));
            var getTen = Lambda<Func<int>>(Constant(10));

            var translated = SourceCode(cfg => cfg
                    .WithClass(cls => cls
                        .WithMethod("DoNothing", doNothing)
                        .WithMethod(getBlah))
                    .WithClass(cls => cls
                        .WithMethod(getOne)
                        .WithMethod(getTen)))
                .ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass1
    {
        public void DoNothing()
        {
        }

        public string GetString()
        {
            return ""Blah"";
        }
    }

    public class GeneratedExpressionClass2
    {
        public int GetInt1()
        {
            return 1;
        }

        public int GetInt2()
        {
            return 10;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
