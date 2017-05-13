namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ReadableExpressions.Extensions;

    [TestClass]
    public class WhenGettingFriendlyNames
    {
        [TestMethod]
        public void ShouldUseFriendlyNamesForArrays()
        {
            var intArrayVariable = Expression.Variable(typeof(int[]), "ints");
            var assignNull = Expression.Assign(intArrayVariable, Expression.Default(intArrayVariable.Type));
            var assignNullBlock = Expression.Block(new[] { intArrayVariable }, assignNull);

            var translated = assignNullBlock.ToReadableString();

            Assert.AreEqual("var ints = default(int[]);", translated);
        }

        [TestMethod]
        public void ShouldUseFriendlyNamesForCharacters()
        {
            Expression<Func<char, double>> characterToNumeric = c => char.GetNumericValue(c);

            var translated = characterToNumeric.ToReadableString();

            Assert.AreEqual("c => char.GetNumericValue(c)", translated);
        }

        [TestMethod]
        public void ShouldUseFriendlyNamesForAnonymousTypes()
        {
            var anon = new { One = 1, Two = "two" };

            var friendlyName = anon.GetType().GetFriendlyName();

            Assert.AreEqual("AnonymousType<int, string>", friendlyName);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/6
        [TestMethod]
        public void ShouldUseFriendlyNamesForMultiplyNestedTypes()
        {
            var nestedType = Expression.Constant(typeof(OuterClass.InnerClass.Nested), typeof(Type));

            var translated = nestedType.ToReadableString();

            Assert.AreEqual("typeof(OuterClass.InnerClass.Nested)", translated);
        }

        [TestMethod]
        public void ShouldUseFriendlyNamesForListsOfNestedTypes()
        {
            var newNestedTypeList = Expression.New(typeof(List<>).MakeGenericType(typeof(OuterClass.InnerClass)));

            var translated = newNestedTypeList.ToReadableString();

            Assert.AreEqual("new List<OuterClass.InnerClass>()", translated);
        }

        [TestMethod]
        public void ShouldUseFriendlyNamesForGenericNestedTypes()
        {
            var genericListEnumeratorType = Expression.Constant(typeof(List<string>.Enumerator), typeof(Type));

            var translated = genericListEnumeratorType.ToReadableString();

            Assert.AreEqual("typeof(List<string>.Enumerator)", translated);
        }

        [TestMethod]
        public void ShouldUseFriendlyNamesForGenericMultiplyNestedTypes()
        {
            var nestedGenericType = Expression.Constant(
                typeof(OuterGeneric<int>.InnerGeneric<long>.Nested),
                typeof(Type));

            var translated = nestedGenericType.ToReadableString();

            Assert.AreEqual("typeof(OuterGeneric<int>.InnerGeneric<long>.Nested)", translated);
        }
    }
}
