namespace AgileObjects.ReadableExpressions.UnitTests;

using System;
using System.Collections.Generic;
using ReadableExpressions.Translations;
using ReadableExpressions.Translations.Formatting;

#if NET35
[NUnitTestFixture]
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
    public void ShouldTranslateANullExpressionString()
    {
        var nullToString = new NullToStringExpression();
        var translated = nullToString.ToReadableString();

        translated.ShouldBeNull();
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

        analysis.EnterScope(variable1Block);
        analysis.ShouldBeDeclaredInVariableList(intVariable1).ShouldBeFalse();
        analysis.ExitScope();

        analysis.EnterScope(variable2Block);
        analysis.ShouldBeDeclaredInVariableList(intVariable2).ShouldBeFalse();
        analysis.ExitScope();
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

    internal class NullToStringExpression : Expression
    {
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(string);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // See ExtensionExpression for why this is necessary:
            return this;
        }

        public override string ToString() => null!;
    }

    internal class CustomTranslationExpression :
        Expression,
        ICustomTranslationExpression
    {
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => typeof(void);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // See ExtensionExpression for why this is necessary:
            return this;
        }

        public INodeTranslation GetTranslation(ITranslationContext context)
        {
            return new FixedValueTranslation(
                NodeType,
                ToString(),
                TokenType.Default);
        }

        public override string ToString() => "Customiiiiiize";
    }

    internal class TestCustomAnalysableExpression :
        Expression,
        ICustomAnalysableExpression
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

    #endregion
}