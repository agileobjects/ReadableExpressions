namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System.Collections.Generic;
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
    public class WhenFormattingCodeToHtml : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAParameterlessNewExpression()
        {
            var createObject = CreateLambda(() => new object());

            var translated = ToReadableHtmlString(createObject.Body);

            translated.ShouldBe("<span class=\"kw\">new</span> <span class=\"tn\">Object</span>()");
        }

        [Fact]
        public void ShouldTranslateANewListExpressionWithAdditions()
        {
            var createList = CreateLambda(() => new List<decimal> { 1m, 2.005m, 3m });

            var translated = ToReadableHtmlString(createList.Body);

            translated.ShouldBe(
                "<span class=\"kw\">new</span> " +
                "<span class=\"tn\">List</span>" +
                "&lt;<span class=\"kw\">decimal</span>&gt; " +
                "{ " +
                "<span class=\"nm\">1m</span>, " +
                "<span class=\"nm\">2.005m</span>, " +
                "<span class=\"nm\">3m</span> " +
                "}");
        }

        private static string ToReadableHtmlString(Expression expression)
        {
            return expression.ToReadableString(settings => settings
                .FormatUsing(TranslationHtmlFormatter.Instance));
        }
    }
}
