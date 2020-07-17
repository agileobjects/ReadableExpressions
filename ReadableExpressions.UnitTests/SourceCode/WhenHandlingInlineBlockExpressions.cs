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
    public class WhenHandlingInlineBlockExpressions
    {
        [Fact]
        public void ShouldExtractAnInlineIfTestBlockToAPrivateMethod()
        {
            var intVariable = Parameter(typeof(int), "input");

            var yepOrNopeBlock = Block(
                IfThen(
                    Block(
                        new[] { intVariable },
                        Assign(
                            intVariable,
                            Call(typeof(Console), "Read", Type.EmptyTypes)),
                        Condition(
                            GreaterThan(intVariable, Constant(100)),
                            Constant(false),
                            Constant(true))),
                    Constant("Yep")),
                Constant("Nope"));

            var yepOrNopeLambda = Lambda<Func<string>>(yepOrNopeBlock);

            var translated = SourceCode(sc => sc
                .WithClass(cls => cls
                    .WithMethod(yepOrNopeLambda)))
                .ToSourceCode();

            const string EXPECTED = @"
using System;

namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public string GetString()
        {
            if (this.GetBool())
            {
                return ""Yep"";
            }

            return ""Nope"";
        }

        private bool GetBool()
        {
            var input = Console.Read();

            return (input > 100) ? false : true;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}