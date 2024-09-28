namespace AgileObjects.ReadableExpressions.UnitTests.Reflection;

using System;
using System.Linq;
using ReadableExpressions.Translations.Reflection;

#if NET35
[NUnitTestFixture]
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

        var argObject = GenericParameterFactory.For(argument);

        argObject.ShouldNotBeNull();
        argObject.Assembly.ShouldBeSameAs(typeof(Helper).GetAssembly());
        argObject.Name.ShouldBe("TUnconstrained");
        argObject.FullName.ShouldBeNull();
        argObject.IsGenericParameter.ShouldBeTrue();
        argObject.HasConstraints.ShouldBeFalse();
        argObject.HasClassConstraint.ShouldBeFalse();
        argObject.HasStructConstraint.ShouldBeFalse();
        argObject.HasNewableConstraint.ShouldBeFalse();
        argObject.ConstraintTypes.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldCreateAnUnconstrainedClosedGenericArgument()
    {
        var argument = typeof(Helper)
            .GetPublicInstanceMethod(nameof(Helper.UnconstrainedGenericArg))
            .MakeGenericMethod(typeof(string))
            .GetGenericArguments()
            .First();

        var argObject = GenericParameterFactory.For(argument);

        argObject.ShouldNotBeNull();
        argObject.AsType().ShouldBe(typeof(string));
        argObject.IsGenericParameter.ShouldBeFalse();
        argObject.HasConstraints.ShouldBeFalse();
        argObject.HasClassConstraint.ShouldBeFalse();
        argObject.HasStructConstraint.ShouldBeFalse();
        argObject.HasNewableConstraint.ShouldBeFalse();
        argObject.ConstraintTypes.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldCreateANewableClassConstrainedGenericArgument()
    {
        var argument = typeof(Helper)
            .GetPublicInstanceMethod(nameof(Helper.ClassConstrainedGenericArg))
            .GetGenericArguments()
            .First();

        var argObject = GenericParameterFactory.For(argument);

        argObject.ShouldNotBeNull();
        argObject.Assembly.ShouldBeSameAs(typeof(Helper).GetAssembly());
        argObject.Name.ShouldBe("TConstrained");
        argObject.FullName.ShouldBeNull();
        argObject.IsGenericParameter.ShouldBeTrue();
        argObject.HasConstraints.ShouldBeTrue();
        argObject.HasClassConstraint.ShouldBeTrue();
        argObject.HasStructConstraint.ShouldBeFalse();
        argObject.HasNewableConstraint.ShouldBeTrue();
        argObject.ConstraintTypes.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public void ShouldCreateAStructInterfaceConstrainedGenericArgument()
    {
        var argument = typeof(Helper)
            .GetPublicInstanceMethod(nameof(Helper.StructInterfaceConstrainedGenericArg))
            .GetGenericArguments()
            .First();

        var argObject = GenericParameterFactory.For(argument);

        argObject.ShouldNotBeNull();
        argObject.Name.ShouldBe("TConstrained");
        argObject.FullName.ShouldBeNull();
        argObject.IsGenericParameter.ShouldBeTrue();
        argObject.HasConstraints.ShouldBeTrue();
        argObject.HasClassConstraint.ShouldBeFalse();
        argObject.HasStructConstraint.ShouldBeTrue();
        argObject.HasNewableConstraint.ShouldBeFalse();
        argObject.ConstraintTypes.ShouldHaveSingleItem().AsType().ShouldBe(typeof(IDisposable));
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

    #endregion
}