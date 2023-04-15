namespace AgileObjects.ReadableExpressions.UnitTests.Reflection
{
    using System;
    using System.Reflection;
    using Common;
    using NetStandardPolyfills;
    using ReadableExpressions.Translations.Reflection;
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
        public void ShouldTranslateAPublicInstanceReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.PublicInstanceReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "public int WhenTranslatingFieldInfos.FieldHelper.PublicInstanceReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAPublicInstanceReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.PublicInstanceReadWrite));

            var fieldWrapper = new ClrFieldWrapper(field);

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
        public void ShouldTranslateAPublicInstanceReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.PublicInstanceReadonly));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "public readonly int WhenTranslatingFieldInfos.FieldHelper.PublicInstanceReadonly;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAPublicInstanceReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetPublicInstanceField(nameof(FieldHelper.PublicInstanceReadonly));

            var fieldWrapper = new ClrFieldWrapper(field);

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
        public void ShouldTranslateAPublicStaticReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicStaticField(nameof(FieldHelper.PublicStaticReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "public static int WhenTranslatingFieldInfos.FieldHelper.PublicStaticReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAPublicStaticReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetPublicStaticField(nameof(FieldHelper.PublicStaticReadWrite));

            var fieldWrapper = new ClrFieldWrapper(field);

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
        public void ShouldTranslateAPublicConstantField()
        {
            var field = typeof(FieldHelper)
                .GetPublicStaticField(nameof(FieldHelper.PublicConstant));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "public const string WhenTranslatingFieldInfos.FieldHelper.PublicConstant;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAPublicConstantField()
        {
            var field = typeof(FieldHelper)
                .GetPublicStaticField(nameof(FieldHelper.PublicConstant));

            var fieldWrapper = new ClrFieldWrapper(field);

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
            fieldWrapper.IsConstant.ShouldBeTrue();
            fieldWrapper.IsReadonly.ShouldBeTrue();
        }

        [Fact]
        public void ShouldTranslateAInternalInstanceReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.InternalInstanceReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "internal string WhenTranslatingFieldInfos.FieldHelper.InternalInstanceReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAInternalInstanceReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.InternalInstanceReadWrite));

            var fieldWrapper = new ClrFieldWrapper(field);

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
        public void ShouldTranslateAProtectedInternalInstanceReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.ProtectedInternalInstanceReadWrite));

            var translated = field.ToReadableString();

            const string EXPECTED =
                "protected internal string " +
                    "WhenTranslatingFieldInfos.FieldHelper.ProtectedInternalInstanceReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAProtectedInternalInstanceReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField(nameof(FieldHelper.ProtectedInternalInstanceReadWrite));

            var fieldWrapper = new ClrFieldWrapper(field);

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
        public void ShouldTranslateAProtectedInstanceReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField("ProtectedInstanceReadonly");

            var translated = field.ToReadableString();

            const string EXPECTED =
                "protected readonly string " +
                    "WhenTranslatingFieldInfos.FieldHelper.ProtectedInstanceReadonly;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAProtectedInstanceReadonlyField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicInstanceField("ProtectedInstanceReadonly");

            var fieldWrapper = new ClrFieldWrapper(field);

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
        public void ShouldTranslateAPrivateStaticReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicStaticField("_privateStaticReadWrite");

            var translated = field.ToReadableString();

            const string EXPECTED =
                "private static DateTime " +
                    "WhenTranslatingFieldInfos.FieldHelper._privateStaticReadWrite;";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldDescribeAPrivateStaticReadWriteField()
        {
            var field = typeof(FieldHelper)
                .GetNonPublicStaticField("_privateStaticReadWrite");

            var fieldWrapper = new ClrFieldWrapper(field);

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
            public static int PublicStaticReadWrite;

            public const string PublicConstant = "hello!";

            public int PublicInstanceReadWrite;

            public readonly int PublicInstanceReadonly = 123;

            internal string InternalInstanceReadWrite;

            protected internal string ProtectedInternalInstanceReadWrite;

            protected readonly string ProtectedInstanceReadonly = "Hello!";

            private static DateTime _privateStaticReadWrite = DateTime.Now;
        }
        // ReSharper restore UnusedMember.Global
        // ReSharper restore UnusedMember.Local
        // ReSharper restore MemberCanBePrivate.Local

        #endregion
    }
}