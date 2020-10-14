namespace AgileObjects.ReadableExpressions.UnitTests.Reflection
{
    using System;
    using System.Linq;
    using Common;
    using NetStandardPolyfills;
    using Translations.Reflection;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenCreatingGenericArguments
    {
        [Fact]
        public void ShouldCreateAnUnconstrainedOpenGenericArgument()
        {
            var argument = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.UnconstrainedGenericArg))
                .GetGenericArguments()
                .First();

            var argObject = GenericArgumentFactory
                .For(argument, new TestTranslationSettings());

            argObject.ShouldNotBeNull();
            argObject.Type.Name.ShouldBe("TUnconstrained");
            argObject.Type.FullName.ShouldBeNull();
            argObject.TypeName.ShouldBe("TUnconstrained");
            argObject.IsClosed.ShouldBeFalse();
            argObject.HasConstraints.ShouldBeFalse();
            argObject.HasClassConstraint.ShouldBeFalse();
            argObject.HasStructConstraint.ShouldBeFalse();
            argObject.HasNewableConstraint.ShouldBeFalse();
            argObject.TypeConstraints.ShouldNotBeNull().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldCreateAnUnconstrainedClosedGenericArgument()
        {
            var argument = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.UnconstrainedGenericArg))
                .MakeGenericMethod(typeof(string))
                .GetGenericArguments()
                .First();

            var argObject = GenericArgumentFactory
                .For(argument, new TestTranslationSettings());

            argObject.ShouldNotBeNull();
            argObject.Type.ShouldBe(typeof(string));
            argObject.TypeName.ShouldBe("string");
            argObject.IsClosed.ShouldBeTrue();
            argObject.HasConstraints.ShouldBeFalse();
            argObject.HasClassConstraint.ShouldBeFalse();
            argObject.HasStructConstraint.ShouldBeFalse();
            argObject.HasNewableConstraint.ShouldBeFalse();
            argObject.TypeConstraints.ShouldNotBeNull().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldCreateANewableClassConstrainedGenericArgument()
        {
            var argument = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.ClassConstrainedGenericArg))
                .GetGenericArguments()
                .First();

            var argObject = GenericArgumentFactory
                .For(argument, new TestTranslationSettings());

            argObject.ShouldNotBeNull();
            argObject.Type.Name.ShouldBe("TConstrained");
            argObject.Type.FullName.ShouldBeNull();
            argObject.TypeName.ShouldBe("TConstrained");
            argObject.IsClosed.ShouldBeFalse();
            argObject.HasConstraints.ShouldBeTrue();
            argObject.HasClassConstraint.ShouldBeTrue();
            argObject.HasStructConstraint.ShouldBeFalse();
            argObject.HasNewableConstraint.ShouldBeTrue();
            argObject.TypeConstraints.ShouldNotBeNull().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldCreateAStructInterfaceConstrainedGenericArgument()
        {
            var argument = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.StructInterfaceConstrainedGenericArg))
                .GetGenericArguments()
                .First();

            var argObject = GenericArgumentFactory
                .For(argument, new TestTranslationSettings());

            argObject.ShouldNotBeNull();
            argObject.Type.Name.ShouldBe("TConstrained");
            argObject.Type.FullName.ShouldBeNull();
            argObject.TypeName.ShouldBe("TConstrained");
            argObject.IsClosed.ShouldBeFalse();
            argObject.HasConstraints.ShouldBeTrue();
            argObject.HasClassConstraint.ShouldBeFalse();
            argObject.HasStructConstraint.ShouldBeTrue();
            argObject.HasNewableConstraint.ShouldBeFalse();
            argObject.TypeConstraints.ShouldHaveSingleItem().ShouldBe(typeof(IDisposable));
        }

        #region Helper Members

        private class Helper
        {
            public void UnconstrainedGenericArg<TUnconstrained>()
                => Console.WriteLine(typeof(TUnconstrained).Name);

            public void ClassConstrainedGenericArg<TConstrained>()
                where TConstrained : class, new()
            {
                Console.WriteLine(typeof(TConstrained).Name);
            }

            public void StructInterfaceConstrainedGenericArg<TConstrained>()
                where TConstrained : struct, IDisposable
            {
                Console.WriteLine(typeof(TConstrained).Name);
            }
        }

        private class TestTranslationSettings : TranslationSettings
        {
        }

        #endregion
    }
}
