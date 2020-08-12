#if FEATURE_BUILD
namespace AgileObjects.ReadableExpressions.UnitTests.Build
{
    using System;
    using System.Reflection;
    using Xunit;
    using static System.Linq.Expressions.Expression;
    using static ReadableExpressions.Build.ReadableSourceCodeExpression;

    public class WhenBuildingSourceCodeIncorrectly
    {
        [Fact]
        public void ShouldErrorIfNoClassesSpecified()
        {
            var classEx = Should.Throw<InvalidOperationException>(() =>
            {
                SourceCode(cfg => cfg
                    .WithNamespaceOf<CommentExpression>())
                    .ToSourceCode();
            });

            classEx.Message.ShouldContain("class must be specified");
        }

        [Fact]
        public void ShouldErrorIfNullClassNameSpecified()
        {
            var classNameEx = Should.Throw<ArgumentException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                SourceCode(cfg => cfg
                    .WithClass(null, cls => cls
                        .WithMethod(doNothing)));
            });

            classNameEx.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void ShouldErrorIfWhitespaceClassNameSpecified()
        {
            var classNameEx = Should.Throw<ArgumentException>(() =>
             {
                 var doNothing = Lambda<Action>(Default(typeof(void)));

                 SourceCode(cfg => cfg
                     .WithClass("   ", cls => cls
                         .WithMethod(doNothing)));
             });

            classNameEx.Message.ShouldContain("cannot be blank");
        }

        [Fact]
        public void ShouldErrorIfInvalidClassNameSpecified()
        {
            var classNameEx = Should.Throw<ArgumentException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                SourceCode(cfg => cfg
                    .WithClass("X-Y-Z", cls => cls
                        .WithMethod(doNothing)));
            });

            classNameEx.Message.ShouldContain("invalid class name");
        }

        [Fact]
        public void ShouldErrorIfDuplicateClassNamesSpecified()
        {
            var configEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                SourceCode(cfg => cfg
                    .WithClass("MyClass", cls => cls
                        .WithMethod(doNothing))
                    .WithClass("MyClass", cls => cls
                        .WithMethod(doNothing)));
            });

            configEx.Message.ShouldContain("Duplicate class name");
            configEx.Message.ShouldContain("MyClass");
        }

        [Fact]
        public void ShouldErrorIfNoMethodsSpecified()
        {
            var methodEx = Should.Throw<InvalidOperationException>(() =>
            {
                SourceCode(cfg => cfg
                    .WithClass(cls => cls))
                    .ToSourceCode();
            });

            methodEx.Message.ShouldContain("method must be specified");
        }

        [Fact]
        public void ShouldErrorIfAmbiguousInterfacesImplemented()
        {
            var interfaceEx = Should.Throw<AmbiguousMatchException>(() =>
            {
                var getString = Lambda<Func<string>>(Default(typeof(string)));

                SourceCode(cfg => cfg
                    .WithClass(cls => cls
                        .Implementing(typeof(IMessager), typeof(IRandomStringFactory))
                        .WithMethod(getString)))
                    .ToSourceCode();
            });

            interfaceEx.Message.ShouldContain("'(): string'");
            interfaceEx.Message.ShouldContain("matches multiple interface methods");
        }

        [Fact]
        public void ShouldErrorIfNullMethodNameSpecified()
        {
            var methodNameEx = Should.Throw<ArgumentException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                SourceCode(cfg => cfg
                    .WithClass(cls => cls
                        .WithMethod(null, doNothing)));
            });

            methodNameEx.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void ShouldErrorIfWhitespaceMethodNameSpecified()
        {
            var methodNameEx = Should.Throw<ArgumentException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                SourceCode(cfg => cfg
                    .WithClass(cls => cls
                        .WithMethod("\t", doNothing)));
            });

            methodNameEx.Message.ShouldContain("cannot be blank");
        }

        [Fact]
        public void ShouldErrorIfInvalidMethodNameSpecified()
        {
            var methodNameEx = Should.Throw<ArgumentException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                SourceCode(cfg => cfg
                    .WithClass(cls => cls
                        .WithMethod(" My_Method", doNothing)));
            });

            methodNameEx.Message.ShouldContain("invalid method name");
        }

        [Fact]
        public void ShouldErrorIfDuplicateMethodNamesSpecified()
        {
            var configEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                SourceCode(cfg => cfg
                    .WithClass(cls => cls
                        .WithMethod("MyMethod", doNothing)
                        .WithMethod("MyMethod", doNothing)));
            });

            configEx.Message.ShouldContain("Duplicate method name");
            configEx.Message.ShouldContain("MyMethod");
        }

        #region Helper Members

        public interface IMessager
        {
            string GetMessage();
        }

        public interface IRandomStringFactory
        {
            string Generate();
        }

        #endregion
    }
}
#endif