namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
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
    public class WhenTranslatingToMethods : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnEmptyParameterlessLambdaActionToAMethod()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCodeMethod();

            const string EXPECTED = @"
public void DoAction()
{
}";
            EXPECTED.ShouldBeCompilableMethod();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateADefaultVoidExpressionToAMethod()
        {
            var doNothing = Default(typeof(void));

            var translated = doNothing.ToSourceCodeMethod();

            const string EXPECTED = @"
public void DoAction()
{
}";
            EXPECTED.ShouldBeCompilableMethod();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeNonScopeVariablesAsParameters()
        {
            var intParameter = Parameter(typeof(int), "scopedInt");
            var intVariable = Variable(typeof(int), "nonScopedInt");
            var addInts = Lambda<Func<int, int>>(Add(intParameter, intVariable), intParameter);

            var translated = addInts.ToSourceCodeMethod();

            const string EXPECTED = @"
public int GetInt
(
    int scopedInt,
    int nonScopedInt
)
{
    return scopedInt + nonScopedInt;
}";
            EXPECTED.ShouldBeCompilableMethod();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldRecogniseBlockVariablesAsInScope()
        {
            var intParameter = Parameter(typeof(int), "scopedInt");
            var intVariable = Variable(typeof(int), "blockScopedInt");
            var assignBlockInt = Assign(intVariable, Constant(1));
            var addInts = Add(intVariable, intParameter);
            var block = Block(new[] { intVariable }, assignBlockInt, addInts);
            var addIntsLambda = Lambda<Func<int, int>>(block, intParameter);

            var translated = addIntsLambda.ToSourceCodeMethod();

            const string EXPECTED = @"
public int GetInt
(
    int scopedInt
)
{
    var blockScopedInt = 1;

    return blockScopedInt + scopedInt;
}";
            EXPECTED.ShouldBeCompilableMethod();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomMethodName()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCodeMethod(s => s
                .NameMethodsUsing(ctx => "MagicMethod" + ctx.Index));

            const string EXPECTED = @"
public void MagicMethod0()
{
}";
            EXPECTED.ShouldBeCompilableMethod();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}