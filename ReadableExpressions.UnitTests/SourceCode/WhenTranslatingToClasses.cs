namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
{
    using System;
    using ReadableExpressions.Extensions;
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
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomClassName()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCodeClass(s => s.NameClassesUsing(ctx =>
                $"My{ctx.Type.GetVariableNameInPascalCase()}Class{ctx.Index}"));

            const string EXPECTED = @"
public class MyVoidClass0
{
    public void DoAction()
    {
    }
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}