namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq;
    using Common;
    using NetStandardPolyfills;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingLambdas : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAParameterlessLambda()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var translated = returnOneThousand.ToReadableString();

            translated.ShouldBe("() => 1000");
        }

        [Fact]
        public void ShouldTranslateASingleParameterLambda()
        {
            var returnArgumentPlusOneTen = CreateLambda((int i) => i + 10);

            var translated = returnArgumentPlusOneTen.ToReadableString();

            translated.ShouldBe("i => i + 10");
        }

        [Fact]
        public void ShouldTranslateAMultipleParameterLambda()
        {
            var convertStringsToInt = CreateLambda((string str1, string str2) => int.Parse(str1) + int.Parse(str2));

            var translated = convertStringsToInt.ToReadableString();

            translated.ShouldBe("(str1, str2) => int.Parse(str1) + int.Parse(str2)");
        }

        [Fact]
        public void ShouldIncludeLambdaParameterTypes()
        {
            var concatStringAndInt = CreateLambda((string strValue, int intValue) => strValue + intValue);

            var translated = concatStringAndInt.ToReadableString(stgs => stgs.ShowLambdaParameterTypes);

            translated.ShouldBe("(string strValue, int intValue) => strValue + intValue");
        }

        [Fact]
        public void ShouldTranslateALambdaInvocation()
        {
            var writeLine = CreateLambda(() => Console.WriteLine());
            var writeLineInvocation = Invoke(writeLine);

            var translated = writeLineInvocation.ToReadableString();

            translated.ShouldBe("(() => Console.WriteLine()).Invoke()");
        }

        // see http://stackoverflow.com/questions/3716492/what-does-expression-quote-do-that-expression-constant-can-t-already-do
        [Fact]
        public void ShouldTranslateAQuotedLambda()
        {
            var intToDouble = CreateLambda((int i) => (double)i);

            var quotedLambda = Quote(intToDouble);

            var translated = quotedLambda.ToReadableString();

            translated.ShouldBe("i => (double)i");
        }

        [Fact]
        public void ShouldTranslateAQuotedLambdaQueryableProjection()
        {
            var project = CreateLambda((IQueryable<int> items) => items.Select(item => new { Number = item }));

            var translated = project.Body.ToReadableString();

            translated.ShouldBe("items.Select(item => new { Number = item })");
        }

        [Fact]
        public void ShouldTranslateANestedQuotedLambda()
        {
            var intA = Parameter(typeof(int), "a");
            var intB = Parameter(typeof(int), "b");
            var addition = Add(intA, intB);
            var additionInnerLambda = Lambda(addition, intB);
            var quotedInnerLambda = Quote(additionInnerLambda);
            var additionOuterLambda = Lambda(quotedInnerLambda, intA);

            var translated = additionOuterLambda.ToReadableString();

            translated.ShouldBe("a => b => a + b");
        }

        [Fact]
        public void ShouldTranslateQuotedLambdaWithAnAnnotation()
        {
            var intToDouble = CreateLambda((int i) => (double)i);

            var quotedLambda = Quote(intToDouble);

            var translated = quotedLambda.ToReadableString(stgs =>
                stgs.ShowQuotedLambdaComments);

            const string EXPECTED = @"
// Quoted to induce a closure:
i => (double)i";

            translated.ShouldBe(EXPECTED);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/31
        [Fact]
        public void ShouldTranslateUnnamedLambdaParameters()
        {
            var stringsParameter = Parameter(typeof(string[]));
            var linqSelect = CreateLambda((string[] ints) => ints.Select(int.Parse));
            var linqSelectWithUnnamed = Lambda(linqSelect.Body, stringsParameter);
            var quoted = Quote(linqSelectWithUnnamed);

            var translated = quoted.ToReadableString();

            translated.ShouldBe("stringArray => ints.Select(int.Parse)");
        }

        [Fact]
        public void ShouldTranslateRuntimeVariables()
        {
            var intVariable1 = Variable(typeof(int), "i1");
            var intVariable2 = Variable(typeof(int), "i2");
            var runtimeVariables = RuntimeVariables(intVariable1, intVariable2);

            var translated = runtimeVariables.ToReadableString();

            translated.ShouldBe("(i1, i2)");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/49
        [Fact]
        public void ShouldTranslateWithoutError()
        {
            var parentEntity = Parameter(typeof(Issue49.EntityBase), "parentEntity");
            var entityExpression = Parameter(typeof(Issue49.EntityBase), "entity");

            var sourceLambda = Lambda<Func<Issue49.EntityBase, Issue49.EntityBase, bool>>(
                Call(
                    Convert(parentEntity, typeof(Issue49.DerivedEntity)),
                    typeof(Issue49.DerivedEntity).GetPublicInstanceMethod(nameof(Issue49.DerivedEntity.Remove)),
                    Convert(parentEntity, typeof(Issue49.DerivedEntity))),
                parentEntity,
                entityExpression);

            sourceLambda.ToReadableString();
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/106
        [Fact]
        public void ShouldTranslateADelegateLambda()
        {
            var boolParameter = Parameter(typeof(bool), "regBool");
            var boolAssignment1 = Assign(boolParameter, Constant(false));
            var boolAssignment2 = Assign(boolParameter, Constant(true));

            var delegateLambda = Lambda<Issue106.RegularDelegate>(
                Block(boolAssignment1, boolAssignment2),
                boolParameter);

            const string EXPECTED = @"
regBool =>
{
    regBool = false;
    regBool = true;
}";
            var translated = delegateLambda.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/106
        [Fact]
        public void ShouldIncludeAnOutParameterKeyword()
        {
            var boolParameter = Parameter(typeof(bool).MakeByRefType(), "outBool");
            var boolAssignment1 = Assign(boolParameter, Constant(false));
            var boolAssignment2 = Assign(boolParameter, Constant(true));

            var delegateLambda = Lambda<Issue106.OutDelegate>(
                Block(boolAssignment1, boolAssignment2),
                boolParameter);

            const string EXPECTED = @"
(out bool outBool) =>
{
    outBool = false;
    outBool = true;
}";
            var translated = delegateLambda.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/106
        [Fact]
        public void ShouldIncludeARefParameterKeyword()
        {
            var boolParameter = Parameter(typeof(bool).MakeByRefType(), "refBool");
            var boolAssignment1 = Assign(boolParameter, Constant(false));
            var boolAssignment2 = Assign(boolParameter, Constant(true));

            var delegateLambda = Lambda<Issue106.RefDelegate>(
                Block(boolAssignment1, boolAssignment2),
                boolParameter);

            const string EXPECTED = @"
(ref bool refBool) =>
{
    refBool = false;
    refBool = true;
}";
            var translated = delegateLambda.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/106
        [Fact]
        public void ShouldTranslateAParamsParameterDelegate()
        {
            var boolsParameter = Parameter(typeof(bool[]), "bools");
            var zeroethBool = ArrayAccess(boolsParameter, Constant(0));
            var boolAssignment1 = Assign(zeroethBool, Constant(false));
            var boolAssignment2 = Assign(zeroethBool, Constant(true));

            var delegateLambda = Lambda<Issue106.ParamsDelegate>(
                Block(boolAssignment1, boolAssignment2),
                boolsParameter);

            const string EXPECTED = @"
bools =>
{
    bools[0] = false;
    bools[0] = true;
}";
            var translated = delegateLambda.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/106
        [Fact]
        public void ShouldTranslateARegularParameterAndParamsParameterDelegate()
        {
            var intParameter = Parameter(typeof(int), "regInt");
            var boolsParameter = Parameter(typeof(bool[]), "bools");
            var zeroethBool = ArrayAccess(boolsParameter, Constant(0));
            var boolAssignment1 = Assign(zeroethBool, Constant(false));
            var boolAssignment2 = Assign(zeroethBool, Constant(true));

            var delegateLambda = Lambda<Issue106.RegAndParamsDelegate>(
                Block(boolAssignment1, boolAssignment2),
                intParameter,
                boolsParameter);

            const string EXPECTED = @"
(regInt, bools) =>
{
    bools[0] = false;
    bools[0] = true;
}";
            var translated = delegateLambda.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/106
        [Fact]
        public void ShouldTranslateAnOutParameterAndParamsParameterDelegate()
        {
            var intParameter = Parameter(typeof(int).MakeByRefType(), "outInt");
            var boolsParameter = Parameter(typeof(bool[]), "bools");
            var zeroethBool = ArrayAccess(boolsParameter, Constant(0));
            var boolAssignment1 = Assign(zeroethBool, Constant(false));
            var boolAssignment2 = Assign(zeroethBool, Constant(true));

            var delegateLambda = Lambda<Issue106.OutAndParamsDelegate>(
                Block(boolAssignment1, boolAssignment2),
                intParameter,
                boolsParameter);

            const string EXPECTED = @"
(out int outInt, bool[] bools) =>
{
    bools[0] = false;
    bools[0] = true;
}";
            var translated = delegateLambda.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/106
        [Fact]
        public void ShouldTranslateAMultiParameterDelegate()
        {
            var boolParameter = Parameter(typeof(bool), "regBool");
            var refBoolParameter = Parameter(typeof(bool).MakeByRefType(), "refBool");
            var outBoolParameter = Parameter(typeof(bool).MakeByRefType(), "outBool");
            
            var outBoolAssignment1 = Assign(outBoolParameter, Constant(false));
            var outBoolAssignment2 = Assign(outBoolParameter, Constant(true));

            var delegateLambda = Lambda<Issue106.MixedDelegate>(
                Block(outBoolAssignment1, outBoolAssignment2),
                boolParameter,
                refBoolParameter,
                outBoolParameter);

            const string EXPECTED = @"
(bool regBool, ref bool refBool, out bool outBool) =>
{
    outBool = false;
    outBool = true;
}";
            var translated = delegateLambda.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        #region Helper Members

        private static class Issue49
        {
            public class EntityBase { }

            public class DerivedEntity : EntityBase
            {
                public bool Remove(DerivedEntity derived) => derived != null;
            }
        }

        private static class Issue106
        {
            public delegate void OutDelegate(out bool outBool);
            public delegate void RefDelegate(ref bool refBool);
            public delegate void RegularDelegate(bool regBool);
            public delegate void ParamsDelegate(params bool[] boolParams);
            public delegate void RegAndParamsDelegate(int regInt, params bool[] boolParams);
            public delegate void OutAndParamsDelegate(out int outInt, params bool[] boolParams);
            public delegate void MixedDelegate(bool regBool, ref bool refBool, out bool outBool);
        }

        #endregion
    }
}
