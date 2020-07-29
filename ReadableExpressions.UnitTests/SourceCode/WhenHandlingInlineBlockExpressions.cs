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
        public void ShouldExtractAMultilineIfTestBlockToAPrivateMethod()
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
        public void ShouldExtractMultilineConditionalBranchesToPrivateMethods()
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
            return (i > 3) ? this.GetInt2(j, i) : this.GetInt3(j, i);
        }

        private int GetInt2
        (
            int j,
            int i
        )
        {
            j = 2;

            return i * j;
        }

        private int GetInt3
        (
            int j,
            int i
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

        [Fact]
        public void ShouldExtractMultilineBinaryOperandBlocksToPrivateMethods()
        {
            var intParameter1 = Parameter(typeof(int), "i");
            var intParameter2 = Parameter(typeof(int), "j");
            var intVariable1 = Variable(typeof(int), "k");
            var intVariable2 = Variable(typeof(int), "l");

            var yepOrNope = Condition(
                GreaterThan(
                    Block(
                        new[] { intVariable1 },
                        Assign(intVariable1, Constant(2)),
                        Multiply(intParameter1, intVariable1)),
                    Block(
                        new[] { intVariable2 },
                        Assign(intVariable2, Constant(3)),
                        Multiply(intParameter2, intVariable2))
                ),
                Constant("Yep"),
                Constant("Nope"));

            var translated = yepOrNope.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public string GetString
        (
            int i,
            int j
        )
        {
            return (this.GetInt1(i) > this.GetInt2(j)) ? ""Yep"" : ""Nope"";
        }

        private int GetInt1
        (
            int i
        )
        {
            var k = 2;

            return i * k;
        }

        private int GetInt2
        (
            int j
        )
        {
            var l = 3;

            return j * l;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldExtractNestedMultilineBlocksToPrivateMethods()
        {
            var parameterI = Parameter(typeof(int), "i");
            var variableJ = Variable(typeof(int), "j");
            var variableK = Variable(typeof(int), "k");
            var variableL = Variable(typeof(int), "l");

            var assignNestedBlockResult = Block(
                new[] { variableJ },
                Assign(
                    variableJ,
                    Block(
                        new[] { variableK },
                        Assign(variableK, Constant(2)),
                        Multiply(variableK, Block(
                            new[] { variableL },
                            Assign(variableL, Constant(3)),
                            Multiply(parameterI, variableL))))
                    ),
                variableJ);

            var assignmentLambda = Lambda<Func<int, int>>(assignNestedBlockResult, parameterI);

            var translated = assignmentLambda.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt1
        (
            int i
        )
        {
            var j = this.GetInt3(i);

            return j;
        }

        private int GetInt3
        (
            int i
        )
        {
            var k = 2;

            return k * this.GetInt2(i);
        }

        private int GetInt2
        (
            int i
        )
        {
            var l = 3;

            return i * l;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}