namespace AgileObjects.ReadableExpressions.UnitTests;

using System.Collections.Generic;
using System.Linq;

#if NET35
[NUnitTestFixture]
#endif
public class WhenShowingCapturedValues : TestClassBase
{
    // See https://github.com/agileobjects/ReadableExpressions/issues/86
    [Fact]
    public void ShouldIncludeCapturedLocalVariableValues()
    {
        var value = int.Parse("123");

        var capturedLocalVariableLamda = CreateLambda(
            (PropertiesHelper helper) => helper.PublicInstance == value);

        var translated = capturedLocalVariableLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helper.PublicInstance == 123");
    }

    [Fact]
    public void ShouldIncludeCapturedInstanceFieldValues()
    {
        var capture = new FieldsHelper { PublicInstance = 7238 };

        var capturedInstanceFieldLamda = CreateLambda(
            (PropertiesHelper helper) => helper.PublicInstance == capture.PublicInstance);

        var translated = capturedInstanceFieldLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helper.PublicInstance == 7238");
    }

    [Fact]
    public void ShouldIncludeCapturedInstancePropertyValues()
    {
        var capture = new PropertiesHelper { PublicInstance = 999 };

        var capturedInstancePropertyLamda = CreateLambda(
            (PropertiesHelper helper) => helper.PublicInstance == capture.PublicInstance);

        var translated = capturedInstancePropertyLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helper.PublicInstance == 999");
    }

    [Fact]
    public void ShouldIncludeCapturedStaticFieldValues()
    {
        FieldsHelper.PublicStatic = 90210;

        var capturedStaticFieldLamda = CreateLambda(
            (FieldsHelper helper) => helper.PublicInstance == FieldsHelper.PublicStatic);

        var translated = capturedStaticFieldLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helper.PublicInstance == 90210");
    }

    [Fact]
    public void ShouldIncludeCapturedStaticBclFieldValues()
    {
        var capturedStaticFieldLamda = CreateLambda(
            (FieldsHelper helper) => helper.PublicInstance.ToString() == string.Empty);

        var translated = capturedStaticFieldLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helper.PublicInstance.ToString() == \"\"");
    }

    [Fact]
    public void ShouldIncludeCapturedStaticPropertyValues()
    {
        PropertiesHelper.PublicStatic = 456;

        var capturedStaticPropertyLamda = CreateLambda(
            (PropertiesHelper helper) => helper.PublicInstance == PropertiesHelper.PublicStatic);

        var translated = capturedStaticPropertyLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helper.PublicInstance == 456");
    }

    // See https://github.com/agileobjects/ReadableExpressions/issues/129
    // See https://github.com/agileobjects/ReadableExpressions/issues/133
    [Fact]
    public void ShouldHandleCapturedNestedEnumCollectionLinqFirstAccess()
    {
        var source = new ValueWrapper<List<ValueWrapper<OddNumber>>>
        {
            Value = new() { new() { Value = OddNumber.One } }
        };

        var capturedLocalVariableLamda = CreateLambda(
            (List<ValueWrapper<OddNumber>> helpers) =>
                helpers.Any(h => h.Value == source.Value.First().Value));

        var translated = capturedLocalVariableLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helpers.Any(h => h.Value == OddNumber.One)");
    }

    [Fact]
    public void ShouldHandleCapturedNestedEnumCollectionLinqFirstOrDefaultAccess()
    {
        var source = new ValueWrapper<List<ValueWrapper<OddNumber>>>
        {
            Value = new() { new() { Value = OddNumber.Three } }
        };

        var capturedLocalVariableLamda = CreateLambda(
            (IEnumerable<ValueWrapper<OddNumber>> helpers) =>
                helpers.Any(h => h.Value == source.Value.FirstOrDefault().Value));

        var translated = capturedLocalVariableLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helpers.Any(h => h.Value == OddNumber.Three)");
    }

    [Fact]
    public void ShouldHandleCapturedNestedEnumCollectionLinqLastAccess()
    {
        var source = new ValueWrapper<List<ValueWrapper<OddNumber>>>
        {
            Value = new() { new() { Value = OddNumber.Three } }
        };

        var capturedLocalVariableLamda = CreateLambda(
            (IEnumerable<ValueWrapper<OddNumber>> helpers) =>
                helpers.Any(h => h.Value == source.Value.Last().Value));

        var translated = capturedLocalVariableLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helpers.Any(h => h.Value == OddNumber.Three)");
    }

    [Fact]
    public void ShouldHandleCapturedNestedEnumCollectionLinqLastOrDefaultAccess()
    {
        var source = new ValueWrapper<List<ValueWrapper<OddNumber>>>
        {
            Value = new() { new() { Value = OddNumber.One } }
        };

        var capturedLocalVariableLamda = CreateLambda(
            (ICollection<ValueWrapper<OddNumber>> helpers) =>
                helpers.Any(h => h.Value == source.Value.LastOrDefault().Value));

        var translated = capturedLocalVariableLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helpers.Any(h => h.Value == OddNumber.One)");
    }

    [Fact]
    public void ShouldHandleCapturedNestedIntCollectionLinqAnyAccess()
    {
        var source = new ValueWrapper<List<int>>
        {
            Value = new() { 1, 2, 3 }
        };

        var capturedLocalVariableLamda = CreateLambda(
            (ValueWrapper<bool> helper) => helper.Value == source.Value.Any());

        var translated = capturedLocalVariableLamda.Body
            .ToReadableString(stgs => stgs.ShowCapturedValues);

        translated.ShouldBe("helper.Value == true");
    }
}