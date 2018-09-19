namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System.Collections.Generic;
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
            typeof(Box[]).GetVariableNameInCamelCase(new TranslationSettings()).ShouldBe("boxArray");
        }

        [Fact]
        public void ShouldNameAVariableForACollectionTypeEndingInX()
        {
            typeof(ICollection<Box>).GetVariableNameInCamelCase(new TranslationSettings()).ShouldBe("boxes");
        }

        [Fact]
        public void ShouldNameAVariableForAnEnumerableTypeEndingInZ()
        {
            typeof(IEnumerable<Fuzz>).GetVariableNameInPascalCase(new TranslationSettings()).ShouldBe("Fuzzes");
        }

        [Fact]
        public void ShouldNameAVariableForAnEnumerableTypeEndingInDoubleS()
        {
            typeof(IEnumerable<Glass>).GetVariableNameInPascalCase(new TranslationSettings()).ShouldBe("Glasses");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInCh()
        {
            typeof(List<Church>).GetVariableNameInCamelCase(new TranslationSettings()).ShouldBe("churches");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInSh()
        {
            typeof(List<Hush>).GetVariableNameInCamelCase(new TranslationSettings()).ShouldBe("hushes");
        }

        [Fact]
        public void ShouldNameAVariableForAListTypeEndingInVowelY()
        {
            typeof(List<Journey>).GetVariableNameInCamelCase(new TranslationSettings()).ShouldBe("journeys");
        }

        [Fact]
        public void ShouldNameAVariableForAnIListTypeEndingInConsonantY()
        {
            typeof(IList<Body>).GetVariableNameInPascalCase(new TranslationSettings()).ShouldBe("Bodies");
        }

        [Fact]
        public void ShouldNameANullableLongVariable()
        {
            typeof(long?).GetVariableNameInCamelCase(new TranslationSettings()).ShouldBe("nullableLong");
        }

        [Fact]
        public void ShouldNameAnArrayOfArraysVariable()
        {
            typeof(int?[][]).GetVariableNameInCamelCase(new TranslationSettings()).ShouldBe("nullableIntArrayArray");
        }

        // ReSharper disable ClassNeverInstantiated.Local
        private class Box { }

        private class Fuzz { }

        private class Glass { }

        private class Church { }

        private class Hush { }

        private class Journey { }

        private class Body { }
        // ReSharper restore ClassNeverInstantiated.Local
    }
}
