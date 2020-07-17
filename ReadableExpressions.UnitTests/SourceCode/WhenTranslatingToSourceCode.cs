namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
{
    using System;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
    using static ReadableExpression;
#else
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;
    using static ReadableExpression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingToSourceCode : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnEmptyParameterlessLambdaActionToASourceCodeMethod()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public void DoAction()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateADefaultVoidExpressionToASourceCodeMethod()
        {
            var doNothing = Default(typeof(void));

            var translated = doNothing.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public void DoAction()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAParameterlessLambdaFuncToASourceCodeMethod()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var translated = returnOneThousand.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt()
        {
            return 1000;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateASingleParameterLambdaFuncToASourceCodeMethod()
        {
            var returnGivenLong = CreateLambda((long arg) => arg);

            var translated = returnGivenLong.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public long GetLong
        (
            long arg
        )
        {
            return arg;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATwoParameterLambdaActionToASourceCodeMethod()
        {
            var subtractShortFromInt = CreateLambda((int value1, short value2) => value1 - value2);

            var translated = subtractShortFromInt.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt
        (
            int value1,
            short value2
        )
        {
            return value1 - ((int)value2);
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeNonScopeVariablesAsMethodParameters()
        {
            var int1Variable = Parameter(typeof(int), "int1");
            var int2Variable = Variable(typeof(int), "int2");
            var addInts = Add(int1Variable, int2Variable);

            var translated = addInts.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
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
    }
}";
            EXPECTED.ShouldBeCompilableClass();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomNamespace()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCode(s => s
                .WithNamespace("AgileObjects.GeneratedStuff"));

            const string EXPECTED = @"
namespace AgileObjects.GeneratedStuff
{
    public class GeneratedExpressionClass
    {
        public void DoAction()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomTypeNamespace()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCode(s => s
                .WithNamespaceOf<WhenTranslatingToSourceCode>());

            var expected = @$"
namespace {typeof(WhenTranslatingToSourceCode).Namespace}
{{
    public class GeneratedExpressionClass
    {{
        public void DoAction()
        {{
        }}
    }}
}}";
            expected.ShouldCompile();
            translated.ShouldBe(expected.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomClassName()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCode(s => s
                .NameClassesUsing((sc, ctx) => $"My{ctx.TypeName}Class_{ctx.Index}"));

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class MyVoidClass_0
    {
        public void DoAction()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomMethodName()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCode(s => s
                .NameMethodsUsing((sc, cls, ctx) => $"Method_{cls.Index}_{ctx.Index}"));

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public void Method_0_0()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldUseACustomClassAndMethodName()
        {
            var doNothing = Lambda<Action>(Default(typeof(void)));

            var translated = doNothing.ToSourceCode(s => s
                .NameClassesUsing(ctx => "MySpecialClass")
                .NameMethodsUsing((clsExp, mCtx) =>
                    $"{clsExp.Name}{mCtx.ReturnTypeName}Method_{clsExp.Index + 1}_{mCtx.Index + 1}"));

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class MySpecialClass
    {
        public void MySpecialClassVoidMethod_1_1()
        {
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATwoLambdaBlockToTwoSingleMethodClasses()
        {
            var getDefaultInt = Lambda<Func<int>>(Default(typeof(int)));
            var getDefaultString = Lambda<Func<string>>(Default(typeof(string)));
            var block = Block(getDefaultInt, getDefaultString);

            var translated = block.ToSourceCode();

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass1
    {
        public int GetInt()
        {
            return default(int);
        }
    }

    public class GeneratedExpressionClass2
    {
        public string GetString()
        {
            return null;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateATwoLambdaBlockToASingleTwoMethodClass()
        {
            var getDefaultInt = Lambda<Func<int>>(Default(typeof(int)));
            var getDefaultString = Lambda<Func<string>>(Default(typeof(string)));
            var block = Block(getDefaultInt, getDefaultString);

            var translated = block.ToSourceCode(s => s.CreateSingleClass);

            const string EXPECTED = @"
namespace GeneratedExpressionCode
{
    public class GeneratedExpressionClass
    {
        public int GetInt()
        {
            return default(int);
        }

        public string GetString()
        {
            return null;
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateACommentAndLambdaBlockToACommentedSingleMethodClass()
        {
            var classComment = Comment("This is my generated class and I LOVE IT");
            var getDefaultDateTime = Lambda<Func<DateTime>>(Default(typeof(DateTime)));
            var block = Block(classComment, getDefaultDateTime);

            var translated = block.ToSourceCode();

            const string EXPECTED = @"
using System;

namespace GeneratedExpressionCode
{
    /// <summary>
    /// This is my generated class and I LOVE IT
    /// </summary>
    public class GeneratedExpressionClass
    {
        public DateTime GetDateTime()
        {
            return default(DateTime);
        }
    }
}";
            EXPECTED.ShouldCompile();
            translated.ShouldBe(EXPECTED.TrimStart());
        }
    }
}
