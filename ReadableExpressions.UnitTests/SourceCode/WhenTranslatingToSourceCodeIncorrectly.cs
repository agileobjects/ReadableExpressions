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
    public class WhenTranslatingToSourceCodeIncorrectly
    {
        [Fact]
        public void ShouldErrorIfNullCustomClassName()
        {
            var classNameEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                doNothing.ToSourceCode(s => s
                    .NameClassesUsing((sc, ctx) => null));
            });

            classNameEx.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void ShouldErrorIfBlankCustomClassName()
        {
            var classNameEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                doNothing.ToSourceCode(s => s
                    .NameClassesUsing((sc, ctx) => string.Empty));
            });

            classNameEx.Message.ShouldContain("cannot be blank");
        }

        [Fact]
        public void ShouldErrorIfInvalidCustomClassName()
        {
            var classNameEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                doNothing.ToSourceCode(s => s
                    .NameClassesUsing((sc, ctx) => $"1_Class_{ctx.Index}"));
            });

            classNameEx.Message.ShouldContain("invalid class name");
        }

        [Fact]
        public void ShouldErrorIfNullCustomMethodName()
        {
            var methodNameEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                doNothing.ToSourceCode(s => s
                    .NameMethodsUsing((sc, cls, ctx) => null));
            });

            methodNameEx.Message.ShouldContain("cannot be null");
        }

        [Fact]
        public void ShouldErrorIfBlankCustomMethodName()
        {
            var methodNameEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                doNothing.ToSourceCode(s => s
                    .NameMethodsUsing((sc, cls, ctx) => string.Empty));
            });

            methodNameEx.Message.ShouldContain("cannot be blank");
        }

        [Fact]
        public void ShouldErrorIfInvalidCustomMethodName()
        {
            var methodNameEx = Should.Throw<InvalidOperationException>(() =>
            {
                var doNothing = Lambda<Action>(Default(typeof(void)));

                doNothing.ToSourceCode(s => s
                    .NameMethodsUsing((sc, cls, ctx) => $"Method {ctx.Index}"));
            });

            methodNameEx.Message.ShouldContain("invalid method name");
        }

        [Fact]
        public void ShouldErrorIfCommentAndLambdaBlockHasInvalidOrder()
        {
            var invalidBlockEx = Should.Throw<NotSupportedException>(() =>
            {
                var doNothingComment = Comment("NOTHIIIING");
                var doNothing = Lambda<Action>(Default(typeof(void)));

                var block = Block(
                    doNothingComment,
                    doNothingComment,
                    doNothing);

                block.ToSourceCode();
            });

            invalidBlockEx.Message.ShouldContain("LambdaExpressions with optional Comments");
        }
    }
}
