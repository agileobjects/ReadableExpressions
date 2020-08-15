﻿namespace AgileObjects.ReadableExpressions.UnitTests.Build.SourceCodeExpressions
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Common;
    using ReadableExpressions.Build;
    using ReadableExpressions.Build.SourceCode;
    using Xunit;

    public class WhenVisitingSourceCodeExpressions : TestClassBase
    {
        [Fact]
        public void ShouldVisitSourceCodeExpressions()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var sourceCode = ReadableSourceCodeExpression.SourceCode(returnOneThousand);

            var visitor = new VisitationHelper();

            visitor.Visit(sourceCode);

            visitor.VisitedExpressions.ShouldContain(returnOneThousand.Body);
            visitor.SourceCodeVisited.ShouldBeTrue();
            visitor.ClassVisited.ShouldBeTrue();
            visitor.MethodVisited.ShouldBeTrue();
        }

        #region Helper Members

        private class VisitationHelper : ExpressionVisitor
        {
            public VisitationHelper()
            {
                VisitedExpressions = new List<Expression>();
            }

            public IList<Expression> VisitedExpressions { get; }

            public bool SourceCodeVisited { get; private set; }

            public bool ClassVisited { get; private set; }

            public bool MethodVisited { get; private set; }

            public override Expression Visit(Expression node)
            {
                VisitedExpressions.Add(node);

                switch (node)
                {
                    case SourceCodeExpression _:
                        SourceCodeVisited = true;
                        break;

                    case ClassExpression _:
                        ClassVisited = true;
                        break;

                    case MethodExpression _:
                        MethodVisited = true;
                        break;
                }

                return base.Visit(node);
            }
        }

        #endregion
    }
}