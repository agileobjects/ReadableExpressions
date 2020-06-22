namespace AgileObjects.ReadableExpressions.UnitTests.SourceCode
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenVisitingSourceCodeExpressions : TestClassBase
    {
        [Fact]
        public void ShouldVisitASourceCodeExpression()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var sourceCode = ReadableExpression.SourceCode(returnOneThousand);

            var visitor = new VisitationHelper();

            visitor.Visit(sourceCode);

            visitor.VisitedExpressions.ShouldContain(returnOneThousand.Body);
        }

        [Fact]
        public void ShouldVisitAClassExpression()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var @class = ReadableExpression.Class(returnOneThousand);

            var visitor = new VisitationHelper();

            visitor.Visit(@class);

            visitor.VisitedExpressions.ShouldContain(returnOneThousand.Body);
        }

        [Fact]
        public void ShouldVisitAMethodExpression()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var @class = ReadableExpression.Method(returnOneThousand);

            var visitor = new VisitationHelper();

            visitor.Visit(@class);

            visitor.VisitedExpressions.ShouldContain(returnOneThousand.Body);
        }

        #region Helper Members

        private class VisitationHelper : ExpressionVisitor
        {
            public VisitationHelper()
            {
                VisitedExpressions = new List<Expression>();
            }

            public IList<Expression> VisitedExpressions { get; }

            public override Expression Visit(Expression node)
            {
                VisitedExpressions.Add(node);

                return base.Visit(node);
            }
        }

        #endregion
    }
}