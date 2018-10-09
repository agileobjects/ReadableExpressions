namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

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
            var intParameter1 = Expression.Parameter(typeof(int), "a");
            var intParameter2 = Expression.Parameter(typeof(int), "b");
            var checkedAddition = Expression.AddChecked(intParameter1, intParameter2);

            var checkedAdditionLambda = Expression.Lambda<Func<int, int, int>>(
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
            var intParameter1 = Expression.Parameter(typeof(int), "a");
            var intParameter2 = Expression.Parameter(typeof(int), "b");
            var checkedSubtraction = Expression.SubtractChecked(intParameter1, intParameter2);

            var checkedSubtractionLambda = Expression.Lambda<Func<int, int, int>>(
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
            var intParameter = Expression.Parameter(typeof(int), "i");
            var checkedNegation = Expression.NegateChecked(intParameter);
            var checkedNegationLambda = Expression.Lambda<Func<int, int>>(checkedNegation, intParameter);

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

            var variableOne = Expression.Variable(typeof(int), "one");
            var variableTwo = Expression.Variable(typeof(int), "two");

            var variableOneAssignment = Expression.Assign(variableOne, consoleRead.Body);
            var variableTwoAssignment = Expression.Assign(variableTwo, consoleRead.Body);

            var variableOnePlusTwo = Expression.Add(variableOne, variableTwo);

            var valueOneBlock = Expression.Block(
                new[] { variableOne, variableTwo },
                variableOneAssignment,
                variableTwoAssignment,
                variableOnePlusTwo);

            var intVariable = Expression.Parameter(typeof(int), "i");
            var checkedMultiplication = Expression.MultiplyChecked(valueOneBlock, intVariable);

            var translated = ToReadableString(checkedMultiplication);

            const string EXPECTED = @"
checked
{
    {
        var one = Console.Read();
        var two = Console.Read();

        return (one + two);
    } * i
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultiplicationPowerExpression()
        {
            var variableOne = Expression.Variable(typeof(double), "d1");
            var variableTwo = Expression.Variable(typeof(double), "d2");
            var varOneToThePowerOfVarTwo = Expression.Power(variableOne, variableTwo);

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
            var intVariable = Expression.Variable(typeof(int), "i");
            var increment = Expression.Increment(intVariable);

            var translated = ToReadableString(increment);

            translated.ShouldBe("++i");
        }

        [Fact]
        public void ShouldTranslateAPreIncrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var preIncrement = Expression.PreIncrementAssign(intVariable);

            var translated = ToReadableString(preIncrement);

            translated.ShouldBe("++i");
        }

        [Fact]
        public void ShouldTranslateAPostIncrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var postIncrement = Expression.PostIncrementAssign(intVariable);

            var translated = ToReadableString(postIncrement);

            translated.ShouldBe("i++");
        }

        [Fact]
        public void ShouldTranslateADecrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var decrement = Expression.Decrement(intVariable);

            var translated = ToReadableString(decrement);

            translated.ShouldBe("--i");
        }

        [Fact]
        public void ShouldTranslateAPreDecrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var preDecrement = Expression.PreDecrementAssign(intVariable);

            var translated = ToReadableString(preDecrement);

            translated.ShouldBe("--i");
        }

        [Fact]
        public void ShouldTranslateAPostDecrementExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var postDecrement = Expression.PostDecrementAssign(intVariable);

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