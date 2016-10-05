namespace AgileObjects.ReadableExpressions.UnitTests.Polyfills
{
    using System;
    using System.Data;
    using System.Reflection;
    using Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenRetrievingTypeCodes
    {
        [TestMethod]
        public void ShouldReturnEmpty()
        {
            var typeCode = default(Type).GetTypeCode();

            Assert.AreEqual(NetStandardTypeCode.Empty, typeCode);
        }

        [TestMethod]
        public void ShouldReturnDbNull()
        {
            var typeCode = typeof(DBNull).GetTypeCode();

            Assert.AreEqual(NetStandardTypeCode.DBNull, typeCode);
        }

        [TestMethod]
        public void ShouldReturnBoolean()
        {
            var typeCode = typeof(bool).GetTypeCode();

            Assert.AreEqual(NetStandardTypeCode.Boolean, typeCode);
        }

        [TestMethod]
        public void ShouldReturnString()
        {
            var typeCode = typeof(string).GetTypeCode();

            Assert.AreEqual(NetStandardTypeCode.String, typeCode);
        }

        [TestMethod]
        public void ShouldReturnUnderlyingTypeForAnEnum()
        {
            var typeCode = typeof(BindingFlags).GetTypeCode();

            Assert.AreEqual(NetStandardTypeCode.Int32, typeCode);
        }

        [TestMethod]
        public void ShouldFallbackToObject()
        {
            var typeCode = typeof(IDataReader).GetTypeCode();

            Assert.AreEqual(NetStandardTypeCode.Object, typeCode);
        }
    }
}
