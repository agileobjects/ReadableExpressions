namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // See http://stackoverflow.com/questions/3716492/what-does-expression-quote-do-that-expression-constant-can-t-already-do
    [TestClass]
    public class WhenTranslatingQuotedLambdas
    {
        [TestMethod]
        public void ShouldTranslateAQuotedLambda()
        {
            Expression<Func<int, double>> intToDouble = i => i;

            var quotedLambda = Expression.Quote(intToDouble);

            var translated = quotedLambda.ToReadableString();

            // The only place Quote expressions are used is nested 
            // lambdas, so the translation will be indented:
            const string EXPECTED = @"
    // Quoted to induce a closure:
    i => (double)i";

            Assert.AreEqual(EXPECTED, translated);
        }

        [TestMethod]
        public void ShouldTranslateANestedQuotedLambda()
        {
            var intA = Expression.Parameter(typeof(int), "a");
            var intB = Expression.Parameter(typeof(int), "b");
            var addition = Expression.Add(intA, intB);
            var additionInnerLambda = Expression.Lambda(addition, intB);
            var quotedInnerLambda = Expression.Quote(additionInnerLambda);
            var additionOuterLambda = Expression.Lambda(quotedInnerLambda, intA);

            var translated = additionOuterLambda.ToReadableString();

            const string EXPECTED = @"
a =>
    // Quoted to induce a closure:
    b => a + b";

            Assert.AreEqual(EXPECTED.TrimStart(), translated);
        }
    }
}