namespace AgileObjects.ReadableExpressions.UnitTests.Reflection
{
    using System;
    using System.Reflection;
    using Common;
    using NetStandardPolyfills;
    using Translations.Reflection;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingFieldInfos
    {
        [Fact]
        public void ShouldTranslateANullFieldInfo()
        {
            var translated = default(FieldInfo).ToReadableString();

            translated.ShouldBe("[Field not found]");
        }

        [Fact]
        public void ShouldTranslateAnInstancePublicReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.InstancePublicReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "public int WhenTranslatingFieldInfos.FieldHelper.InstancePublicReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnInstancePublicReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.InstancePublicReadWrite));

            var fieldWrapper = new BclFieldWrapper(field);

            fieldWrapper.DeclaringType.AsType().ShouldBe(typeof(FieldHelper));
            fieldWrapper.Name.ShouldBe(field.Name);
            fieldWrapper.Type.AsType().ShouldBe(field.FieldType);
            fieldWrapper.IsPublic.ShouldBeTrue();
            fieldWrapper.IsProtectedInternal.ShouldBeFalse();
            fieldWrapper.IsInternal.ShouldBeFalse();
            fieldWrapper.IsProtected.ShouldBeFalse();
            fieldWrapper.IsPrivateProtected.ShouldBeFalse();
            fieldWrapper.IsPrivate.ShouldBeFalse();

            fieldWrapper.IsStatic.ShouldBeFalse();
            fieldWrapper.IsReadonly.ShouldBeFalse();
        }

