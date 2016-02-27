namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingLambdas
    {
        [TestMethod]
        public void ShouldTranslateAParameterlessLambda()
        {
            Expression<Func<int>> returnOneThousand = () => 1000;

            var translated = returnOneThousand.ToReadableString();

            Assert.AreEqual("() => 1000", translated);
        }

        [TestMethod]
        public void ShouldTranslateASingleParameterLambda()
        {
            Expression<Func<int, int>> returnArgumentPlusOneTen = i => i + 10;

            var translated = returnArgumentPlusOneTen.ToReadableString();

            Assert.AreEqual("i => i + 10", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMultipleParameterLambda()
        {
            Expression<Func<string, string, int>> convertStringsToInt = (str1, str2) => int.Parse(str1) + int.Parse(str2);

            var translated = convertStringsToInt.ToReadableString();

            Assert.AreEqual("(str1, str2) => Int32.Parse(str1) + Int32.Parse(str2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateALambdaInvocation()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            var writeLineInvocation = Expression.Invoke(writeLine);

            var translated = writeLineInvocation.ToReadableString();

            Assert.AreEqual("(() => Console.WriteLine()).Invoke()", translated);
        }

        // see http://stackoverflow.com/questions/3716492/what-does-expression-quote-do-that-expression-constant-can-t-already-do
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
