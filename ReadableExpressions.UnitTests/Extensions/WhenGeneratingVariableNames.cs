namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
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
        public void ShouldNameAVariableForACollectionTypeEndingInX()
        {
            typeof(ICollection<Box>).GetVariableNameInCamelCase().ShouldBe("boxes");
        }

        [Fact]
        public void ShouldNameAVariableForAnEnumerableTypeEndingInZ()
        {
            typeof(IEnumerable<Fuzz>).GetVariableNameInPascalCase().ShouldBe("Fuzzes");
        }

        [Fact]
        public void ShouldNameAVariableForAnEnumerableTypeEndingInDoubleS()
        {
            typeof(IEnumerable<Glass>).GetVariableNameInPascalCase().ShouldBe("Glasses");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInCh()
        {
            typeof(List<Church>).GetVariableNameInCamelCase().ShouldBe("churches");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInSh()
        {
            typeof(List<Hush>).GetVariableNameInCamelCase().ShouldBe("hushes");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInVowelY()
        {
            typeof(List<Journey>).GetVariableNameInCamelCase().ShouldBe("journeys");
        }

        [Fact]
        public void ShouldNameAVariableForAnIListTypeEndingInConsonantY()
        {
            typeof(IList<Body>).GetVariableNameInPascalCase().ShouldBe("Bodies");
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
                .ShouldBe($"{nameof(WhenGeneratingVariableNames)}__Issue48_Int__Inner");
        }

        #region Helper Members

        // ReSharper disable ClassNeverInstantiated.Local
        private class Box { }

        private class Fuzz { }

        private class Glass { }

        private class Church { }

        private class Hush { }

        private class Journey { }

        private class Body { }

        private class Issue48<T>
        {
            public class Inner { }
        }
        // ReSharper restore ClassNeverInstantiated.Local

        #endregion
    }
}