        [Fact]
        public void ShouldTranslateAnInstancePublicReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.InstancePublicReadonly));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "public readonly int WhenTranslatingFieldInfos.FieldHelper.InstancePublicReadonly;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnInstancePublicReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.InstancePublicReadonly));

            var fieldWrapper = new BclFieldWrapper(field);

            fieldWrapper.DeclaringType.AsType().ShouldBe(typeof(FieldHelper));
            fieldWrapper.Name.ShouldBe(field.Name);
            fieldWrapper.Type.AsType().ShouldBe(field.FieldType);
            fieldWrapper.IsPublic.ShouldBeTrue();
            fieldWrapper.IsProtectedInternal.ShouldBeFalse();
            fieldWrapper.IsInternal.ShouldBeFalse();
            fieldWrapper.IsProtected.ShouldBeFalse();
            fieldWrapper.IsPrivateProtected.ShouldBeFalse();
            fieldWrapper.IsPrivate.ShouldBeFalse();

            fieldWrapper.IsStatic.ShouldBeFalse();
            fieldWrapper.IsReadonly.ShouldBeTrue();
        }

        [Fact]
        public void ShouldTranslateAnStaticPublicReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicStaticField(nameof(FieldHelper.StaticPublicReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "public static int WhenTranslatingFieldInfos.FieldHelper.StaticPublicReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnStaticPublicReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicStaticField(nameof(FieldHelper.StaticPublicReadWrite));

            var fieldWrapper = new BclFieldWrapper(field);

            fieldWrapper.DeclaringType.AsType().ShouldBe(typeof(FieldHelper));
            fieldWrapper.Name.ShouldBe(field.Name);
            fieldWrapper.Type.AsType().ShouldBe(field.FieldType);
            fieldWrapper.IsPublic.ShouldBeTrue();
            fieldWrapper.IsProtectedInternal.ShouldBeFalse();
            fieldWrapper.IsInternal.ShouldBeFalse();
            fieldWrapper.IsProtected.ShouldBeFalse();
            fieldWrapper.IsPrivateProtected.ShouldBeFalse();
            fieldWrapper.IsPrivate.ShouldBeFalse();

            fieldWrapper.IsStatic.ShouldBeTrue();
            fieldWrapper.IsReadonly.ShouldBeFalse();
        }

        [Fact]
        public void ShouldTranslateAnInstanceInternalReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.InstanceInternalReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "internal string WhenTranslatingFieldInfos.FieldHelper.InstanceInternalReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnInstanceInternalReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.InstanceInternalReadWrite));

            var fieldWrapper = new BclFieldWrapper(field);

            fieldWrapper.DeclaringType.AsType().ShouldBe(typeof(FieldHelper));
            fieldWrapper.Name.ShouldBe(field.Name);
            fieldWrapper.Type.AsType().ShouldBe(field.FieldType);
            fieldWrapper.IsPublic.ShouldBeFalse();
            fieldWrapper.IsProtectedInternal.ShouldBeFalse();
            fieldWrapper.IsInternal.ShouldBeTrue();
            fieldWrapper.IsProtected.ShouldBeFalse();
            fieldWrapper.IsPrivateProtected.ShouldBeFalse();
            fieldWrapper.IsPrivate.ShouldBeFalse();

            fieldWrapper.IsStatic.ShouldBeFalse();
            fieldWrapper.IsReadonly.ShouldBeFalse();
        }

        [Fact]
        public void ShouldTranslateAnInstanceProtectedInternalReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.InstanceProtectedInternalReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "protected internal string " +
                    "WhenTranslatingFieldInfos.FieldHelper.InstanceProtectedInternalReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnInstanceProtectedInternalReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.InstanceProtectedInternalReadWrite));

            var fieldWrapper = new BclFieldWrapper(field);

            fieldWrapper.DeclaringType.AsType().ShouldBe(typeof(FieldHelper));
            fieldWrapper.Name.ShouldBe(field.Name);
            fieldWrapper.Type.AsType().ShouldBe(field.FieldType);
            fieldWrapper.IsPublic.ShouldBeFalse();
            fieldWrapper.IsProtectedInternal.ShouldBeTrue();
            fieldWrapper.IsInternal.ShouldBeFalse();
            fieldWrapper.IsProtected.ShouldBeFalse();
            fieldWrapper.IsPrivateProtected.ShouldBeFalse();
            fieldWrapper.IsPrivate.ShouldBeFalse();

            fieldWrapper.IsStatic.ShouldBeFalse();
            fieldWrapper.IsReadonly.ShouldBeFalse();
        }

        [Fact]
        public void ShouldTranslateAnInstanceProtectedReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField("InstanceProtectedReadonly");

            var translated = field.ToReadableString();

            const string EXPECTED =
                "protected readonly string " +
                    "WhenTranslatingFieldInfos.FieldHelper.InstanceProtectedReadonly;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnInstanceProtectedReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField("InstanceProtectedReadonly");

            var fieldWrapper = new BclFieldWrapper(field);

            fieldWrapper.DeclaringType.AsType().ShouldBe(typeof(FieldHelper));
            fieldWrapper.Name.ShouldBe(field.Name);
            fieldWrapper.Type.AsType().ShouldBe(field.FieldType);
            fieldWrapper.IsPublic.ShouldBeFalse();
            fieldWrapper.IsProtectedInternal.ShouldBeFalse();
            fieldWrapper.IsInternal.ShouldBeFalse();
            fieldWrapper.IsProtected.ShouldBeTrue();
            fieldWrapper.IsPrivateProtected.ShouldBeFalse();
            fieldWrapper.IsPrivate.ShouldBeFalse();

            fieldWrapper.IsStatic.ShouldBeFalse();
            fieldWrapper.IsReadonly.ShouldBeTrue();
        }

        [Fact]
        public void ShouldTranslateAnStaticPrivateReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicStaticField("_staticPrivateReadWrite");

            var translated = field.ToReadableString();

            const string EXPECTED =
                "private static DateTime " +
                    "WhenTranslatingFieldInfos.FieldHelper._staticPrivateReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAnStaticPrivateReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicStaticField("_staticPrivateReadWrite");

            var fieldWrapper = new BclFieldWrapper(field);

            fieldWrapper.DeclaringType.AsType().ShouldBe(typeof(FieldHelper));
            fieldWrapper.Name.ShouldBe(field.Name);
            fieldWrapper.Type.AsType().ShouldBe(field.FieldType);
            fieldWrapper.IsPublic.ShouldBeFalse();
            fieldWrapper.IsProtectedInternal.ShouldBeFalse();
            fieldWrapper.IsInternal.ShouldBeFalse();
            fieldWrapper.IsProtected.ShouldBeFalse();
            fieldWrapper.IsPrivateProtected.ShouldBeFalse();
            fieldWrapper.IsPrivate.ShouldBeTrue();

            fieldWrapper.IsStatic.ShouldBeTrue();
            fieldWrapper.IsReadonly.ShouldBeFalse();
        }

        #region Helper Classes

        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedMember.Global
        public class FieldHelper
        {
            public static int StaticPublicReadWrite;

            public int InstancePublicReadWrite;

            public readonly int InstancePublicReadonly = 123;

            internal string InstanceInternalReadWrite;

            protected internal string InstanceProtectedInternalReadWrite;

            protected readonly string InstanceProtectedReadonly = "Hello!";

            private static DateTime _staticPrivateReadWrite = DateTime.Now;
        }
        // ReSharper restore UnusedMember.Global
        // ReSharper restore UnusedMember.Local
        // ReSharper restore MemberCanBePrivate.Local

        #endregion
    }
}