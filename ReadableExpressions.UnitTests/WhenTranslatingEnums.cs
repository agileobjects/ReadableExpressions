namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingEnums
    {
        [TestMethod]
        public void ShouldTranslateAnEnumMember()
        {
            Expression<Func<OddNumber>> getOddNumber = () => OddNumber.One;

            var translated = getOddNumber.Body.ToReadableString();

            Assert.AreEqual("OddNumber.One", translated);
        }

        private enum OddNumber
        {
            One = 1
        }
    }
}
