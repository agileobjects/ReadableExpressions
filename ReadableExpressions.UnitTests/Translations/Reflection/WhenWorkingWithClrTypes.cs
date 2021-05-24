namespace AgileObjects.ReadableExpressions.UnitTests.Translations.Reflection
{
    using System;
    using Common;
    using ReadableExpressions.Translations.Reflection;
    using Xunit;

    public class WhenWorkingWithClrTypes
    {
        [Fact]
        public void ShouldReturnTheSingletonObjectType()
        {
            ClrTypeWrapper.For(typeof(object)).ShouldBeSameAs(ClrTypeWrapper.Object);
        }

        [Fact]
        public void ShouldReturnTheSingletonValueTypeType()
        {
            ClrTypeWrapper.For(typeof(ValueType)).ShouldBeSameAs(ClrTypeWrapper.ValueType);
        }

        [Fact]
        public void ShouldReturnTheSingletonAttributeType()
        {
            ClrTypeWrapper.For<Attribute>().ShouldBeSameAs(ClrTypeWrapper.Attribute);
        }

        [Fact]
        public void ShouldReturnTheSingletonStringType()
        {
            ClrTypeWrapper.For<string>().ShouldBeSameAs(ClrTypeWrapper.String);
        }

        [Fact]
        public void ShouldReturnTheSingletonVoidType()
        {
            ClrTypeWrapper.For(typeof(void)).ShouldBeSameAs(ClrTypeWrapper.Void);
        }

        [Fact]
        public void ShouldReturnTheSingletonEnumType()
        {
            ClrTypeWrapper.For<Enum>().ShouldBeSameAs(ClrTypeWrapper.Enum);
        }
    }
}
