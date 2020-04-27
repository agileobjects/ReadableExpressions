namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Visualizers.Core;
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
    public class WhenFormattingCodeAsHtml : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAParameterlessNewExpression()
        {
            var createObject = CreateLambda(() => new object());

            var translated = ToReadableHtmlString(createObject.Body);

            translated.ShouldBe("<span class=\"kw\">new </span><span class=\"tn\">Object</span>()");
        }

        [Fact]
        public void ShouldTranslateAnAssignment()
        {
            var intVariable = Variable(typeof(int), "i");
            var assignDefaultToInt = Assign(intVariable, Default(typeof(int)));

            var translated = ToReadableHtmlString(assignDefaultToInt);

            translated.ShouldBe(
                "<span class=\"vb\">i</span> = " +
                "<span class=\"kw\">default</span>" +
                "(" +
                "<span class=\"kw\">int</span>" +
                ")");
        }

        [Fact]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            var createList = CreateLambda(() => new List<decimal> { 1m, 2.005m, 3m });

            var translated = ToReadableHtmlString(createList.Body);

            translated.ShouldBe(
                "<span class=\"kw\">new </span>" +
                "<span class=\"tn\">List</span>" +
                "&lt;<span class=\"kw\">decimal</span>&gt; " +
                "{ " +
                "<span class=\"nm\">1</span><span class=\"nm\">m</span>, " +
                "<span class=\"nm\">2.005</span><span class=\"nm\">m</span>, " +
                "<span class=\"nm\">3</span><span class=\"nm\">m</span> " +
                "}");
        }

        [Fact]
        public void ShouldTranslateAConditionalBranchWithAComment()
        {
            var comment = ReadableExpression.Comment("Maths works");
            var one = Constant(1);
            var oneEqualsOne = Equal(one, one);
            var ifOneEqualsOneComment = IfThen(oneEqualsOne, comment);

            var translated = ToReadableHtmlString(ifOneEqualsOneComment);

            const string EXPECTED = @"
<span class=""cs"">if </span>(<span class=""nm"">1</span> == <span class=""nm"">1</span>)
{
    <span class=""cm"">// Maths works</span>
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldAssignTheResultOfATryCatch()
        {
            var intVariable = Variable(typeof(int), "i");

            var assignIntToZero = Assign(intVariable, Constant(0));

            var read = CreateLambda(() => Console.Read());

            var returnDefault = Catch(typeof(IOException), Default(typeof(int)));
            var readOrDefault = TryCatch(read.Body, returnDefault);

            var assignReadOrDefault = Assign(intVariable, readOrDefault);

            var assignmentBlock = Block(new[] { intVariable }, assignIntToZero, assignReadOrDefault);

            var translated = ToReadableHtmlString(assignmentBlock);

            const string EXPECTED = @"
<span class=""kw"">var</span> <span class=""vb"">i</span> = <span class=""nm"">0</span>;
<span class=""vb"">i</span> =
{
    <span class=""kw"">try</span>
    {
        <span class=""cs"">return </span><span class=""tn"">Console</span>.<span class=""mn"">Read</span>();
    }
    <span class=""kw"">catch</span> (<span class=""tn"">IOException</span>)
    {
        <span class=""cs"">return </span><span class=""kw"">default</span>(<span class=""kw"">int</span>);
    }
};";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAReturnKeywordForAnObjectInitStatement()
        {
            var exception = Variable(typeof(Exception), "ex");
            var newAddress = New(typeof(Address).GetConstructors().First());
            var line1Property = newAddress.Type.GetMember("Line1").First();
            var line1Value = Constant("Over here");
            var line1Init = Bind(line1Property, line1Value);
            var addressInit = MemberInit(newAddress, line1Init);
            var rethrow = Rethrow(newAddress.Type);
            var globalCatchAndRethrow = Catch(exception, rethrow);
            var tryCatch = TryCatch(addressInit, globalCatchAndRethrow);

            var tryCatchBlock = Block(tryCatch);

            var translated = ToReadableHtmlString(tryCatchBlock);

            const string EXPECTED = @"
<span class=""kw"">try</span>
{
    <span class=""cs"">return </span><span class=""kw"">new </span><span class=""tn"">WhenFormattingCodeAsHtml</span>.<span class=""tn"">Address</span>
    {
        Line1 = <span class=""tx"">""</span><span class=""tx"">Over here</span><span class=""tx"">""</span>
    };
}
<span class=""kw"">catch</span>
{
    <span class=""kw"">throw</span>;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateGotoStatements()
        {
            var labelTargetOne = Label(typeof(void), "One");
            var labelOne = Label(labelTargetOne);
            var writeOne = CreateLambda(() => Console.Write("One"));
            var gotoOne = Goto(labelTargetOne);

            var labelTargetTwo = Label(typeof(void), "Two");
            var labelTwo = Label(labelTargetTwo);
            var writeTwo = CreateLambda(() => Console.Write("Two"));
            var gotoTwo = Goto(labelTargetTwo);

            var intVariable = Variable(typeof(int), "i");
            var intEqualsOne = Equal(intVariable, Constant(1));
            var intEqualsTwo = Equal(intVariable, Constant(2));

            var ifTwoGotoTwo = IfThen(intEqualsTwo, gotoTwo);
            var gotoOneOrTwo = IfThenElse(intEqualsOne, gotoOne, ifTwoGotoTwo);

            var writeNeither = CreateLambda(() => Console.Write("Neither"));
            var returnFromBlock = Return(Label());

            var block = Block(
                gotoOneOrTwo,
                writeNeither.Body,
                returnFromBlock,
                labelOne,
                writeOne.Body,
                labelTwo,
                writeTwo.Body);

            var translated = ToReadableHtmlString(block);

            const string EXPECTED = @"
<span class=""cs"">if </span>(<span class=""vb"">i</span> == <span class=""nm"">1</span>)
{
    <span class=""cs"">goto </span>One;
}
<span class=""cs"">else if </span>(<span class=""vb"">i</span> == <span class=""nm"">2</span>)
{
    <span class=""cs"">goto </span>Two;
}

<span class=""tn"">Console</span>.<span class=""mn"">Write</span>(<span class=""tx"">""</span><span class=""tx"">Neither</span><span class=""tx"">""</span>);
<span class=""cs"">return</span>;

One:
<span class=""tn"">Console</span>.<span class=""mn"">Write</span>(<span class=""tx"">""</span><span class=""tx"">One</span><span class=""tx"">""</span>);

Two:
<span class=""tn"">Console</span>.<span class=""mn"">Write</span>(<span class=""tx"">""</span><span class=""tx"">Two</span><span class=""tx"">""</span>);
";
            translated.ShouldBe(EXPECTED.Trim());
        }

        #region Helper Members

        private static string ToReadableHtmlString(Expression expression)
        {
            return expression.ToReadableString(settings => settings
                .FormatUsing(TranslationHtmlFormatter.Instance));
        }

        private class Address
        {
            // ReSharper disable once UnusedMember.Local
            public string Line1 { get; set; }
        }

        #endregion
    }
}
