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

            var translated = SourceCode(sc => sc
                .WithClass(cls => cls
                    .WithMethod(yepOrNopeBlock)))
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

        [Fact]
        public void ShouldExtractInlineConditionalBranchesToPrivateMethods()
        {
            var intParameter1 = Parameter(typeof(int), "i");
            var intParameter2 = Parameter(typeof(int), "j");

            var conditional = Condition(
                GreaterThan(intParameter1, Constant(3)),
                Block(
                    Assign(intParameter2, Constant(2)),
                    Multiply(intParameter1, intParameter2)),
                Block(
                    Assign(intParameter2, Constant(3)),
                    Multiply(intParameter1, intParameter2)));

            var translated = SourceCode(sc => sc
                .WithClass(cls => cls
                    .WithMethod(conditional)))
                .ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt1
        (
            int i,
            int j
        )
        {
            return (i > 3) ? this.GetInt2(i, j) : this.GetInt3(i, j);
        }

        private int GetInt2
        (
            int i,
            int j
        )
        {
            j = 2;

            return i * j;
        }

        private int GetInt3
        (
            int i,
            int j
        )
        {
            j = 3;

            return i * j;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}