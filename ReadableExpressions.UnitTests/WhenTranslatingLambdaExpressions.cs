namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingLambdaExpressions
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
    }
}
