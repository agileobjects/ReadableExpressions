namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

            Assert.AreEqual("checked { i += 10 }", translated);
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
        public void ShouldTranslateAMultiLineCheckedSubtractionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");

            Expression<Action> consoleRead = () => Console.Read();

            var variableOne = Expression.Variable(typeof(int), "one");
            var variableTwo = Expression.Variable(typeof(int), "two");

            var variableOneAssignment = Expression.Assign(variableOne, consoleRead.Body);
            var variableTwoAssignment = Expression.Assign(variableTwo, consoleRead.Body);

            var variableOnePlusTwo = Expression.Add(variableOne, variableTwo);

            var valueBlock = Expression.Block(
                new[] { variableOne, variableTwo },
                variableOneAssignment,
                variableTwoAssignment,
                variableOnePlusTwo);

            var substractOneAndAssign = Expression.SubtractAssignChecked(intVariable, valueBlock);

            var translated = substractOneAndAssign.ToReadableString();

            const string EXPECTED = @"
checked
{
    i -= 
    {
        var one = Console.Read();
        var two = Console.Read();
        return (one + two);
    }
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
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

            Assert.AreEqual("checked { i *= 2 }", translated);
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

            Assert.AreEqual("d **= 2d", translated);
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
        public void ShouldTranslateANegatedBooleanAssignment()
        {
            var boolVariable1 = Expression.Variable(typeof(bool), "isItNot");
            var boolVariable2 = Expression.Variable(typeof(bool), "isIt");
            var assignBool = Expression.Assign(boolVariable1, Expression.IsFalse(boolVariable2));
            var negated = Expression.Not(assignBool);

            var translated = negated.ToReadableString();

            Assert.AreEqual("!(isItNot = !isIt)", translated);
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

            Assert.AreEqual("i = j = 1", translated);
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

        [TestMethod]
        public void ShouldAssignTheResultOfATryCatch()
        {
            var intVariable = Expression.Variable(typeof(int), "i");

            Expression<Func<int>> read = () => Console.Read();

            var returnDefault = Expression.Catch(typeof(IOException), Expression.Default(typeof(int)));
            var readOrDefault = Expression.TryCatch(read.Body, returnDefault);

            var assignReadOrDefault = Expression.Assign(intVariable, readOrDefault);

            var translated = assignReadOrDefault.ToReadableString();

            const string EXPECTED = @"
i = 
{
    try
    {
        return Console.Read();
    }
    catch (IOException)
    {
        return default(int);
    }
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldAssignAVariableInAConditionalTest()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var assignVariable = Expression.Assign(intVariable, Expression.Constant(10));
            var isAssignmentFive = Expression.Equal(assignVariable, Expression.Constant(5));
            var ifFiveDoNothing = Expression.IfThen(isAssignmentFive, Expression.Empty());

            var translated = ifFiveDoNothing.ToReadableString();

            const string EXPECTED = @"
if ((i = 10) == 5)
{
}";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldAssignAVariableInAMethodCallArgument()
        {
            var stringVariable = Expression.Variable(typeof(string), "value");
            var setStringVariableToNull = Expression.Assign(stringVariable, Expression.Default(typeof(string)));

            var intVariable = Expression.Variable(typeof(int), "i");

            var intToStringMethod = typeof(int)
                .GetMethods()
                .First(m =>
                    (m.Name == "ToString") &&
                    (m.GetParameters().FirstOrDefault()?.ParameterType == typeof(string)));

            var intToString = Expression.Call(intVariable, intToStringMethod, setStringVariableToNull);

            var translated = intToString.ToReadableString();

            const string EXPECTED = @"i.ToString(value = null)";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleLineTernaryAssignment()
        {
            Expression<Action> consoleRead = () => Console.Read();

            var variableOne = Expression.Variable(typeof(int), "one");
            var variableTwo = Expression.Variable(typeof(int), "two");
            var resultVariableOne = Expression.Variable(typeof(int), "resultOne");

            var variableOneAssignment = Expression.Assign(variableOne, consoleRead.Body);
            var variableTwoAssignment = Expression.Assign(variableTwo, consoleRead.Body);

            var variableOneTimesTwo = Expression.Multiply(variableOne, variableTwo);
            var resultOneAssignment = Expression.Assign(resultVariableOne, variableOneTimesTwo);

            var ifTrueBlock = Expression.Block(
                new[] { variableOne, variableTwo, resultVariableOne },
                variableOneAssignment,
                variableTwoAssignment,
                resultOneAssignment,
                resultVariableOne);

            var variableThree = Expression.Variable(typeof(int), "three");
            var variableFour = Expression.Variable(typeof(int), "four");
            var resultVariableTwo = Expression.Variable(typeof(int), "resultTwo");

            var variableThreeAssignment = Expression.Assign(variableThree, consoleRead.Body);
            var variableFourAssignment = Expression.Assign(variableFour, consoleRead.Body);

            var variableThreeDivideFour = Expression.Divide(variableThree, variableFour);
            var resultTwoAssignment = Expression.Assign(resultVariableTwo, variableThreeDivideFour);

            var ifFalseBlock = Expression.Block(
                new[] { variableThree, variableFour, resultVariableTwo },
                variableThreeAssignment,
                variableFourAssignment,
                resultTwoAssignment,
                resultVariableTwo);

            var dateTimeNow = Expression.Property(null, typeof(DateTime), "Now");
            var nowHour = Expression.Property(dateTimeNow, "Hour");
            var nowHourModuloTwo = Expression.Modulo(nowHour, Expression.Constant(2));
            var nowHourIsEven = Expression.Equal(nowHourModuloTwo, Expression.Constant(0));

            var conditional = Expression.Condition(nowHourIsEven, ifTrueBlock, ifFalseBlock);

            var resultVariable = Expression.Variable(typeof(int), "result");
            var resultAssignment = Expression.Assign(resultVariable, conditional);

            var translated = resultAssignment.ToReadableString();

            const string EXPECTED = @"
result = ((DateTime.Now.Hour % 2) == 0)
    ? {
        var one = Console.Read();
        var two = Console.Read();
        var resultOne = one * two;
        return resultOne;
    }
    : {
        var three = Console.Read();
        var four = Console.Read();
        var resultTwo = three / four;
        return resultTwo;
    }";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        [TestMethod]
        public void ShouldTranslatedMultipleLineValueBlockAssignments()
        {
            Expression<Func<string[], IEnumerable<int>>> linqSelect = ints => ints.Select(int.Parse);
            var selectMethod = ((MethodCallExpression)linqSelect.Body).Method;

            Expression<Func<string[]>> getStringArray = () => new[] { "1", "2", "blah" };
            var stringArray = getStringArray.Body;

            // ReSharper disable once RedundantAssignment
            Expression<Func<string, int, int>> intTryParse = (str, value) => int.TryParse(str, out value) ? value : 0;
            var stringParameter = intTryParse.Parameters[0];
            var intVariable = intTryParse.Parameters[1];
            var tryParseTernary = intTryParse.Body;

            var tryParseBlock = Expression.Block(new[] { intVariable }, tryParseTernary);
            var tryParseLambda = Expression.Lambda<Func<string, int>>(tryParseBlock, stringParameter);

            var selectCall = Expression.Call(selectMethod, stringArray, tryParseLambda);

            Expression<Func<IEnumerable<int>, int[]>> linqToArray = ints => ints.ToArray();
            var toArrayMethod = ((MethodCallExpression)linqToArray.Body).Method;

            var toArrayCall = Expression.Call(toArrayMethod, selectCall);

            var resultVariable = Expression.Variable(typeof(IList<int>), "result");
            var assignment = Expression.Assign(resultVariable, toArrayCall);
            var assignmentBlock = Expression.Block(assignment);

            var translation = assignmentBlock.ToReadableString();

            const string EXPECTED = @"
IList<int> result = new[] { ""1"", ""2"", ""blah"" }
    .Select(str =>
    {
        int value;
        return int.TryParse(str, out value) ? value : 0;
    })
    .ToArray();";

            Assert.AreEqual(EXPECTED.TrimStart(), translation);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/7
        [TestMethod]
        public void ShouldTranslateANestedBlockAssignment()
        {
            Expression<Action> consoleRead = () => Console.Read();

            var variableOne = Expression.Variable(typeof(int), "one");
            var variableTwo = Expression.Variable(typeof(int), "two");

            var variableOneAssignment = Expression.Assign(variableOne, consoleRead.Body);
            var variableTwoAssignment = Expression.Assign(variableTwo, consoleRead.Body);

            var variableOneMinusTwo = Expression.Subtract(variableOne, variableTwo);

            var valueBlock = Expression.Block(
                new[] { variableOne, variableTwo },
                variableOneAssignment,
                variableTwoAssignment,
                variableOneMinusTwo);

            var wrappingBlock = Expression.Block(valueBlock);

            var resultVariable = Expression.Variable(typeof(int), "result");
            var resultOneAssignment = Expression.Assign(resultVariable, wrappingBlock);

            var translated = resultOneAssignment.ToReadableString();

            const string EXPECTED = @"
result = 
{
    var one = Console.Read();
    var two = Console.Read();
    return (one - two);
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/7
        [TestMethod]
        public void ShouldTranslateMultiStatementValueBlockAssignments()
        {
            ParameterExpression existingInts;
            var valueConditional = GetReturnStatementBlock(out existingInts);

            Expression<Action> consoleRead = () => Console.Read();

            var multiStatementValueBlock = Expression.Block(
                new[] { existingInts },
                consoleRead.Body,
                valueConditional);

            var resultVariable = Expression.Variable(multiStatementValueBlock.Type, "result");
            var resultOneAssignment = Expression.Assign(resultVariable, multiStatementValueBlock);

            var translated = resultOneAssignment.ToReadableString();

            const string EXPECTED = @"
result = 
{
    List<int> ints;
    Console.Read();
    return (ints == null)
        ? new List<int>()
        : {
            var enumerator = ints.GetEnumerator();
            while (true)
            {
                if (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    ints.Add(item);
                }
                else
                {
                    break;
                }
            }

            return ints;
        }
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/7
        [TestMethod]
        public void ShouldTranslateSingleStatementValueBlockAssignments()
        {
            ParameterExpression existingInts;
            var valueConditional = GetReturnStatementBlock(out existingInts);

            var singleStatementValueBlock = Expression.Block(
                new[] { existingInts },
                valueConditional);

            var resultVariable = Expression.Variable(singleStatementValueBlock.Type, "result");
            var resultOneAssignment = Expression.Assign(resultVariable, singleStatementValueBlock);

            var translated = resultOneAssignment.ToReadableString();

            const string EXPECTED = @"
result = 
{
    List<int> ints;
    return (ints == null)
        ? new List<int>()
        : {
            var enumerator = ints.GetEnumerator();
            while (true)
            {
                if (enumerator.MoveNext())
                {
                    var item = enumerator.Current;
                    ints.Add(item);
                }
                else
                {
                    break;
                }
            }

            return ints;
        }
}";
            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }

        private static Expression GetReturnStatementBlock(out ParameterExpression existingInts)
        {
            existingInts = Expression.Variable(typeof(List<int>), "ints");

            var existingIntsEnumerator = Expression.Variable(typeof(List<int>.Enumerator), "enumerator");
            var getEnumeratorMethod = existingInts.Type.GetMethod("GetEnumerator");
            var getEnumeratorCall = Expression.Call(existingInts, getEnumeratorMethod);
            var enumeratorAssignment = Expression.Assign(existingIntsEnumerator, getEnumeratorCall);

            var enumeratorMoveNextMethod = existingIntsEnumerator.Type.GetMethod("MoveNext");
            var enumeratorMoveNextCall = Expression.Call(existingIntsEnumerator, enumeratorMoveNextMethod);

            var enumeratorItem = Expression.Variable(typeof(int), "item");
            var enumeratorCurrent = Expression.Property(existingIntsEnumerator, "Current");
            var itemAssignment = Expression.Assign(enumeratorItem, enumeratorCurrent);

            var intsAddMethod = existingInts.Type.GetMethod("Add");
            var intsAddCall = Expression.Call(existingInts, intsAddMethod, enumeratorItem);

            var addItemBlock = Expression.Block(
                new[] { enumeratorItem },
                itemAssignment,
                intsAddCall);

            var loopBreakTarget = Expression.Label(typeof(void), "LoopBreak");

            var conditionallyAddItems = Expression.Condition(
                Expression.IsTrue(enumeratorMoveNextCall),
                addItemBlock,
                Expression.Break(loopBreakTarget));

            var addItemsLoop = Expression.Loop(conditionallyAddItems, loopBreakTarget);

            var populateExistingInts = Expression.Block(
                new[] { existingIntsEnumerator },
                enumeratorAssignment,
                addItemsLoop);

            var conditionFalseBlock = Expression.Block(
                populateExistingInts,
                existingInts);

            var valueConditional = Expression.Condition(
                Expression.Equal(existingInts, Expression.Default(existingInts.Type)),
                Expression.New(conditionFalseBlock.Type),
                conditionFalseBlock);

            return valueConditional;
        }
    }
}
