namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System.Collections;
    using System.Collections.Generic;
    using ReadableExpressions.Extensions;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenAccessingTypeInformation
    {
        [Fact]
        public void ShouldEvaluateAnArrayAsEnumerable()
        {
            typeof(int[]).IsEnumerable().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateAGenericListAsEnumerable()
        {
            typeof(List<string>).IsEnumerable().ShouldBeTrue();
        }

        [Fact]
        public void ShouldEvaluateANonGenericListAsEnumerable()
        {
            typeof(IList).IsEnumerable().ShouldBeTrue();
        }

        [Fact]
        public void ShouldNotEvaluateAStringAsEnumerable()
        {
            typeof(string).IsEnumerable().ShouldBeFalse();
        }
    }
}
