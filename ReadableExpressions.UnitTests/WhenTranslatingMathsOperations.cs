namespace AgileObjects.ReadableExpressions.UnitTests
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
    public class WhenTranslatingMathsOperations : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnAdditionExpression()
        {
            var addInts = CreateLambda((int i1, int i2) => i1 + i2);

            var translated = ToReadableString(addInts);

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

            var translated = ToReadableString(checkedAdditionLambda);

            translated.ShouldBe("(a, b) => checked(a + b)");
        }

        [Fact]
        public void ShouldTranslateASubtractionExpression()
        {
            var subtractInts = CreateLambda((int i1, int i2) => i1 - i2);

            var translated = ToReadableString(subtractInts);

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

            var translated = ToReadableString(checkedSubtractionLambda);

            translated.ShouldBe("(a, b) => checked(a - b)");
        }

        [Fact]
        public void ShouldTranslateANegationExpression()
        {
            var negateInts = CreateLambda((int i) => -i);

            var translated = ToReadableString(negateInts);

            translated.ShouldBe("i => -i");
        }

        [Fact]
        public void ShouldTranslateACheckedNegationExpression()
        {
            var intParameter = Parameter(typeof(int), "i");
            var checkedNegation = NegateChecked(intParameter);
            var checkedNegationLambda = Lambda<Func<int, int>>(checkedNegation, intParameter);

            var translated = ToReadableString(checkedNegationLambda);

            translated.ShouldBe("i => -i");
        }

        [Fact]
        public void ShouldTranslateAMultiplicationExpression()
        {
            var multiplyInts = CreateLambda((int i1, int i2) => i1 * i2);

            var translated = ToReadableString(multiplyInts);

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

            var translated = ToReadableString(checkedMultiplication);

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

            var translated = ToReadableString(varOneToThePowerOfVarTwo);

            translated.ShouldBe("d1 ** d2");
        }

        [Fact]
        public void ShouldTranslateADivisionExpression()
        {
            var divideInts = CreateLambda((int i1, int i2) => i1 / i2);

            var translated = ToReadableString(divideInts);

            translated.ShouldBe("(i1, i2) => i1 / i2");
        }

        [Fact]
        public void ShouldTranslateAModuloExpression()
        {
            var findModulo = CreateLambda((int i1, int i2) => i1 % i2);

            var translated = ToReadableString(findModulo.Body);

            translated.ShouldBe("i1 % i2");
        }

        [Fact]
        public void ShouldMaintainExpressionParenthesis()
        {
            var operationWithBrackets = CreateLambda((int i1, int i2) => (i1 / i2) * i1);

            var translated = ToReadableString(operationWithBrackets);

            translated.ShouldBe("(i1, i2) => (i1 / i2) * i1");
        }

        [Fact]
        public void ShouldTranslateAnIncrementExpression()
        {
            var intVariable = Variable(typeof(int), "i");
            var increment = Increment(intVariable);

            var translated = ToReadableString(increment);

            translated.ShouldBe("++i");
        }

        [Fact]
        public void ShouldTranslateAPreIncrementExpression()
        {
            var intVariable = Variable(typeof(int), "i");
            var preIncrement = PreIncrementAssign(intVariable);

            var translated = ToReadableString(preIncrement);

            translated.ShouldBe("++i");
        }

        [Fact]
        public void ShouldTranslateAPostIncrementExpression()
        {
            var intVariable = Variable(typeof(int), "i");
            var postIncrement = PostIncrementAssign(intVariable);

            var translated = ToReadableString(postIncrement);

            translated.ShouldBe("i++");
        }

        [Fact]
        public void ShouldTranslateADecrementExpression()
        {
            var intVariable = Variable(typeof(int), "i");
            var decrement = Decrement(intVariable);

            var translated = ToReadableString(decrement);

            translated.ShouldBe("--i");
        }

        [Fact]
        public void ShouldTranslateAPreDecrementExpression()
        {
            var intVariable = Variable(typeof(int), "i");
            var preDecrement = PreDecrementAssign(intVariable);

            var translated = ToReadableString(preDecrement);

            translated.ShouldBe("--i");
        }

        [Fact]
        public void ShouldTranslateAPostDecrementExpression()
        {
            var intVariable = Variable(typeof(int), "i");
            var postDecrement = PostDecrementAssign(intVariable);

            var translated = ToReadableString(postDecrement);

            translated.ShouldBe("i--");
        }

        [Fact]
        public void ShouldMaintainRelevantParentheses()
        {
            var mather = CreateLambda((int i, int j, int k) => (i + j) * k);

            var translated = ToReadableString(mather.Body);

            translated.ShouldBe("(i + j) * k");
        }
    }
}