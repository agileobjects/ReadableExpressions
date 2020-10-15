﻿namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Translations;
    using Translations.Formatting;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

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

        [Fact]
        public void ShouldAnalyseACustomAnalysableExpression()
        {
            var intVariable1 = Variable(typeof(int), "i");
            var assignVariable1 = Assign(intVariable1, Constant(1));
            var variable1Block = Block(new[] { intVariable1 }, assignVariable1);

            var intVariable2 = Variable(typeof(int), "j");
            var assignVariable2 = Assign(intVariable2, Constant(2));
            var variable2Block = Block(new[] { intVariable2 }, assignVariable2);

            var container = new TestCustomAnalysableExpression(variable1Block, variable2Block);

            var analysis = ExpressionAnalysis.For(container, new TestTranslationSettings());

            analysis.JoinedAssignmentVariables.ShouldBe(intVariable1, intVariable2);
        }

        [Fact]
        public void ShouldTranslateACustomDefaultableExpression()
        {
            var nullableClassExpression = new TestCustomDefaultableExpression(
                typeof(object),
                allowNullKeyword: true);

            nullableClassExpression.ToReadableString().ShouldBe("null");

            var nonNullableClassExpression = new TestCustomDefaultableExpression(
                typeof(object),
                allowNullKeyword: false);

            nonNullableClassExpression.ToReadableString().ShouldBe("default(object)");

            var nullableValueTypeExpression = new TestCustomDefaultableExpression(
                typeof(DateTime),
                allowNullKeyword: true);

            nullableValueTypeExpression.ToReadableString().ShouldBe("default(DateTime)");

            var nonNullableValueTypeExpression = new TestCustomDefaultableExpression(
                typeof(ValueType),
                allowNullKeyword: false);

            nonNullableValueTypeExpression.ToReadableString().ShouldBe("default(ValueType)");
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

        internal class TestCustomAnalysableExpression : Expression, ICustomAnalysableExpression
        {
            public TestCustomAnalysableExpression(params Expression[] expressions)
            {
                Expressions = expressions;
            }

            public override ExpressionType NodeType => ExpressionType.Extension;

            public override Type Type => typeof(void);

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                // See ExtensionExpression for why this is necessary:
                return this;
            }

            public IEnumerable<Expression> Expressions { get; }
        }

        internal class TestCustomDefaultableExpression : Expression, ICustomDefaultableExpression
        {
            public TestCustomDefaultableExpression(
                Type type,
                bool allowNullKeyword)
            {
                Type = type;
                AllowNullKeyword = allowNullKeyword;
            }

            public override ExpressionType NodeType => ExpressionType.Default;

            public override Type Type { get; }

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                // See ExtensionExpression for why this is necessary:
                return this;
            }

            public bool AllowNullKeyword { get; }
        }

        private class TestTranslationSettings : TranslationSettings
        {
        }

        #endregion
    }
}
