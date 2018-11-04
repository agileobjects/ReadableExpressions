namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NetStandardPolyfills;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingAssignments : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var assignDefaultToInt = Expression.Assign(intVariable, Expression.Default(typeof(int)));

            var translated = ToReadableString(assignDefaultToInt);

            translated.ShouldBe("i = default(int)");
        }

        [Fact]
        public void ShouldTranslateAnAdditionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var addOneAndAssign = Expression.AddAssign(intVariable, Expression.Constant(1));

            var translated = ToReadableString(addOneAndAssign);

            translated.ShouldBe("i += 1");
        }

        [Fact]
        public void ShouldTranslateACheckedAdditionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var addTenAndAssign = Expression.AddAssignChecked(intVariable, Expression.Constant(10));

            var translated = ToReadableString(addTenAndAssign);

            translated.ShouldBe("checked { i += 10; }");
        }

        [Fact]
        public void ShouldTranslateASubtractionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var substractTenAndAssign = Expression.SubtractAssign(intVariable, Expression.Constant(10));

            var translated = ToReadableString(substractTenAndAssign);

            translated.ShouldBe("i -= 10");
        }

        [Fact]
        public void ShouldTranslateAMultiLineCheckedSubtractionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");

            var consoleRead = CreateLambda(() => Console.Read());

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

            var translated = ToReadableString(substractOneAndAssign);

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

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultiplicationAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var tripleAndAssign = Expression.MultiplyAssign(intVariable, Expression.Constant(3));

            var translated = ToReadableString(tripleAndAssign);

            translated.ShouldBe("i *= 3");
        }

        [Fact]
        public void ShouldTranslateACheckedMultiplicationAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var doubleAndAssign = Expression.MultiplyAssignChecked(intVariable, Expression.Constant(2));

            var translated = ToReadableString(doubleAndAssign);

            translated.ShouldBe("checked { i *= 2; }");
        }

        [Fact]
        public void ShouldTranslateADivisionAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var halveAndAssign = Expression.DivideAssign(intVariable, Expression.Constant(2));

            var translated = ToReadableString(halveAndAssign);

            translated.ShouldBe("i /= 2");
        }

        [Fact]
        public void ShouldTranslateAModuloAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var moduloTwoAndAssign = Expression.ModuloAssign(intVariable, Expression.Constant(2));

            var translated = ToReadableString(moduloTwoAndAssign);

            translated.ShouldBe(@"i %= 2");
        }

        [Fact]
        public void ShouldTranslateAPowerAssignment()
        {
            var doubleVariable = Expression.Variable(typeof(double), "d");
            var doubleTwo = Expression.Constant(2.0, typeof(double));
            var powerTwoAssign = Expression.PowerAssign(doubleVariable, doubleTwo);

            var translated = ToReadableString(powerTwoAssign);

            translated.ShouldBe("d **= 2d");
        }

        [Fact]
        public void ShouldTranslateABitwiseAndAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var bitwiseAndAssign = Expression.AndAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(bitwiseAndAssign);

            translated.ShouldBe("i1 &= i2");
        }

        [Fact]
        public void ShouldTranslateABitwiseOrAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var bitwiseOrAssign = Expression.OrAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(bitwiseOrAssign);

            translated.ShouldBe("i1 |= i2");
        }

        [Fact]
        public void ShouldTranslateABitwiseExclusiveOrAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var bitwiseExclusiveOrAssign = Expression.ExclusiveOrAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(bitwiseExclusiveOrAssign);

            translated.ShouldBe("i1 ^= i2");
        }

        [Fact]
        public void ShouldTranslateALeftShiftAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var leftShiftAndAssign = Expression.LeftShiftAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(leftShiftAndAssign);

            translated.ShouldBe("i1 <<= i2");
        }

        [Fact]
        public void ShouldTranslateARightShiftAssignment()
        {
            var intVariableOne = Expression.Variable(typeof(int), "i1");
            var intVariableTwo = Expression.Variable(typeof(int), "i2");
            var rightShiftAndAssign = Expression.RightShiftAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(rightShiftAndAssign);

            translated.ShouldBe("i1 >>= i2");
        }

        [Fact]
        public void ShouldNotWrapAnAssignmentValueInParentheses()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var oneMultipliedByTwo = Expression.Multiply(Expression.Constant(1), Expression.Constant(2));
            var assignment = Expression.Assign(intVariable, oneMultipliedByTwo);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("i = 1 * 2");
        }

        [Fact]
        public void ShouldTranslateANegatedBooleanAssignment()
        {
            var boolVariable1 = Expression.Variable(typeof(bool), "isItNot");
            var boolVariable2 = Expression.Variable(typeof(bool), "isIt");
            var assignBool = Expression.Assign(boolVariable1, Expression.IsFalse(boolVariable2));
            var negated = Expression.Not(assignBool);

            var translated = ToReadableString(negated);

            translated.ShouldBe("!(isItNot = !isIt)");
        }

        [Fact]
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

            var translated = ToReadableString(assignment);

            translated.ShouldBe("i = (j > 1) ? 3 : default(int)");
        }

        [Fact]
        public void ShouldTranslateAnAssignmentResultAssignment()
        {
            var intVariable1 = Expression.Variable(typeof(int), "i");
            var intVariable2 = Expression.Variable(typeof(int), "j");
            var assignVariable2 = Expression.Assign(intVariable2, Expression.Constant(1));
            var setVariableOneToAssignmentResult = Expression.Assign(intVariable1, assignVariable2);

            var translated = ToReadableString(setVariableOneToAssignmentResult);

            translated.ShouldBe("i = j = 1");
        }

        [Fact]
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

            var translated = ToReadableString(assignmentBlock);

            const string EXPECTED = @"
