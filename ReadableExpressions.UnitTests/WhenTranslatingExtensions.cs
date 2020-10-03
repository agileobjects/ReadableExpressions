namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using Common;
    using Translations;
    using Translations.Formatting;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingExtensions
    {
        [Fact]
        public void ShouldTranslateAnExtensionExpressionType()
        {
            var extension = new ExtensionExpression();
            var translated = extension.ToReadableString();

            translated.ShouldBe(extension.ToString());
        }

        [Fact]
        public void ShouldTranslateAnUnknownExpressionType()
        {
            var unknown = new UnknownExpression();
            var translated = unknown.ToReadableString();

            translated.ShouldBe(unknown.ToString());
        }

        [Fact]
        public void ShouldTranslateACustomTranslationExpression()
        {
            var custom = new CustomTranslationExpression();
            var translated = custom.ToReadableString();

            translated.ShouldBe(custom.ToString());
        }

        #region Helper Members

        internal class ExtensionExpression : Expression
        {
            public ExtensionExpression(Type type = null)
            {
                Type = type ?? typeof(object);
            }

            public override ExpressionType NodeType => ExpressionType.Extension;

            public override Type Type { get; }

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                // The default implementation of VisitChildren falls over 
                // if the Expression is not reducible. Short-circuit that 
                // with this:
                return this;
            }

            public override string ToString() => "Exteeennndddiiiinnngg";
        }

        internal class UnknownExpression : Expression
        {
            public override ExpressionType NodeType => (ExpressionType)5346372;

            public override Type Type => typeof(void);

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                // See ExtensionExpression for why this is necessary:
                return this;
            }

            public override string ToString()
            {
                return "You can't know me!";
            }
        }

        internal class CustomTranslationExpression : Expression, ICustomTranslationExpression
        {
            public override ExpressionType NodeType => ExpressionType.Extension;

            public override Type Type => typeof(void);

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                // See ExtensionExpression for why this is necessary:
                return this;
            }

            public ITranslation GetTranslation(ITranslationContext context)
            {
                return new FixedValueTranslation(
                    NodeType,
                    ToString(),
                    Type,
                    TokenType.Default,
                    context);
            }

            public override string ToString() => "Customiiiiiize";
        }

        #endregion
    }
}
