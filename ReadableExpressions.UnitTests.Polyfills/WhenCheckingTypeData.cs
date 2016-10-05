namespace AgileObjects.ReadableExpressions.UnitTests.Polyfills
{
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenCheckingTypeData
    {
        [TestMethod]
        public void ShouldFlagAParamsArray()
        {
            var paramsParameter = typeof(TestHelper)
                .GetMethod("DoParamsStuff")
                .GetParameters()
                .First();


            Assert.IsTrue(paramsParameter.IsParamsArray());
        }

        [TestMethod]
        public void ShouldFlagANonParamsArray()
        {
            var paramsParameter = typeof(TestHelper)
                .GetMethod("DoNonParamsStuff")
                .GetParameters()
                .First();


            Assert.IsFalse(paramsParameter.IsParamsArray());
        }

        [TestMethod]
        public void ShouldFindATypeAttribute()
        {
            Assert.IsTrue(typeof(WhenCheckingTypeData).HasAttribute<TestClassAttribute>());
        }

        [TestMethod]
        public void ShouldNotFindATypeAttribute()
        {
            Assert.IsFalse(typeof(TestHelper).HasAttribute<TestClassAttribute>());
        }

        [TestMethod]
        public void ShouldFindANonPublicInstanceMethod()
        {
            var method = typeof(TestHelper).GetNonPublicInstanceMethods().FirstOrDefault();

            Assert.IsNotNull(method);
            Assert.AreEqual("NonPublicMethod", method.Name);
        }

        [TestMethod]
        public void ShouldFlagAnAnonymousType()
        {
            var anon = new { Value = "Value!" };

            Assert.IsTrue(anon.GetType().IsAnonymous());
        }

        [TestMethod]
        public void ShouldFlagANonAnonymousType()
        {
            Assert.IsFalse(typeof(IOrderedEnumerable<int>).IsAnonymous());
        }

        private class TestHelper
        {
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            public void DoParamsStuff(params int[] ints)
            {
            }

            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once UnusedParameter.Local
            public void DoNonParamsStuff(int[] ints)
            {

            }

            // ReSharper disable once UnusedMember.Local
            internal void NonPublicMethod()
            {
            }
        }
    }
}
