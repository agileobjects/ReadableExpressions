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
    using static System.Linq.Expressions.Expression;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingAssignments : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var assignDefaultToInt = Assign(intVariable, Default(typeof(int)));

            var translated = ToReadableString(assignDefaultToInt);

            translated.ShouldBe("i = default(int)");
        }

        [Fact]
        public void ShouldTranslateAnAdditionAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var addOneAndAssign = AddAssign(intVariable, Constant(1));

            var translated = ToReadableString(addOneAndAssign);

            translated.ShouldBe("i += 1");
        }

        [Fact]
        public void ShouldTranslateACheckedAdditionAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var addTenAndAssign = AddAssignChecked(intVariable, Constant(10));

            var translated = ToReadableString(addTenAndAssign);

            translated.ShouldBe("checked { i += 10; }");
        }

        [Fact]
        public void ShouldTranslateASubtractionAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var substractTenAndAssign = SubtractAssign(intVariable, Constant(10));

            var translated = ToReadableString(substractTenAndAssign);

            translated.ShouldBe("i -= 10");
        }

        [Fact]
        public void ShouldTranslateAMultiLineCheckedSubtractionAssignment()
        {
            var intVariable = Variable(typeof(int), "i");

            var consoleRead = CreateLambda(() => Console.Read());

            var variableOne = Variable(typeof(int), "one");
            var variableTwo = Variable(typeof(int), "two");

            var variableOneAssignment = Assign(variableOne, consoleRead.Body);
            var variableTwoAssignment = Assign(variableTwo, consoleRead.Body);

            var variableOnePlusTwo = Add(variableOne, variableTwo);

            var valueBlock = Block(
                new[] { variableOne, variableTwo },
                variableOneAssignment,
                variableTwoAssignment,
                variableOnePlusTwo);

            var substractOneAndAssign = SubtractAssignChecked(intVariable, valueBlock);

            var translated = ToReadableString(substractOneAndAssign);

            const string EXPECTED = @"
checked
{
    i -=
    {
        var one = Console.Read();
        var two = Console.Read();

        return one + two;
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAMultiplicationAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var tripleAndAssign = MultiplyAssign(intVariable, Constant(3));

            var translated = ToReadableString(tripleAndAssign);

            translated.ShouldBe("i *= 3");
        }

        [Fact]
        public void ShouldTranslateACheckedMultiplicationAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var doubleAndAssign = MultiplyAssignChecked(intVariable, Constant(2));

            var translated = ToReadableString(doubleAndAssign);

            translated.ShouldBe("checked { i *= 2; }");
        }

        [Fact]
        public void ShouldTranslateADivisionAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var halveAndAssign = DivideAssign(intVariable, Constant(2));

            var translated = ToReadableString(halveAndAssign);

            translated.ShouldBe("i /= 2");
        }

        [Fact]
        public void ShouldTranslateAModuloAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var moduloTwoAndAssign = ModuloAssign(intVariable, Constant(2));

            var translated = ToReadableString(moduloTwoAndAssign);

            translated.ShouldBe(@"i %= 2");
        }

        [Fact]
        public void ShouldTranslateAPowerAssignment()
        {
            var doubleVariable = Variable(typeof(double), "d");
            var doubleTwo = Constant(2.0, typeof(double));
            var powerTwoAssign = PowerAssign(doubleVariable, doubleTwo);

            var translated = ToReadableString(powerTwoAssign);

            translated.ShouldBe("d **= 2d");
        }

        [Fact]
        public void ShouldTranslateABitwiseAndAssignment()
        {
            var intVariableOne = Variable(typeof(int), "i1");
            var intVariableTwo = Variable(typeof(int), "i2");
            var bitwiseAndAssign = AndAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(bitwiseAndAssign);

            translated.ShouldBe("i1 &= i2");
        }

        [Fact]
        public void ShouldTranslateABitwiseOrAssignment()
        {
            var intVariableOne = Variable(typeof(int), "i1");
            var intVariableTwo = Variable(typeof(int), "i2");
            var bitwiseOrAssign = OrAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(bitwiseOrAssign);

            translated.ShouldBe("i1 |= i2");
        }

        [Fact]
        public void ShouldTranslateABitwiseExclusiveOrAssignment()
        {
            var intVariableOne = Variable(typeof(int), "i1");
            var intVariableTwo = Variable(typeof(int), "i2");
            var bitwiseExclusiveOrAssign = ExclusiveOrAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(bitwiseExclusiveOrAssign);

            translated.ShouldBe("i1 ^= i2");
        }

        [Fact]
        public void ShouldTranslateALeftShiftAssignment()
        {
            var intVariableOne = Variable(typeof(int), "i1");
            var intVariableTwo = Variable(typeof(int), "i2");
            var leftShiftAndAssign = LeftShiftAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(leftShiftAndAssign);

            translated.ShouldBe("i1 <<= i2");
        }

        [Fact]
        public void ShouldTranslateARightShiftAssignment()
        {
            var intVariableOne = Variable(typeof(int), "i1");
            var intVariableTwo = Variable(typeof(int), "i2");
            var rightShiftAndAssign = RightShiftAssign(intVariableOne, intVariableTwo);

            var translated = ToReadableString(rightShiftAndAssign);

            translated.ShouldBe("i1 >>= i2");
        }

        [Fact]
        public void ShouldNotWrapAnAssignmentValueInParentheses()
        {
            var intVariable = Variable(typeof(int), "i");
            var oneMultipliedByTwo = Multiply(Constant(1), Constant(2));
            var assignment = Assign(intVariable, oneMultipliedByTwo);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("i = 1 * 2");
        }

        [Fact]
        public void ShouldTranslateANegatedBooleanAssignment()
        {
            var boolVariable1 = Variable(typeof(bool), "isItNot");
            var boolVariable2 = Variable(typeof(bool), "isIt");
            var assignBool = Assign(boolVariable1, IsFalse(boolVariable2));
            var negated = Not(assignBool);

            var translated = ToReadableString(negated);

            translated.ShouldBe("!(isItNot = !isIt)");
        }

        [Fact]
        public void ShouldWrapAnAssignmentTernaryTestInParentheses()
        {
            var intVariable1 = Variable(typeof(int), "i");
            var intVariable2 = Variable(typeof(int), "j");

            var intVariable2GreaterThanOne = GreaterThan(intVariable2, Constant(1));

            var threeOrDefault = Condition(
                intVariable2GreaterThanOne,
                Constant(3),
                Default(typeof(int)));

            var assignment = Assign(intVariable1, threeOrDefault);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("i = (j > 1) ? 3 : default(int)");
        }

        [Fact]
        public void ShouldTranslateAnAssignmentResultAssignment()
        {
            var intVariable1 = Variable(typeof(int), "i");
            var intVariable2 = Variable(typeof(int), "j");
            var assignVariable2 = Assign(intVariable2, Constant(1));
            var setVariableOneToAssignmentResult = Assign(intVariable1, assignVariable2);

            var translated = ToReadableString(setVariableOneToAssignmentResult);

            translated.ShouldBe("i = j = 1");
        }

        [Fact]
        public void ShouldTranslateABlockAssignmentResultAssignment()
        {
            var longVariable = Variable(typeof(long), "i");
            var intVariable = Variable(typeof(int), "j");
            var assignInt = Assign(intVariable, Constant(10));
            var castAssignmentResult = Convert(assignInt, typeof(long));
            var assignIntBlock = Block(castAssignmentResult);
            var setLongVariableToAssignmentResult = Assign(longVariable, assignIntBlock);

            var assignmentBlock = Block(
                new[] { longVariable, intVariable },
                setLongVariableToAssignmentResult);

            var translated = ToReadableString(assignmentBlock);

            const string EXPECTED = @"
int j;
var i = (long)(j = 10);";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldAssignTheResultOfATryCatch()
        {
            var intVariable = Variable(typeof(int), "i");

            var assignIntToZero = Assign(intVariable, Constant(0));

            var read = CreateLambda(() => Console.Read());

            var returnDefault = Catch(typeof(IOException), Default(typeof(int)));
            var readOrDefault = TryCatch(read.Body, returnDefault);

            var assignReadOrDefault = Assign(intVariable, readOrDefault);

            var assignmentBlock = Block(new[] { intVariable }, assignIntToZero, assignReadOrDefault);

            var translated = ToReadableString(assignmentBlock);

            const string EXPECTED = @"
var i = 0;
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
};";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldAssignAVariableInAConditionalTest()
        {
            var intVariable = Variable(typeof(int), "i");
            var assignVariable = Assign(intVariable, Constant(10));
            var isAssignmentFive = Equal(assignVariable, Constant(5));
            var ifFiveDoNothing = IfThen(isAssignmentFive, Empty());

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
            var stringVariable = Variable(typeof(string), "value");
            var setStringVariableToNull = Assign(stringVariable, Default(typeof(string)));

            var intVariable = Variable(typeof(int), "i");

            var intToStringMethod = typeof(int)
                .GetMethods()
                .First(m =>
                    (m.Name == "ToString") &&
                    (m.GetParameters().FirstOrDefault()?.ParameterType == typeof(string)));

            var intToString = Call(intVariable, intToStringMethod, setStringVariableToNull);

            var translated = ToReadableString(intToString);

            translated.ShouldBe("i.ToString(value = null)");
        }

        [Fact]
        public void ShouldAssignAMultiplicationToStringResult()
        {
            var timesThreeToString = CreateLambda((int i) => (i * 3).ToString());

            var stringVariable = Variable(typeof(string), "value");
            var stringAssignment = Assign(stringVariable, timesThreeToString.Body);

            var translated = ToReadableString(stringAssignment);

            translated.ShouldBe("value = (i * 3).ToString()");
        }

        [Fact]
        public void ShouldTranslateAMultipleLineTernaryAssignment()
        {
            var consoleRead = CreateLambda(() => Console.Read());

            var variableOne = Variable(typeof(int), "one");
            var variableTwo = Variable(typeof(int), "two");
            var resultVariableOne = Variable(typeof(int), "resultOne");

            var variableOneAssignment = Assign(variableOne, consoleRead.Body);
            var variableTwoAssignment = Assign(variableTwo, consoleRead.Body);

            var variableOneTimesTwo = Multiply(variableOne, variableTwo);
            var resultOneAssignment = Assign(resultVariableOne, variableOneTimesTwo);

            var ifTrueBlock = Block(
                new[] { variableOne, variableTwo, resultVariableOne },
                variableOneAssignment,
                variableTwoAssignment,
                resultOneAssignment,
                resultVariableOne);

            var variableThree = Variable(typeof(int), "three");
            var variableFour = Variable(typeof(int), "four");
            var resultVariableTwo = Variable(typeof(int), "resultTwo");

            var variableThreeAssignment = Assign(variableThree, consoleRead.Body);
            var variableFourAssignment = Assign(variableFour, consoleRead.Body);

            var variableThreeDivideFour = Divide(variableThree, variableFour);
            var resultTwoAssignment = Assign(resultVariableTwo, variableThreeDivideFour);

            var ifFalseBlock = Block(
                new[] { variableThree, variableFour, resultVariableTwo },
                variableThreeAssignment,
                variableFourAssignment,
                resultTwoAssignment,
                resultVariableTwo);

            var dateTimeNow = Property(null, typeof(DateTime), "Now");
            var nowHour = Property(dateTimeNow, "Hour");
            var nowHourModuloTwo = Modulo(nowHour, Constant(2));
            var nowHourIsEven = Equal(nowHourModuloTwo, Constant(0));

            var conditional = Condition(nowHourIsEven, ifTrueBlock, ifFalseBlock);

            var resultVariable = Variable(typeof(int), "result");
            var resultAssignment = Assign(resultVariable, conditional);

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

            var tryParseBlock = Block(new[] { intVariable }, tryParseTernary);
            var tryParseLambda = Lambda<Func<string, int>>(tryParseBlock, stringParameter);

            var selectCall = Call(selectMethod, stringArray, tryParseLambda);

            var linqToArray = CreateLambda((IEnumerable<int> ints) => ints.ToArray());
            var toArrayMethod = ((MethodCallExpression)linqToArray.Body).Method;

            var toArrayCall = Call(toArrayMethod, selectCall);

            var resultVariable = Variable(typeof(IList<int>), "result");
            var assignment = Assign(resultVariable, toArrayCall);
            var assignmentBlock = Block(assignment);

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

            var variableOne = Variable(typeof(int), "one");
            var variableTwo = Variable(typeof(int), "two");

            var variableOneAssignment = Assign(variableOne, consoleRead.Body);
            var variableTwoAssignment = Assign(variableTwo, consoleRead.Body);

            var variableOneMinusTwo = Subtract(variableOne, variableTwo);

            var valueBlock = Block(
                new[] { variableOne, variableTwo },
                variableOneAssignment,
                variableTwoAssignment,
                variableOneMinusTwo);

            var wrappingBlock = Block(valueBlock);

            var resultVariable = Variable(typeof(int), "result");
            var resultOneAssignment = Assign(resultVariable, wrappingBlock);

            var translated = ToReadableString(resultOneAssignment);

            const string EXPECTED = @"
result =
{
    var one = Console.Read();
    var two = Console.Read();

    return one - two;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/7
        [Fact]
        public void ShouldTranslateMultiStatementValueBlockAssignments()
        {
            var valueConditional = GetReturnStatementBlock(out var existingInts);

            var consoleRead = CreateLambda(() => Console.Read());

            var multiStatementValueBlock = Block(
                new[] { existingInts },
                consoleRead.Body,
                valueConditional);

            var resultVariable = Variable(multiStatementValueBlock.Type, "result");
            var resultOneAssignment = Assign(resultVariable, multiStatementValueBlock);

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

            var singleStatementValueBlock = Block(
                new[] { existingInts },
                valueConditional);

            var resultVariable = Variable(singleStatementValueBlock.Type, "result");
            var resultOneAssignment = Assign(resultVariable, singleStatementValueBlock);

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
            var objectVariable = Variable(typeof(object), "id");
            var objectValue = Variable(typeof(object), "value");
            var intVariable = Variable(typeof(int), "num");
            var intValue = Variable(typeof(int), "numValue");

            var objectNotNull = NotEqual(objectVariable, Default(typeof(object)));
            var defaultInt = Default(typeof(int));

            var intTryParse = Call(
                typeof(int).GetPublicStaticMethod("TryParse", parameterCount: 2),
                Condition(
                    objectNotNull,
                    Call(objectVariable, typeof(object).GetPublicInstanceMethod("ToString")),
                    Default(typeof(string))),
                intValue);

            var objectAsIntOrDefault = Condition(intTryParse, intValue, defaultInt);

            var intParseInnerBlock = Block(new[] { intValue }, objectAsIntOrDefault);

            var intParseOuterBlock = Block(
                new[] { objectVariable },
                Assign(objectVariable, objectValue),
                intParseInnerBlock);

            var intAssignment = Assign(intVariable, intParseOuterBlock);

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
            var objectVariable = Variable(typeof(object), "id");
            var objectValue = Variable(typeof(object), "value");
            var longVariable = Variable(typeof(long), "number");
            var longValue = Variable(typeof(long), "numberValue");

            var longTryParse = Call(
                null,
                typeof(long).GetPublicStaticMethod("TryParse", parameterCount: 2),
                Call(objectVariable, typeof(object).GetPublicInstanceMethod("ToString")),
                longValue);

            var objectNotNull = NotEqual(objectVariable, Default(typeof(object)));
            var defaultlong = Default(typeof(long));

            var objectAslongOrDefault = Condition(
                objectNotNull,
                Condition(longTryParse, longValue, defaultlong),
                defaultlong);

            var longParseInnerBlock = Block(new[] { longValue }, objectAslongOrDefault);

            var longParseOuterBlock = Block(
                new[] { objectVariable },
                Assign(objectVariable, objectValue),
                longParseInnerBlock);

            var longAssignment = Assign(longVariable, longParseOuterBlock);

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
            var extensionVariable = Variable(value.Type, "ext");
            var assignment = Assign(extensionVariable, value);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("ext = " + value);
        }

        private static Expression GetReturnStatementBlock(out ParameterExpression existingInts)
        {
            existingInts = Variable(typeof(List<int>), "ints");

            var existingIntsEnumerator = Variable(typeof(List<int>.Enumerator), "enumerator");
            var getEnumeratorMethod = existingInts.Type.GetPublicInstanceMethod("GetEnumerator");
            var getEnumeratorCall = Call(existingInts, getEnumeratorMethod);
            var enumeratorAssignment = Assign(existingIntsEnumerator, getEnumeratorCall);

            var enumeratorMoveNextMethod = existingIntsEnumerator.Type.GetPublicInstanceMethod("MoveNext");
            var enumeratorMoveNextCall = Call(existingIntsEnumerator, enumeratorMoveNextMethod);

            var enumeratorItem = Variable(typeof(int), "item");
            var enumeratorCurrent = Property(existingIntsEnumerator, "Current");
            var itemAssignment = Assign(enumeratorItem, enumeratorCurrent);

            var intsAddMethod = existingInts.Type.GetPublicInstanceMethod("Add");
            var intsAddCall = Call(existingInts, intsAddMethod, enumeratorItem);

            var addItemBlock = Block(
                new[] { enumeratorItem },
                itemAssignment,
                intsAddCall);

            var loopBreakTarget = Label(typeof(void), "LoopBreak");

            var conditionallyAddItems = Condition(
                IsTrue(enumeratorMoveNextCall),
                addItemBlock,
                Break(loopBreakTarget));

            var addItemsLoop = Loop(conditionallyAddItems, loopBreakTarget);

            var populateExistingInts = Block(
                new[] { existingIntsEnumerator },
                enumeratorAssignment,
                addItemsLoop);

            var conditionFalseBlock = Block(
                populateExistingInts,
                existingInts);

            var valueConditional = Condition(
                Equal(existingInts, Default(existingInts.Type)),
                New(conditionFalseBlock.Type),
                conditionFalseBlock);

            return valueConditional;
        }
    }
}
