namespace AgileObjects.ReadableExpressions.UnitTests.Reflection
{
    using System;
    using System.Reflection;
    using Common;
    using NetStandardPolyfills;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingPropertyInfos
    {
        [Fact]
        public void ShouldTranslateANullPropertyInfo()
        {
            var translated = default(PropertyInfo).ToReadableString();

            translated.ShouldBe("[Property not found]");
        }

        [Fact]
        public void ShouldTranslateAnInstancePublicGetSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetPublicInstanceProperty(nameof(PropertyHelper.InstancePublicGetSet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public int PropertyHelper.InstancePublicGetSet { get; set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAStaticPublicGetSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetPublicStaticProperty(nameof(PropertyHelper.StaticPublicGetSet));

            property.ShouldNotBeNull();

            var translated = property.ToReadableString();

            translated.ShouldBe("public static int PropertyHelper.StaticPublicGetSet { get; set; }");
        }

        [Fact]
        public void ShouldTranslateAnInstanceInternalGetPrivateSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty(nameof(PropertyHelper.InstanceInternalGetPrivateSet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "internal string PropertyHelper.InstanceInternalGetPrivateSet { get; private set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnInstanceProtectedInternalGetProtectedSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty(nameof(PropertyHelper.InstanceProtectedInternalGetProtectedSet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "protected internal string PropertyHelper.InstanceProtectedInternalGetProtectedSet { get; protected set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnInstanceProtectedGetPrivateSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty("InstanceProtectedGetPrivateSet");

            var translated = property.ToReadableString();

            const string EXPECTED =
                "protected string PropertyHelper.InstanceProtectedGetPrivateSet { get; private set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnInstancePrivateGetSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty("InstancePrivateGetSet");

            var translated = property.ToReadableString();

            const string EXPECTED =
                "private DateTime PropertyHelper.InstancePrivateGetSet { get; set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnAbstractPublicGetProperty()
        {
            var property = typeof(AbstractPropertyHelper)
                .GetPublicInstanceProperty("AbstractPublicGet");

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public abstract int AbstractPropertyHelper.AbstractPublicGet { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnAbstractOverridePublicGetProperty()
        {
            var property = typeof(DerivedPropertyHelper)
                .GetPublicInstanceProperty("AbstractPublicGet");

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public override int DerivedPropertyHelper.AbstractPublicGet { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAVirtualPublicGetProperty()
        {
            var property = typeof(AbstractPropertyHelper)
                .GetPublicInstanceProperty("VirtualPublicGet");

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public virtual int AbstractPropertyHelper.VirtualPublicGet { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAVirtualOverridePublicGetProperty()
        {
            var property = typeof(DerivedPropertyHelper)
                .GetPublicInstanceProperty("VirtualPublicGet");

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public override int DerivedPropertyHelper.VirtualPublicGet { get; }";

            translated.ShouldBe(EXPECTED);
        }
    }

    #region Helper Classes

    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper disable MemberCanBePrivate.Local
    // ReSharper disable UnusedMember.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    // ReSharper disable UnusedMember.Global
    public class PropertyHelper
    {
        public static int StaticPublicGetSet { get; set; }

        public int InstancePublicGetSet { get; set; }

        internal string InstanceInternalGetPrivateSet { get; private set; }

        protected internal string InstanceProtectedInternalGetProtectedSet { get; protected set; }

        protected string InstanceProtectedGetPrivateSet { get; private set; }

        private DateTime InstancePrivateGetSet { get; set; }
    }
    // ReSharper restore UnusedMember.Global
    // ReSharper restore UnusedMember.Local
    // ReSharper restore UnusedAutoPropertyAccessor.Local
    // ReSharper restore MemberCanBePrivate.Local
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Local

    public abstract class AbstractPropertyHelper
    {
        public abstract int AbstractPublicGet { get; }

        public virtual int VirtualPublicGet => 123;
    }

    public class DerivedPropertyHelper : AbstractPropertyHelper
    {
        public override int AbstractPublicGet => 123;

        public override int VirtualPublicGet => 456;
    }

    #endregion
}