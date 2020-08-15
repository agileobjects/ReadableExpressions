namespace AgileObjects.ReadableExpressions.UnitTests.Build.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using NetStandardPolyfills;
    using ReadableExpressions.Build;
    using ReadableExpressions.Build.Compilation;
    using ReadableExpressions.Build.SourceCode;
    using Xunit;

    public abstract class CompilerTestsBase
    {
        [Fact]
        public void ShouldCompileSimpleSourceCode()
        {
            var compiler = CreateCompiler();

            const string SOURCE = @"
namespace MyNamespace
{
    public class MyClass
    {
        public string SayHello()
        {
            return ""Hello!"";
        }
    }
}";
            var result = compiler.Compile(SOURCE, Array.Empty<Type>());

            var compiledAssembly = result
                .ShouldNotBeNull()
                .CompiledAssembly
                .ShouldNotBeNull();

            var testType = compiledAssembly.GetType("MyNamespace.MyClass").ShouldNotBeNull();
            var testInstance = Activator.CreateInstance(testType).ShouldNotBeNull();
            var testMethod = testType.GetPublicInstanceMethod("SayHello").ShouldNotBeNull();
            testMethod.Invoke(testInstance, Array.Empty<object>()).ShouldBe("Hello!");
        }

        [Fact]
        public void ShouldCompileSourceCodeWithPassedInDependencies()
        {
            var compiler = CreateCompiler();

            const string SOURCE = @"
namespace MyNamespace
{
    using System.Collections.Generic;
    using System.Linq;

    public static class MyClass
    {
        public static IEnumerable<int> GetInts()
        {
            return new[] { 1, 2, 3 }.ToList();
        }
    }
}";
            var result = compiler.Compile(SOURCE, new[] { typeof(Enumerable) });

            var compiledAssembly = result
                .ShouldNotBeNull()
                .CompiledAssembly
                .ShouldNotBeNull();

            var testType = compiledAssembly.GetType("MyNamespace.MyClass").ShouldNotBeNull();
            var testMethod = testType.GetPublicStaticMethod("GetInts").ShouldNotBeNull();

            testMethod
                .Invoke(null, Array.Empty<object>())
                .ShouldNotBeNull()
                .ShouldBeOfType<IEnumerable<int>>()
                .ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldCompileSourceCodeExpressionSourceCode()
        {
            var compiler = CreateCompiler();

            const string SOURCE = @"
namespace MyNamespace
{
    using System;
    using System.Linq.Expressions;
    using AgileObjects.ReadableExpressions.Build;
    using AgileObjects.ReadableExpressions.Build.SourceCode;

    /// <summary>
    /// Supplies an input <see cref=""SourceCodeExpression""/> to compile to source code.
    /// </summary>
    public static class ExpressionBuilder
    {
        public static SourceCodeExpression Build()
        {
            var doNothing = Expression.Lambda<Action>(Expression.Default(typeof(void)));

            return ReadableSourceCodeExpression
                .SourceCode(sc => sc
                    .WithClass(""MyClass"", cls => cls
                        .WithMethod(""DoNothing"", doNothing)));
        }
    }
}
    ";

            var result = compiler.Compile(SOURCE, new[]
            {
                typeof(Enumerable),
                typeof(ReadableExpression),
                typeof(ReadableSourceCodeExpression)
            });

            var compiledAssembly = result
                .ShouldNotBeNull()
                .CompiledAssembly
                .ShouldNotBeNull();

            var testType = compiledAssembly.GetType("MyNamespace.ExpressionBuilder").ShouldNotBeNull();
            var testMethod = testType.GetPublicStaticMethod("Build").ShouldNotBeNull();

            var sourceCodeExpression = testMethod
                .Invoke(null, Array.Empty<object>())
                .ShouldNotBeNull()
                .ShouldBeOfType<SourceCodeExpression>();

            var classExpression = sourceCodeExpression.Classes.ShouldHaveSingleItem();
            classExpression.Name.ShouldBe("MyClass");

            var methodExpression = classExpression.Methods.ShouldHaveSingleItem();
            methodExpression.ReturnType.ShouldBe(typeof(void));
            methodExpression.Name.ShouldBe("DoNothing");
            methodExpression.Parameters.ShouldBeEmpty();
        }

        #region Helper Members

        internal abstract ICompiler CreateCompiler();

        public class TestClass
        {
        }

        #endregion
    }
}