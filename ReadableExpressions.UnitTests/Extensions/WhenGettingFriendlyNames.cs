namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using ReadableExpressions.Extensions;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenGettingFriendlyNames : TestClassBase
    {
        [Fact]
        public void ShouldUseFriendlyNamesForArrays()
        {
            var intArrayVariable = Expression.Variable(typeof(int[]), "ints");
            var assignNull = Expression.Assign(intArrayVariable, Expression.Default(intArrayVariable.Type));
            var assignNullBlock = Expression.Block(new[] { intArrayVariable }, assignNull);

            var translated = ToReadableString(assignNullBlock);

            translated.ShouldBe("var ints = default(int[]);");
        }

        [Fact]
        public void ShouldUseFriendlyNamesForCharacters()
        {
            var characterToNumeric = CreateLambda((char c) => char.GetNumericValue(c));

            var translated = ToReadableString(characterToNumeric);

            translated.ShouldBe("c => char.GetNumericValue(c)");
        }

        [Fact]
        public void ShouldUseFriendlyNamesForAnonymousTypes()
        {
            var anon = new { One = 1, Two = "two" };

            var friendlyName = anon.GetType().GetFriendlyName();

            friendlyName.ShouldBe("AnonymousType<int, string>");
        }

        // See https://github.com/agileobjects/ReadableExpressions/pull/25
        [Fact]
        public void ShouldUseAnonymousTypeNameFactoryIfConfigured()
        {
            var anon = new { One = 1, Two = "two" };

            var friendlyName = anon.GetType().GetFriendlyName(c => c.NameAnonymousTypesUsing(t => "object"));

            friendlyName.ShouldBe("object");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/6
        [Fact]
        public void ShouldUseFriendlyNamesForMultiplyNestedTypes()
        {
            var nestedType = Expression.Constant(typeof(OuterClass.InnerClass.Nested), typeof(Type));

            var translated = ToReadableString(nestedType);

            translated.ShouldBe("typeof(OuterClass.InnerClass.Nested)");
        }

        [Fact]
        public void ShouldUseFriendlyNamesForListsOfNestedTypes()
        {
            var newNestedTypeList = Expression.New(typeof(List<>).MakeGenericType(typeof(OuterClass.InnerClass)));

            var translated = ToReadableString(newNestedTypeList);

            translated.ShouldBe("new List<OuterClass.InnerClass>()");
        }

        [Fact]
        public void ShouldUseFriendlyNamesForGenericNestedTypes()
        {
            var genericListEnumeratorType = Expression.Constant(typeof(List<string>.Enumerator), typeof(Type));

            var translated = ToReadableString(genericListEnumeratorType);

            translated.ShouldBe("typeof(List<string>.Enumerator)");
        }

        [Fact]
        public void ShouldUseFriendlyNamesForNestedGenericTypes()
        {
            var genericListEnumeratorType = Expression.Constant(typeof(GenericTestHelper<int>), typeof(Type));

            var translated = ToReadableString(genericListEnumeratorType);

            translated.ShouldBe("typeof(WhenGettingFriendlyNames.GenericTestHelper<int>)");
        }

        [Fact]
        public void ShouldUseFriendlyNamesForGenericMultiplyNestedTypes()
        {
            var nestedGenericType = Expression.Constant(
                typeof(OuterGeneric<int>.InnerGeneric<long>.Nested),
                typeof(Type));

            var translated = ToReadableString(nestedGenericType);

            translated.ShouldBe("typeof(OuterGeneric<int>.InnerGeneric<long>.Nested)");
        }

        #region Helper Classes

        // ReSharper disable once UnusedTypeParameter
        private class GenericTestHelper<T>
        {
        }

        #endregion
    }
}
