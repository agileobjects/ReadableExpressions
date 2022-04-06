namespace AgileObjects.ReadableExpressions.UnitTests.Translations
{
    using System.Linq;
    using Common;
    using ReadableExpressions.Translations;
    using ReadableExpressions.Translations.Reflection;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingMethodCalls : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnInstanceCallExpression()
        {
            var objectToString = CreateLambda((object o) => o.ToString());
            var toStringCall = (MethodCallExpression)objectToString.Body;
            var context = new TestTranslationContext(toStringCall);
            var toStringMethod = new ClrMethodWrapper(toStringCall.Method, context);

            var translation = MethodCallTranslation
                .For(toStringMethod, toStringCall.Arguments, context);

            var translated = new TestTranslationWriter(translation).GetContent();

            translated.ShouldBe("this.ToString()");
        }

        [Fact]
        public void ShouldTranslateAStaticCallExpression()
        {
            var oneEqualsTwo = CreateLambda(() => ReferenceEquals("1", "2"));
            var referenceEqualsCall = (MethodCallExpression)oneEqualsTwo.Body;
            var context = new TestTranslationContext(referenceEqualsCall);
            var referenceEqualsMethod = new ClrMethodWrapper(referenceEqualsCall.Method, context);

            var translation = MethodCallTranslation
                .For(referenceEqualsMethod, referenceEqualsCall.Arguments, context);

            var translated = new TestTranslationWriter(translation).GetContent();

            translated.ShouldBe("object.ReferenceEquals(\"1\", \"2\")");
        }

        [Fact]
        public void ShouldTranslateAnExtensionMethodCall()
        {
            var arrayIsEmpty = CreateLambda((string[] a) => a.Any());
            var linqAnyCall = (MethodCallExpression)arrayIsEmpty.Body;
            var context = new TestTranslationContext(linqAnyCall);
            var linqAnyMethod = new ClrMethodWrapper(linqAnyCall.Method, context);

            var translation = MethodCallTranslation
                .For(linqAnyMethod, linqAnyCall.Arguments, context);

            var translated = new TestTranslationWriter(translation).GetContent();

            translated.ShouldBe("this.Any()");
        }
    }
}
