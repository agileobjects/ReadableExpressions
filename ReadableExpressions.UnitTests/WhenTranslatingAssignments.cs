namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingAssignments
    {
        [TestMethod]
        public void ShouldTranslateAnAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var assignDefaultToInt = Expression.Assign(intVariable, Expression.Default(typeof(int)));

            var translated = assignDefaultToInt.ToReadableString();

            Assert.AreEqual("i = default(int)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnAdditionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var addOneAndAssign = Expression.AddAssign(intVariable, Expression.Constant(1));

            var translated = addOneAndAssign.ToReadableString();

            Assert.AreEqual("i += 1", translated);
        }

        [TestMethod]
        public void ShouldTranslateACheckedAdditionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var addTenAndAssign = Expression.AddAssignChecked(intVariable, Expression.Constant(10));

            var translated = addTenAndAssign.ToReadableString();

            Assert.AreEqual("i += 10", translated);
        }

        [TestMethod]
        public void ShouldTranslateASubtractionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var substractTenAndAssign = Expression.SubtractAssign(intVariable, Expression.Constant(10));

            var translated = substractTenAndAssign.ToReadableString();

            Assert.AreEqual("i -= 10", translated);
        }

        [TestMethod]
        public void ShouldTranslateACheckedSubtractionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var substractOneAndAssign = Expression.SubtractAssignChecked(intVariable, Expression.Constant(1));

            var translated = substractOneAndAssign.ToReadableString();

            Assert.AreEqual("i -= 1", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultiplicationAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var tripleAndAssign = Expression.MultiplyAssign(intVariable, Expression.Constant(3));

            var translated = tripleAndAssign.ToReadableString();

            Assert.AreEqual("i *= 3", translated);
        }

        [TestMethod]
        public void ShouldTranslateACheckedMultiplicationAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var doubleAndAssign = Expression.MultiplyAssignChecked(intVariable, Expression.Constant(2));

            var translated = doubleAndAssign.ToReadableString();

            Assert.AreEqual("i *= 2", translated);
        }

        [TestMethod]
        public void ShouldTranslateADivisionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var halveAndAssign = Expression.DivideAssign(intVariable, Expression.Constant(2));

            var translated = halveAndAssign.ToReadableString();

            Assert.AreEqual("i /= 2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAModuloAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var moduloTwoAndAssign = Expression.ModuloAssign(intVariable, Expression.Constant(2));

            var translated = moduloTwoAndAssign.ToReadableString();

            Assert.AreEqual(@"i %= 2", translated);
        }

        [TestMethod]
        public void ShouldTranslateAPowerAssignment()
        {
            var doubleVariable = Expression.Variable(typeof(double), "d");
            var doubleTwo = Expression.Constant(2.0, typeof(double));
            var powerTwoAssign = Expression.PowerAssign(doubleVariable, doubleTwo);

            var translated = powerTwoAssign.ToReadableString();

            Assert.AreEqual("d **= 2.00", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseAndAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var bitwiseAndAssign = Expression.AndAssign(intVariableOne, intVariableTwo);

            var translated = bitwiseAndAssign.ToReadableString();

            Assert.AreEqual("i1 &= i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseOrAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var bitwiseOrAssign = Expression.OrAssign(intVariableOne, intVariableTwo);

            var translated = bitwiseOrAssign.ToReadableString();

            Assert.AreEqual("i1 |= i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateABitwiseExclusiveOrAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var bitwiseExclusiveOrAssign = Expression.ExclusiveOrAssign(intVariableOne, intVariableTwo);

            var translated = bitwiseExclusiveOrAssign.ToReadableString();

            Assert.AreEqual("i1 ^= i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateALeftShiftAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var leftShiftAndAssign = Expression.LeftShiftAssign(intVariableOne, intVariableTwo);

            var translated = leftShiftAndAssign.ToReadableString();

            Assert.AreEqual("i1 <<= i2", translated);
        }

        [TestMethod]
        public void ShouldTranslateARightShiftAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var rightShiftAndAssign = Expression.RightShiftAssign(intVariableOne, intVariableTwo);

            var translated = rightShiftAndAssign.ToReadableString();

            Assert.AreEqual("i1 >>= i2", translated);
        }

        [TestMethod]
        public void ShouldNotWrapAnAssignmentValueInParentheses()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var oneMultipliedByTwo = Expression.Multiply(Expression.Constant(1), Expression.Constant(2));
            var assignment = Expression.Assign(intVariable, oneMultipliedByTwo);

            var translated = assignment.ToReadableString();

            Assert.AreEqual("i = 1 * 2", translated);
        }

        [TestMethod]
        public void ShouldWrapAnAssignmentTernaryTestInParentheses()
        {
            var intVariable1 = Expression.Variable(typeof(int), "i");
            var intVariable2 = Expression.Variable(typeof(int), "j");

            var intVariable2GreaterThanOne = Expression.GreaterThan(intVariable2, Expression.Constant(1));

            var threeOrDefault = Expression.Condition(
                intVariable2GreaterThanOne,
                Expression.Constant(3),
                Expression.Default(typeof(int)));

            var assignment = Expression.Assign(intVariable1, threeOrDefault);

            var translated = assignment.ToReadableString();

            Assert.AreEqual("i = (j > 1) ? 3 : default(int)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnAssignmentResultAssignment()
        {
            var intVariable1 = Expression.Variable(typeof(int), "i");
            var intVariable2 = Expression.Variable(typeof(int), "j");
            var assignVariable2 = Expression.Assign(intVariable2, Expression.Constant(1));
            var setVariableOneToAssignmentResult = Expression.Assign(intVariable1, assignVariable2);

            var translated = setVariableOneToAssignmentResult.ToReadableString();

            Assert.AreEqual("i = (j = 1)", translated);
        }

        [TestMethod]
        public void ShouldTranslateABlockAssignmentResultAssignment()
        {
            var longVariable = Expression.Variable(typeof(long), "i");
            var intVariable = Expression.Variable(typeof(int), "j");
            var assignInt = Expression.Assign(intVariable, Expression.Constant(10));
            var castAssignmentResult = Expression.Convert(assignInt, typeof(long));
            var assignIntBlock = Expression.Block(castAssignmentResult);
            var setLongVariableToAssignmentResult = Expression.Assign(longVariable, assignIntBlock);

            var assignmentBlock = Expression.Block(
                new[] { longVariable, intVariable },
                setLongVariableToAssignmentResult);

            var translated = assignmentBlock.ToReadableString();

            const string EXPECTED = @"
int j;
var i = ((long)(j = 10));";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}
