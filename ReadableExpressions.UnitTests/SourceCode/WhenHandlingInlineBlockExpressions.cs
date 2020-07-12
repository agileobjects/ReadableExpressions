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
    public class WhenHandlingInlineBlockExpressions
    {
        [Fact]
        public void ShouldExtractAnInlineBlockIfTestToAPrivateMethod()
        {
            var intVariable = Parameter(typeof(int), "input");

            var yepOrNopeBlock = Block(
                IfThen(
                    Block(
                        new[]{ intVariable },
                        Assign(
                            intVariable, 
                            Call(typeof(Console), "Read", Type.EmptyTypes)),
                        Condition(
                            GreaterThan(intVariable, Constant(100)),
                            Constant(false),
                            Constant(true))),
                    Constant("Yep")),
                Constant("Nope"));

            var translated = yepOrNopeBlock.ToSourceCodeClass();

            const string EXPECTED = @"
public class GeneratedExpressionClass
{
    public string GetString()
    {
        if (GetBool())
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
}";
            EXPECTED.ShouldBeCompilableClass();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}