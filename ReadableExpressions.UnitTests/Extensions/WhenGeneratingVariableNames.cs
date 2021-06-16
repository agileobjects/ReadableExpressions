namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System.Collections;
    using System.Collections.Generic;
    using Common;
    using ReadableExpressions.Extensions;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenGeneratingVariableNames
    {
        [Fact]
        public void ShouldNameAVariableForAnArrayType()
        {
            typeof(Box[]).GetVariableNameInCamelCase().ShouldBe("boxArray");
        }

        [Fact]
        public void ShouldNameAVariableForAnIEnumerableType()
        {
            typeof(IEnumerable<Fuzz>).GetVariableNameInPascalCase().ShouldBe("FuzzIEnumerable");
        }

        [Fact]
        public void ShouldNameAVariableForAnICollectionType()
        {
            typeof(ICollection<Box>).GetVariableNameInCamelCase().ShouldBe("boxICollection");
        }

        [Fact]
        public void ShouldNameAVariableForAnIListType()
        {
            typeof(IList<Body>).GetVariableNameInPascalCase().ShouldBe("BodyIList");
        }

        [Fact]
        public void ShouldNameAVariableForAListType()
        {
            typeof(List<Church>).GetVariableNameInCamelCase().ShouldBe("churchList");
        }

        [Fact]
        public void ShouldNameAVariableForAHashSetType()
        {
            typeof(HashSet<int>).GetVariableNameInCamelCase().ShouldBe("intHashSet");
        }

        [Fact]
        public void ShouldNameAVariableForAnArrayListType()
        {
            typeof(ArrayList).GetVariableNameInCamelCase().ShouldBe("arrayList");
        }

        [Fact]
        public void ShouldNameAVariableForADictionaryType()
        {
            typeof(Dictionary<string, Church>).GetVariableNameInCamelCase().ShouldBe("stringChurchDictionary");
        }

        [Fact]
        public void ShouldNameANullableLongVariable()
        {
            typeof(long?).GetVariableNameInCamelCase().ShouldBe("nullableLong");
        }

        [Fact]
        public void ShouldNameAnArrayOfArraysVariable()
        {
            typeof(int?[][]).GetVariableNameInCamelCase().ShouldBe("nullableIntArrayArray");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/48
        [Fact]
        public void ShouldNameAnInnerClassOfAGenericType()
        {
            typeof(Issue48<int>.Inner)
                .GetVariableNameInPascalCase()
                .ShouldBe($"{nameof(WhenGeneratingVariableNames)}_IntIssue48_Inner");
        }

        #region Helper Members

        // ReSharper disable ClassNeverInstantiated.Local
        private class Box { }

        private class Fuzz { }

        private class Church { }

        private class Body { }

        // ReSharper disable once UnusedTypeParameter
        private class Issue48<T>
        {
            public class Inner { }
        }
        // ReSharper restore ClassNeverInstantiated.Local

        #endregion
    }
}
