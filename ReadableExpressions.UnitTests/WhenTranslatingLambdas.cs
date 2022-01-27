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

        #region Helper Members

        private static class Issue49
        {
            public class EntityBase { }

            public class DerivedEntity : EntityBase
            {
                public bool Remove(DerivedEntity derived) => derived != null;
            }
        }

        #endregion
    }
}
