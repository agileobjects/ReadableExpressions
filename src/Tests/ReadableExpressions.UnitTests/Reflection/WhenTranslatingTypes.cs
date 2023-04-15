namespace AgileObjects.ReadableExpressions.UnitTests.Reflection
{
    using System;
    using System.Collections.Generic;
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingTypes
    {
        [Fact]
        public void ShouldTranslateANullType()
        {
            var translated = default(Type).ToReadableString();

            translated.ShouldBe("[Type not found]");
        }

        [Fact]
        public void ShouldTranslateAPublicType()
        {
            var translated = typeof(PublicHelper).ToReadableString();

            translated.ShouldBe("public class PublicHelper");
        }

        [Fact]
        public void ShouldTranslateAnInternalType()
        {
            var translated = typeof(InternalHelper).ToReadableString();

            translated.ShouldBe("internal class InternalHelper");
        }

        [Fact]
        public void ShouldTranslateAnInternalInterface()
        {
            var translated = typeof(IInternalInterfaceHelper).ToReadableString();

            translated.ShouldBe("internal interface IInternalInterfaceHelper");
        }

        [Fact]
        public void ShouldTranslateAPublicStruct()
        {
            var translated = typeof(PublicStructHelper).ToReadableString();

            translated.ShouldBe("public struct PublicStructHelper");
        }

        [Fact]
        public void ShouldTranslateANestedPublicType()
        {
            var translated = typeof(NestedPublicHelper).ToReadableString();

            translated.ShouldBe("public class WhenTranslatingTypes.NestedPublicHelper");
        }

        [Fact]
        public void ShouldTranslateAnAbstractNestedPrivateType()
        {
            var translated = typeof(NestedPrivateAbstractHelper).ToReadableString();

            translated.ShouldBe("private abstract class WhenTranslatingTypes.NestedPrivateAbstractHelper");
        }

        [Fact]
        public void ShouldTranslateASealedNestedInternalType()
        {
            var translated = typeof(NestedSealedInternalHelper).ToReadableString();

            translated.ShouldBe("internal sealed class WhenTranslatingTypes.NestedSealedInternalHelper");
        }

        [Fact]
        public void ShouldTranslateAnOpenGenericType()
        {
            var translated = typeof(NestedPublicHelper<>).ToReadableString();

            translated.ShouldBe("public class WhenTranslatingTypes.NestedPublicHelper<T>");
        }

        [Fact]
        public void ShouldTranslateAClosedGenericType()
        {
            var translated = typeof(NestedPublicHelper<Dictionary<string, string>>).ToReadableString();

            translated.ShouldBe("public class WhenTranslatingTypes.NestedPublicHelper<Dictionary<string, string>>");
        }

        [Fact]
        public void ShouldTranslateATwoOpenGenericsType()
        {
            var translated = typeof(NestedProtectedHelper<,>).ToReadableString();

            translated.ShouldBe("protected class WhenTranslatingTypes.NestedProtectedHelper<T1, T2>");
        }

        [Fact]
        public void ShouldTranslateATwoClosedGenericsType()
        {
            var translated = typeof(NestedProtectedHelper<int, DateTime>).ToReadableString();

            translated.ShouldBe("protected class WhenTranslatingTypes.NestedProtectedHelper<int, DateTime>");
        }

        #region Helper Classes

        // ReSharper disable UnusedMember.Local
        public class NestedPublicHelper
        {
        }

        public class NestedPublicHelper<T>
        {
        }

        protected class NestedProtectedHelper<T1, T2>
        {
        }

        private abstract class NestedPrivateAbstractHelper
        {
        }

        internal sealed class NestedSealedInternalHelper
        {
        }
        // ReSharper restore UnusedMember.Local

        #endregion
    }

    public class PublicHelper
    {
    }

    internal class InternalHelper
    {
    }

    internal interface IInternalInterfaceHelper
    {
    }

    public struct PublicStructHelper
    {
    }
}
