namespace AgileObjects.ReadableExpressions.UnitTests.StaticTranslators
{
    using System;
    using System.Collections.Generic;
    using Translations.StaticTranslators;
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
            var translated = DefinitionsTranslator.Translate(default(Type));

            translated.ShouldBe("[Type not found]");
        }

        [Fact]
        public void ShouldTranslateAPublicType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(PublicHelper));

            translated.ShouldBe("public class PublicHelper");
        }

        [Fact]
        public void ShouldTranslateAnInternalType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(InternalHelper));

            translated.ShouldBe("internal class InternalHelper");
        }

        [Fact]
        public void ShouldTranslateAnInternalInterface()
        {
            var translated = DefinitionsTranslator.Translate(typeof(IInternalInterfaceHelper));

            translated.ShouldBe("internal interface IInternalInterfaceHelper");
        }

        [Fact]
        public void ShouldTranslateAPublicStruct()
        {
            var translated = DefinitionsTranslator.Translate(typeof(PublicStructHelper));

            translated.ShouldBe("public struct PublicStructHelper");
        }

        [Fact]
        public void ShouldTranslateANestedPublicType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(NestedPublicHelper));

            translated.ShouldBe("public class WhenTranslatingTypes.NestedPublicHelper");
        }

        [Fact]
        public void ShouldTranslateAnAbstractNestedPrivateType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(NestedPrivateAbstractHelper));

            translated.ShouldBe("private abstract class WhenTranslatingTypes.NestedPrivateAbstractHelper");
        }

        [Fact]
        public void ShouldTranslateASealedNestedInternalType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(NestedSealedInternalHelper));

            translated.ShouldBe("internal sealed class WhenTranslatingTypes.NestedSealedInternalHelper");
        }

        [Fact]
        public void ShouldTranslateAnOpenGenericType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(NestedPublicHelper<>));

            translated.ShouldBe("public class WhenTranslatingTypes.NestedPublicHelper<T>");
        }

        [Fact]
        public void ShouldTranslateAClosedGenericType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(NestedPublicHelper<Dictionary<string, string>>));

            translated.ShouldBe("public class WhenTranslatingTypes.NestedPublicHelper<Dictionary<string, string>>");
        }

        [Fact]
        public void ShouldTranslateATwoOpenGenericsType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(NestedProtectedHelper<,>));

            translated.ShouldBe("protected class WhenTranslatingTypes.NestedProtectedHelper<T1, T2>");
        }

        [Fact]
        public void ShouldTranslateATwoClosedGenericsType()
        {
            var translated = DefinitionsTranslator.Translate(typeof(NestedProtectedHelper<int, DateTime>));

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
