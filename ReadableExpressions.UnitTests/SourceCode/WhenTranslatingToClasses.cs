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
    public class WhenTranslatingToClasses : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnEmptyParameterlessLambdaActionToAClass()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCodeClass();

            const string EXPECTED = @"
public class GeneratedExpressionClass
{
    public void DoAction()
    {
    }
}";
            EXPECTED.ShouldBeCompilableClass();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateADefaultVoidExpressionToAClass()
        {
            var doNothing = Default(typeof(void));

            var translated = doNothing.ToSourceCodeClass();

            const string EXPECTED = @"
public class GeneratedExpressionClass
{
    public void DoAction()
    {
    }
}";
            EXPECTED.ShouldBeCompilableClass();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeNonScopeVariablesAsMethodParameters()
        {
            var int1Variable = Parameter(typeof(int), "int1");
            var int2Variable = Variable(typeof(int), "int2");
            var addInts = Add(int1Variable, int2Variable);

            var translated = addInts.ToSourceCodeClass();

            const string EXPECTED = @"
public class GeneratedExpressionClass
{
    public int GetInt
    (
        int int1,
        int int2
    )
    {
        return int1 + int2;
    }
}";
            EXPECTED.ShouldBeCompilableClass();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomClassName()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCodeClass(s => s
                .NameClassesUsing(ctx => $"My{ctx.TypeName}Class{ctx.Index}"));

            const string EXPECTED = @"
public class MyVoidClass0
{
    public void DoAction()
    {
    }
}";
            EXPECTED.ShouldBeCompilableClass();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomClassAndMethodName()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCodeClass(s => s
                .NameClassesUsing(ctx => "MySpecialClass")
                .NameMethodsUsing((clsExp, mCtx) =>
                    $"{clsExp.Name}{mCtx.ReturnTypeName}Method_{clsExp.Index + 1}_{mCtx.Index + 1}"));

            const string EXPECTED = @"
public class MySpecialClass
{
    public void MySpecialClassVoidMethod_1_1()
    {
    }
}";
            EXPECTED.ShouldBeCompilableClass();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}