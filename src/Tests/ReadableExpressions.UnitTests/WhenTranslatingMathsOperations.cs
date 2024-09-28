namespace AgileObjects.ReadableExpressions.UnitTests;

using System;

#if NET35
[NUnitTestFixture]
#endif
public class WhenTranslatingMathsOperations : TestClassBase
{
    [Fact]
    public void ShouldTranslateAnAdditionExpression()
    {
        var addInts = CreateLambda((int i1, int i2) => i1 + i2);

        var translated = addInts.ToReadableString();

        translated.ShouldBe("(i1, i2) => i1 + i2");
    }

    [Fact]
    public void ShouldTranslateACheckedAdditionExpression()
    {
        var intParameter1 = Parameter(typeof(int), "a");
        var intParameter2 = Parameter(typeof(int), "b");
        var checkedAddition = AddChecked(intParameter1, intParameter2);

        var checkedAdditionLambda = Lambda<Func<int, int, int>>(
            checkedAddition,
            intParameter1,
            intParameter2);

        var translated = checkedAdditionLambda.ToReadableString();

        translated.ShouldBe("(a, b) => checked(a + b)");
    }

    [Fact]
    public void ShouldTranslateASubtractionExpression()
    {
        var subtractInts = CreateLambda((int i1, int i2) => i1 - i2);

        var translated = subtractInts.ToReadableString();

        translated.ShouldBe("(i1, i2) => i1 - i2");
    }

    [Fact]
    public void ShouldTranslateACheckedSubtractionExpression()
    {
        var intParameter1 = Parameter(typeof(int), "a");
        var intParameter2 = Parameter(typeof(int), "b");
        var checkedSubtraction = SubtractChecked(intParameter1, intParameter2);

        var checkedSubtractionLambda = Lambda<Func<int, int, int>>(
            checkedSubtraction,
            intParameter1,
            intParameter2);

        var translated = checkedSubtractionLambda.ToReadableString();

        translated.ShouldBe("(a, b) => checked(a - b)");
    }

    [Fact]
    public void ShouldTranslateANegationExpression()
    {
        var negateInts = CreateLambda((int i) => -i);

        var translated = negateInts.ToReadableString();

        translated.ShouldBe("i => -i");
    }

    [Fact]
    public void ShouldTranslateACheckedNegationExpression()
    {
        var intParameter = Parameter(typeof(int), "i");
        var checkedNegation = NegateChecked(intParameter);
        var checkedNegationLambda = Lambda<Func<int, int>>(checkedNegation, intParameter);

        var translated = checkedNegationLambda.ToReadableString();

        translated.ShouldBe("i => -i");
    }

    [Fact]
    public void ShouldTranslateAMultiplicationExpression()
    {
        var multiplyInts = CreateLambda((int i1, int i2) => i1 * i2);

        var translated = multiplyInts.ToReadableString();

        translated.ShouldBe("(i1, i2) => i1 * i2");
    }

    [Fact]
    public void ShouldTranslateACheckedMultiplicationExpression()
    {
        var consoleRead = CreateLambda(() => Console.Read());

        var variableOne = Variable(typeof(int), "one");
        var variableTwo = Variable(typeof(int), "two");

        var variableOneAssignment = Assign(variableOne, consoleRead.Body);
        var variableTwoAssignment = Assign(variableTwo, consoleRead.Body);

        var variableOnePlusTwo = Add(variableOne, variableTwo);

        var valueOneBlock = Block(
            new[] { variableOne, variableTwo },
            variableOneAssignment,
            variableTwoAssignment,
            variableOnePlusTwo);

        var intVariable = Parameter(typeof(int), "i");
        var checkedMultiplication = MultiplyChecked(valueOneBlock, intVariable);

        var translated = checkedMultiplication.ToReadableString();

        const string EXPECTED = @"
checked
{
    {
        var one = Console.Read();
        var two = Console.Read();

        return one + two;
    } * i
}";

        translated.ShouldBe(EXPECTED.TrimStart());
    }

    [Fact]
    public void ShouldTranslateAMultiplicationPowerExpression()
    {
        var variableOne = Variable(typeof(double), "d1");
        var variableTwo = Variable(typeof(double), "d2");
        var varOneToThePowerOfVarTwo = Power(variableOne, variableTwo);

        var translated = varOneToThePowerOfVarTwo.ToReadableString();

        translated.ShouldBe("d1 ** d2");
    }

    [Fact]
    public void ShouldTranslateADivisionExpression()
    {
        var divideInts = CreateLambda((int i1, int i2) => i1 / i2);

        var translated = divideInts.ToReadableString();

        translated.ShouldBe("(i1, i2) => i1 / i2");
    }

    [Fact]
    public void ShouldTranslateAModuloExpression()
    {
        var findModulo = CreateLambda((int i1, int i2) => i1 % i2);

        var translated = findModulo.Body.ToReadableString();

        translated.ShouldBe("i1 % i2");
    }

    [Fact]
    public void ShouldMaintainExpressionParenthesis()
    {
        var operationWithBrackets = CreateLambda((int i1, int i2) => (i1 / i2) * i1);

        var translated = operationWithBrackets.ToReadableString();

        translated.ShouldBe("(i1, i2) => i1 / i2 * i1");
    }

    [Fact]
    public void ShouldTranslateAnIncrementExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var increment = Increment(intVariable);

        var translated = increment.ToReadableString();

        translated.ShouldBe("i + 1");
    }

    [Fact]
    public void ShouldTranslateAPreIncrementExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var preIncrement = PreIncrementAssign(intVariable);

        var translated = preIncrement.ToReadableString();

        translated.ShouldBe("++i");
    }

    [Fact]
    public void ShouldTranslateAPostIncrementExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var postIncrement = PostIncrementAssign(intVariable);

        var translated = postIncrement.ToReadableString();

        translated.ShouldBe("i++");
    }

    [Fact]
    public void ShouldTranslateADecrementExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var decrement = Decrement(intVariable);

        var translated = decrement.ToReadableString();

        translated.ShouldBe("i - 1");
    }

    [Fact]
    public void ShouldTranslateAPreDecrementExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var preDecrement = PreDecrementAssign(intVariable);

        var translated = preDecrement.ToReadableString();

        translated.ShouldBe("--i");
    }

    [Fact]
    public void ShouldTranslateAPostDecrementExpression()
    {
        var intVariable = Variable(typeof(int), "i");
        var postDecrement = PostDecrementAssign(intVariable);

        var translated = postDecrement.ToReadableString();

        translated.ShouldBe("i--");
    }

    [Fact]
    public void ShouldMaintainRelevantParentheses()
    {
        var mather = CreateLambda((int i, int j, int k) => (i + j) * k);

        var translated = mather.Body.ToReadableString();

        translated.ShouldBe("(i + j) * k");
    }
}