namespace AgileObjects.ReadableExpressions.UnitTests.Extensions;

using System.Collections;
using System.Collections.Generic;
using ReadableExpressions.Extensions;

#if NET35
[NUnitTestFixture]
#endif
public class WhenGeneratingVariableNames
{
    [Fact]
    public void ShouldNameAStringVariable()
    {
        typeof(string).GetVariableNameInCamelCase().ShouldBe("string");
    }

    [Fact]
    public void ShouldNameAnArrayTypeVariable()
    {
        typeof(Box[]).GetVariableNameInCamelCase().ShouldBe("boxArray");
    }

    [Fact]
    public void ShouldNameAnIEnumerableTypeVariable()
    {
        typeof(IEnumerable<Fuzz>).GetVariableNameInPascalCase().ShouldBe("FuzzIEnumerable");
    }

    [Fact]
    public void ShouldNameAnICollectionTypeVariable()
    {
        typeof(ICollection<Box>).GetVariableNameInCamelCase().ShouldBe("boxICollection");
    }

    [Fact]
    public void ShouldNameAnIListTypeVariable()
    {
        typeof(IList<Body>).GetVariableNameInPascalCase().ShouldBe("BodyIList");
    }

    [Fact]
    public void ShouldNameAListTypeVariable()
    {
        typeof(List<Church>).GetVariableNameInCamelCase().ShouldBe("churchList");
    }

    [Fact]
    public void ShouldNameAHashSetTypeVariable()
    {
        typeof(HashSet<int>).GetVariableNameInCamelCase().ShouldBe("intHashSet");
    }

    [Fact]
    public void ShouldNameAnArrayListVariable()
    {
        typeof(ArrayList).GetVariableNameInCamelCase().ShouldBe("arrayList");
    }

    [Fact]
    public void ShouldNameADictionaryTypeVariable()
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
    public void ShouldNameAGenericTypeInnerClassVariable()
    {
        typeof(Issue48<int>.Inner)
            .GetVariableNameInPascalCase()
            .ShouldBe($"{nameof(WhenGeneratingVariableNames)}_IntIssue48_Inner");
    }

    [Fact]
    public void ShouldNameAGenericGenericTypeArgument()
    {
        typeof(Dictionary<int, Dictionary<string, List<byte>>>)
            .GetVariableNameInPascalCase()
            .ShouldBe("IntStringByteListDictionaryDictionary");
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