int j;
var i = ((long)(j = 10));";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldAssignTheResultOfATryCatch()
        {
            var intVariable = Expression.Variable(typeof(int), "i");

            var read = CreateLambda(() => Console.Read());

            var returnDefault = Expression.Catch(typeof(IOException), Expression.Default(typeof(int)));
            var readOrDefault = Expression.TryCatch(read.Body, returnDefault);

            var assignReadOrDefault = Expression.Assign(intVariable, readOrDefault);

            var translated = ToReadableString(assignReadOrDefault);

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

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldAssignAVariableInAConditionalTest()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var assignVariable = Expression.Assign(intVariable, Expression.Constant(10));
            var isAssignmentFive = Expression.Equal(assignVariable, Expression.Constant(5));
            var ifFiveDoNothing = Expression.IfThen(isAssignmentFive, Expression.Empty());

            var translated = ToReadableString(ifFiveDoNothing);

            const string EXPECTED = @"
if ((i = 10) == 5)
{
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
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

            var translated = ToReadableString(intToString);

            translated.ShouldBe("i.ToString(value = null)");
        }

        [Fact]
        public void ShouldAssignAMultiplicationToStringResult()
        {
            var timesThreeToString = CreateLambda((int i) => (i * 3).ToString());

            var stringVariable = Expression.Variable(typeof(string), "value");
            var stringAssignment = Expression.Assign(stringVariable, timesThreeToString.Body);

            var translated = ToReadableString(stringAssignment);

            translated.ShouldBe("value = (i * 3).ToString()");
        }

        [Fact]
        public void ShouldTranslateAMultipleLineTernaryAssignment()
        {
            var consoleRead = CreateLambda(() => Console.Read());

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

            var translated = ToReadableString(resultAssignment);

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
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslatedMultipleLineValueBlockAssignments()
        {
            var linqSelect = CreateLambda((string[] ints) => ints.Select(int.Parse));
            var selectMethod = ((MethodCallExpression)linqSelect.Body).Method;

            var getStringArray = CreateLambda(() => new[] { "1", "2", "blah" });
            var stringArray = getStringArray.Body;

            // ReSharper disable once RedundantAssignment
            var intTryParse = CreateLambda((string str, int value) => int.TryParse(str, out value) ? value : 0);
            var stringParameter = intTryParse.Parameters[0];
            var intVariable = intTryParse.Parameters[1];
            var tryParseTernary = intTryParse.Body;

            var tryParseBlock = Expression.Block(new[] { intVariable }, tryParseTernary);
            var tryParseLambda = Expression.Lambda<Func<string, int>>(tryParseBlock, stringParameter);

            var selectCall = Expression.Call(selectMethod, stringArray, tryParseLambda);

            var linqToArray = CreateLambda((IEnumerable<int> ints) => ints.ToArray());
            var toArrayMethod = ((MethodCallExpression)linqToArray.Body).Method;

            var toArrayCall = Expression.Call(toArrayMethod, selectCall);

            var resultVariable = Expression.Variable(typeof(IList<int>), "result");
            var assignment = Expression.Assign(resultVariable, toArrayCall);
            var assignmentBlock = Expression.Block(assignment);

            var translation = ToReadableString(assignmentBlock);

            const string EXPECTED = @"
IList<int> result = new[] { ""1"", ""2"", ""blah"" }
    .Select(str =>
    {
        int value;
        return int.TryParse(str, out value) ? value : 0;
    })
    .ToArray();";

            translation.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/7
        [Fact]
        public void ShouldTranslateANestedBlockAssignment()
        {
            var consoleRead = CreateLambda(() => Console.Read());

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

            var translated = ToReadableString(resultOneAssignment);

            const string EXPECTED = @"
result =
{
    var one = Console.Read();
    var two = Console.Read();

    return (one - two);
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/7
        [Fact]
        public void ShouldTranslateMultiStatementValueBlockAssignments()
        {
            var valueConditional = GetReturnStatementBlock(out var existingInts);

            var consoleRead = CreateLambda(() => Console.Read());

            var multiStatementValueBlock = Expression.Block(
                new[] { existingInts },
                consoleRead.Body,
                valueConditional);

            var resultVariable = Expression.Variable(multiStatementValueBlock.Type, "result");
            var resultOneAssignment = Expression.Assign(resultVariable, multiStatementValueBlock);

            var translated = ToReadableString(resultOneAssignment);

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
        };
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/7
        [Fact]
        public void ShouldTranslateSingleStatementValueBlockAssignments()
        {
            var valueConditional = GetReturnStatementBlock(out var existingInts);

            var singleStatementValueBlock = Expression.Block(
                new[] { existingInts },
                valueConditional);

            var resultVariable = Expression.Variable(singleStatementValueBlock.Type, "result");
            var resultOneAssignment = Expression.Assign(resultVariable, singleStatementValueBlock);

            var translated = ToReadableString(resultOneAssignment);

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
        };
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAssignmentsOfNestedVariableBlocksWithATernaryReturnValue()
        {
            var objectVariable = Expression.Variable(typeof(object), "id");
            var objectValue = Expression.Variable(typeof(object), "value");
            var intVariable = Expression.Variable(typeof(int), "num");
            var intValue = Expression.Variable(typeof(int), "numValue");

            var objectNotNull = Expression.NotEqual(objectVariable, Expression.Default(typeof(object)));
            var defaultInt = Expression.Default(typeof(int));

            var intTryParse = Expression.Call(
                typeof(int).GetPublicStaticMethod("TryParse", parameterCount: 2),
                Expression.Condition(
                    objectNotNull,
                    Expression.Call(objectVariable, typeof(object).GetPublicInstanceMethod("ToString")),
                    Expression.Default(typeof(string))),
                intValue);

            var objectAsIntOrDefault = Expression.Condition(intTryParse, intValue, defaultInt);

            var intParseInnerBlock = Expression.Block(new[] { intValue }, objectAsIntOrDefault);

            var intParseOuterBlock = Expression.Block(
                new[] { objectVariable },
                Expression.Assign(objectVariable, objectValue),
                intParseInnerBlock);

            var intAssignment = Expression.Assign(intVariable, intParseOuterBlock);

            var translated = ToReadableString(intAssignment);

            const string EXPECTED = @"
num =
{
    var id = value;

    int numValue;
    return int.TryParse((id != null) ? id.ToString() : null, out numValue) ? numValue : default(int);
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAssignmentsOfNestedVariableBlocksWithANestedTernaryReturnValue()
        {
            var objectVariable = Expression.Variable(typeof(object), "id");
            var objectValue = Expression.Variable(typeof(object), "value");
            var longVariable = Expression.Variable(typeof(long), "number");
            var longValue = Expression.Variable(typeof(long), "numberValue");

            var longTryParse = Expression.Call(
                null,
                typeof(long).GetPublicStaticMethod("TryParse", parameterCount: 2),
                Expression.Call(objectVariable, typeof(object).GetPublicInstanceMethod("ToString")),
                longValue);

            var objectNotNull = Expression.NotEqual(objectVariable, Expression.Default(typeof(object)));
            var defaultlong = Expression.Default(typeof(long));

            var objectAslongOrDefault = Expression.Condition(
                objectNotNull,
                Expression.Condition(longTryParse, longValue, defaultlong),
                defaultlong);

            var longParseInnerBlock = Expression.Block(new[] { longValue }, objectAslongOrDefault);

            var longParseOuterBlock = Expression.Block(
                new[] { objectVariable },
                Expression.Assign(objectVariable, objectValue),
                longParseInnerBlock);

            var longAssignment = Expression.Assign(longVariable, longParseOuterBlock);

            var translated = ToReadableString(longAssignment);

            const string EXPECTED = @"
number =
{
    var id = value;

    long numberValue;
    return (id != null)
        ? long.TryParse(id.ToString(), out numberValue) ? numberValue : default(long)
        : default(long);
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnExtensionAssignment()
        {
            var value = new ExtensionExpression(typeof(int));
            var extensionVariable = Expression.Variable(value.Type, "ext");
            var assignment = Expression.Assign(extensionVariable, value);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("ext = " + value);
        }

        private static Expression GetReturnStatementBlock(out ParameterExpression existingInts)
        {
            existingInts = Expression.Variable(typeof(List<int>), "ints");

            var existingIntsEnumerator = Expression.Variable(typeof(List<int>.Enumerator), "enumerator");
            var getEnumeratorMethod = existingInts.Type.GetPublicInstanceMethod("GetEnumerator");
            var getEnumeratorCall = Expression.Call(existingInts, getEnumeratorMethod);
            var enumeratorAssignment = Expression.Assign(existingIntsEnumerator, getEnumeratorCall);

            var enumeratorMoveNextMethod = existingIntsEnumerator.Type.GetPublicInstanceMethod("MoveNext");
            var enumeratorMoveNextCall = Expression.Call(existingIntsEnumerator, enumeratorMoveNextMethod);

            var enumeratorItem = Expression.Variable(typeof(int), "item");
            var enumeratorCurrent = Expression.Property(existingIntsEnumerator, "Current");
            var itemAssignment = Expression.Assign(enumeratorItem, enumeratorCurrent);

            var intsAddMethod = existingInts.Type.GetPublicInstanceMethod("Add");
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
