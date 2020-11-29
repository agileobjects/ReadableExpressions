namespace AgileObjects.ReadableExpressions.UnitTests.Reflection
{
    using System;
    using System.Reflection;
    using Common;
    using NetStandardPolyfills;
    using Translations.Reflection;
#if !NET35
    using Xunit;
    using static Common.TestTranslationSettings;
#else
    using static Common.TestTranslationSettings;
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
        public void ShouldDescribeAnInstancePublicGetSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetPublicInstanceProperty(nameof(PropertyHelper.InstancePublicGetSet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(PropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeTrue();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeFalse();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeTrue();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsPublic.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeTrue();
            propertyWrapper.Setter.ShouldNotBeNull();
            propertyWrapper.Setter.IsPublic.ShouldBeTrue();
        }

        [Fact]
        public void ShouldTranslateAStaticPublicGetSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetPublicStaticProperty(nameof(PropertyHelper.StaticPublicGetSet));

            var translated = property.ToReadableString();

            translated.ShouldBe("public static int PropertyHelper.StaticPublicGetSet { get; set; }");
        }

        [Fact]
        public void ShouldDescribeAStaticPublicGetSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetPublicStaticProperty(nameof(PropertyHelper.StaticPublicGetSet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(PropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeTrue();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeTrue();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeFalse();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeTrue();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsPublic.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeTrue();
            propertyWrapper.Setter.ShouldNotBeNull();
            propertyWrapper.Setter.IsPublic.ShouldBeTrue();
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
        public void ShouldDescribeAnInstanceInternalGetPrivateSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty(nameof(PropertyHelper.InstanceInternalGetPrivateSet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(PropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeFalse();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeTrue();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeFalse();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeFalse();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsInternal.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldNotBeNull();
            propertyWrapper.Setter.IsPrivate.ShouldBeTrue();
        }

        [Fact]
        public void ShouldTranslateAnInstanceProtectedInternalGetProtectedSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty(
                    nameof(PropertyHelper.InstanceProtectedInternalGetProtectedSet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "protected internal string PropertyHelper.InstanceProtectedInternalGetProtectedSet " +
                    "{ get; protected set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnInstanceProtectedInternalGetProtectedSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty(
                    nameof(PropertyHelper.InstanceProtectedInternalGetProtectedSet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(PropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeFalse();
            propertyWrapper.IsProtectedInternal.ShouldBeTrue();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeFalse();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeFalse();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsProtectedInternal.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldNotBeNull();
            propertyWrapper.Setter.IsProtected.ShouldBeTrue();
        }

        [Fact]
        public void ShouldTranslateAnInstanceProtectedGetPrivateProtectedSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty("InstanceProtectedGetPrivateProtectedSet");

            var translated = property.ToReadableString();

            const string EXPECTED =
                "protected string PropertyHelper.InstanceProtectedGetPrivateProtectedSet " +
                    "{ get; private protected set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnInstanceProtectedGetPrivateProtectedSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty("InstanceProtectedGetPrivateProtectedSet");

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(PropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeFalse();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeTrue();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeFalse();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeFalse();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsProtected.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldNotBeNull();
            propertyWrapper.Setter.IsPrivateProtected.ShouldBeTrue();
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
        public void ShouldDescribeAnInstancePrivateGetSetProperty()
        {
            var property = typeof(PropertyHelper)
                .GetNonPublicInstanceProperty("InstancePrivateGetSet");

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(PropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeFalse();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeTrue();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeFalse();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeFalse();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsPrivate.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldNotBeNull();
            propertyWrapper.Setter.IsPrivate.ShouldBeTrue();
        }

        [Fact]
        public void ShouldTranslateAnAbstractPublicGetProperty()
        {
            var property = typeof(AbstractPropertyHelper)
                .GetPublicInstanceProperty(nameof(AbstractPropertyHelper.AbstractPublicGet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public abstract int AbstractPropertyHelper.AbstractPublicGet { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnAbstractPublicGetProperty()
        {
            var property = typeof(AbstractPropertyHelper)
                .GetPublicInstanceProperty(nameof(AbstractPropertyHelper.AbstractPublicGet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(AbstractPropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeTrue();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeTrue();
            propertyWrapper.IsVirtual.ShouldBeFalse();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeTrue();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsPublic.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldBeNull();
        }

        [Fact]
        public void ShouldTranslateAnAbstractOverridePublicGetProperty()
        {
            var property = typeof(DerivedPropertyHelper)
                .GetPublicInstanceProperty(nameof(DerivedPropertyHelper.AbstractPublicGet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public override int DerivedPropertyHelper.AbstractPublicGet { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnAbstractOverridePublicGetProperty()
        {
            var property = typeof(DerivedPropertyHelper)
                .GetPublicInstanceProperty(nameof(DerivedPropertyHelper.AbstractPublicGet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(DerivedPropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeTrue();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeTrue();
            propertyWrapper.IsOverride.ShouldBeTrue();

            propertyWrapper.IsReadable.ShouldBeTrue();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsPublic.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldBeNull();
        }

        [Fact]
        public void ShouldTranslateAVirtualPublicGetProperty()
        {
            var property = typeof(AbstractPropertyHelper)
                .GetPublicInstanceProperty(nameof(AbstractPropertyHelper.VirtualPublicGet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "public virtual int AbstractPropertyHelper.VirtualPublicGet { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAVirtualPublicGetProperty()
        {
            var property = typeof(AbstractPropertyHelper)
                .GetPublicInstanceProperty(nameof(AbstractPropertyHelper.VirtualPublicGet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(AbstractPropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeTrue();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeTrue();
            propertyWrapper.IsOverride.ShouldBeFalse();

            propertyWrapper.IsReadable.ShouldBeTrue();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsPublic.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldBeNull();
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

        [Fact]
        public void ShouldDescribeAVirtualOverridePublicGetProperty()
        {
            var property = typeof(DerivedPropertyHelper)
                .GetPublicInstanceProperty(nameof(DerivedPropertyHelper.VirtualPublicGet));

            var propertyWrapper = new BclPropertyWrapper(property, TestSettings);

            propertyWrapper.DeclaringType.AsType().ShouldBe(typeof(DerivedPropertyHelper));
            propertyWrapper.Name.ShouldBe(property.Name);
            propertyWrapper.Type.AsType().ShouldBe(property.PropertyType);
            propertyWrapper.IsPublic.ShouldBeTrue();
            propertyWrapper.IsProtectedInternal.ShouldBeFalse();
            propertyWrapper.IsInternal.ShouldBeFalse();
            propertyWrapper.IsProtected.ShouldBeFalse();
            propertyWrapper.IsPrivateProtected.ShouldBeFalse();
            propertyWrapper.IsPrivate.ShouldBeFalse();

            propertyWrapper.IsStatic.ShouldBeFalse();
            propertyWrapper.IsAbstract.ShouldBeFalse();
            propertyWrapper.IsVirtual.ShouldBeTrue();
            propertyWrapper.IsOverride.ShouldBeTrue();

            propertyWrapper.IsReadable.ShouldBeTrue();
            propertyWrapper.Getter.ShouldNotBeNull();
            propertyWrapper.Getter.IsPublic.ShouldBeTrue();
            propertyWrapper.IsWritable.ShouldBeFalse();
            propertyWrapper.Setter.ShouldBeNull();
        }

        [Fact]
        public void ShouldTranslateAnInterfaceGetSetProperty()
        {
            var property = typeof(IInterfacePropertyHelper)
                .GetPublicInstanceProperty(nameof(IInterfacePropertyHelper.GetSet));

            var translated = property.ToReadableString();

            const string EXPECTED =
                "string IInterfacePropertyHelper.GetSet { get; set; }";

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

        protected string InstanceProtectedGetPrivateProtectedSet { get; private protected set; }

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

    public interface IInterfacePropertyHelper
    {
        string GetSet { get; set; }
    }

    #endregion
}