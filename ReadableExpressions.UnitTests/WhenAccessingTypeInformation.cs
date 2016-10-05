namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenAccessingTypeInformation
    {
        [TestMethod]
        public void ShouldEvaluateAComplexTypeAsPotentiallyNull()
        {
            Assert.IsTrue(typeof(object).CanBeNull());
        }

        [TestMethod]
        public void ShouldEvaluateAnArrayAsPotentiallyNull()
        {
            Assert.IsTrue(typeof(string[]).CanBeNull());
        }

        [TestMethod]
        public void ShouldEvaluateAGenericEnumerableAsPotentiallyNull()
        {
            Assert.IsTrue(typeof(IEnumerable<DateTime>).CanBeNull());
        }

        [TestMethod]
        public void ShouldEvaluateAStringAsPotentiallyNull()
        {
            Assert.IsTrue(typeof(string).CanBeNull());
        }

        [TestMethod]
        public void ShouldNotEvaluateAGuidAsPotentiallyNull()
        {
            Assert.IsFalse(typeof(Guid).CanBeNull());
        }

        [TestMethod]
        public void ShouldEvaluateANullableValueTypeAsPotentiallyNull()
        {
            Assert.IsTrue(typeof(long?).CanBeNull());
        }
    }
}
