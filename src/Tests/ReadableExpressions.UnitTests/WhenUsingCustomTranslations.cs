namespace AgileObjects.ReadableExpressions.UnitTests;

using System.Linq;
#if !NET35
using System.Linq.Expressions;
#else
using Microsoft.Scripting.Ast;
#endif
using Common;
#if !NET35
using Xunit;
using static System.Linq.Expressions.Expression;
#else
using Fact = NUnit.Framework.TestAttribute;
using static Microsoft.Scripting.Ast.Expression;

[NUnit.Framework.TestFixture]
#endif
public class WhenUsingCustomTranslations : TestClassBase
{
    [Fact]
    public void ShouldUseACustomSourceCodeFactoryForARootExpression()
    {
        var nullString = Default(typeof(string));

        var translated = nullString.ToReadableString(stgs => stgs
            .AddTranslatorFor(ExpressionType.Default, (_, _) => "I'm null!"));

        translated.ShouldBe("I'm null!");
    }

    [Fact]
    public void ShouldAllowACustomSourceCodeFactoryToReturnNull()
    {
        var nullString = Default(typeof(string));

        var translated = nullString.ToReadableString(stgs => stgs
            .AddTranslatorFor(ExpressionType.Default, (_, _) => null));

        translated.ShouldBeNull();
    }

    [Fact]
    public void ShouldUseACustomSourceCodeFactoryForAChildExpression()
    {
        var defaultInt = Default(typeof(int));
        var oneTwoThree = Constant(123);

        var intArray = NewArrayInit(
            typeof(int),
            oneTwoThree,
            defaultInt,
            oneTwoThree);

        var translated = intArray.ToReadableString(stgs => stgs
            .AddTranslatorFor(ExpressionType.Default, (_, _) => "000"));

        translated.ShouldBe("new[] { 123, 000, 123 }");
    }

    [Fact]
    public void ShouldUseACustomSourceCodeFactoryForAParentExpression()
    {
        var defaultInt = Default(typeof(int));
        var fourFiveSix = Constant(456);

        var intArray = NewArrayInit(
            typeof(int),
            fourFiveSix,
            defaultInt,
            fourFiveSix);

        var translated = intArray.ToReadableString(stgs => stgs
            .AddTranslatorFor(ExpressionType.NewArrayInit, (expr, translator) =>
            {
                var arrayInit = (NewArrayExpression)expr;

                var childExpressions = arrayInit
                    .Expressions.Select(translator).ToArray();

                return $"{{ {string.Join(", ", childExpressions)} }}";
            }));

        translated.ShouldBe("{ 456, default(int), 456 }");
    }
}