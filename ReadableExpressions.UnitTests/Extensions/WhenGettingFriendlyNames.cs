namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using ReadableExpressions.Extensions;
    using Xunit;

    public class WhenGettingFriendlyNames
    {
        [Fact]
        public void ShouldUseFriendlyNamesForArrays()
        {
            var intArrayVariable = Expression.Variable(typeof(int[]), "ints");
            var assignNull = Expression.Assign(intArrayVariable, Expression.Default(intArrayVariable.Type));
            var assignNullBlock = Expression.Block(new[] { intArrayVariable }, assignNull);

            var translated = assignNullBlock.ToReadableString();

            Assert.Equal("var ints = default(int[]);", translated);
        }

        [Fact]
        public void ShouldUseFriendlyNamesForCharacters()
        {
            Expression<Func<char, double>> characterToNumeric = c => char.GetNumericValue(c);

            var translated = characterToNumeric.ToReadableString();

            Assert.Equal("c => char.GetNumericValue(c)", translated);
        }

        [Fact]
        public void ShouldUseFriendlyNamesForAnonymousTypes()
        {
            var anon = new { One = 1, Two = "two" };

            var friendlyName = anon.GetType().GetFriendlyName();

            Assert.Equal("AnonymousType<int, string>", friendlyName);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/6
        [Fact]
        public void ShouldUseFriendlyNamesForMultiplyNestedTypes()
        {
            var nestedType = Expression.Constant(typeof(OuterClass.InnerClass.Nested), typeof(Type));

            var translated = nestedType.ToReadableString();

            Assert.Equal("typeof(OuterClass.InnerClass.Nested)", translated);
        }

        [Fact]
        public void ShouldUseFriendlyNamesForListsOfNestedTypes()
        {
            var newNestedTypeList = Expression.New(typeof(List<>).MakeGenericType(typeof(OuterClass.InnerClass)));

            var translated = newNestedTypeList.ToReadableString();

            Assert.Equal("new List<OuterClass.InnerClass>()", translated);
        }

        [Fact]
        public void ShouldUseFriendlyNamesForGenericNestedTypes()
        {
            var genericListEnumeratorType = Expression.Constant(typeof(List<string>.Enumerator), typeof(Type));

            var translated = genericListEnumeratorType.ToReadableString();

            Assert.Equal("typeof(List<string>.Enumerator)", translated);
        }

        [Fact]
        public void ShouldUseFriendlyNamesForGenericMultiplyNestedTypes()
        {
            var nestedGenericType = Expression.Constant(
                typeof(OuterGeneric<int>.InnerGeneric<long>.Nested),
                typeof(Type));

            var translated = nestedGenericType.ToReadableString();

            Assert.Equal("typeof(OuterGeneric<int>.InnerGeneric<long>.Nested)", translated);
        }
    }
}
