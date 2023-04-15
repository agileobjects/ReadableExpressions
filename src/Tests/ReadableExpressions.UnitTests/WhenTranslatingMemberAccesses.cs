namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using Common;
    using Common.Vb;
    using NetStandardPolyfills;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;
    using static Microsoft.Scripting.Ast.Expression;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingMemberAccesses : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAnArrayLengthExpression()
        {
            var getArrayLength = CreateLambda((string[] a) => a.Length);

            var translated = getArrayLength.ToReadableString();

            translated.ShouldBe("a => a.Length");
        }

        [Fact]
        public void ShouldTranslateAConstantStringLengthExpression()
        {
            var getConstantLength = Property(Constant("Constant!"), "Length");
            
            var translated = getConstantLength.ToReadableString();

            translated.ShouldBe("\"Constant!\".Length");
        }

        [Fact]
        public void ShouldTranslateAnInstanceCallExpression()
        {
            var objectToString = CreateLambda((object o) => o.ToString());

            var translated = objectToString.ToReadableString();

            translated.ShouldBe("o => o.ToString()");
        }

        [Fact]
        public void ShouldTranslateAnInstanceCallExpressionStaticMemberArgument()
        {
            var intToFormattedString = CreateLambda((int i) => i.ToString(CultureInfo.CurrentCulture));

            var translated = intToFormattedString.ToReadableString();

            translated.ShouldBe("i => i.ToString(CultureInfo.CurrentCulture)");
        }

        [Fact]
        public void ShouldTranslateAnInstanceCallExpressionParameterArgument()
        {
            var intToFormattedString = CreateLambda((int i, CultureInfo ci) => i.ToString(ci));

            var translated = intToFormattedString.ToReadableString();

            translated.ShouldBe("(i, ci) => i.ToString(ci)");
        }

        [Fact]
        public void ShouldTranslateAParameterlessExtensionMethodCall()
        {
            var arrayIsEmpty = CreateLambda((string[] a) => a.Any());

            var translated = arrayIsEmpty.ToReadableString();

            translated.ShouldBe("a => a.Any()");
        }

        [Fact]
        public void ShouldTranslateAnExtensionMethodCallWithSimpleParameters()
        {
            var notTheFirstTwo = CreateLambda((string[] a) => a.Skip(2));

            var translated = notTheFirstTwo.ToReadableString();

            translated.ShouldBe("a => a.Skip(2)");
        }

        [Fact]
        public void ShouldTranslateAnExtensionMethodCallWithALambdaParameter()
        {
            var noBlankStrings = CreateLambda((string[] a) => a.All(i => i.Length != 0));

            var translated = noBlankStrings.ToReadableString();

            translated.ShouldBe("a => a.All(i => i.Length != 0)");
        }

        [Fact]
        public void ShouldTranslateAnExtensionMethodCallWithTypedLambdaParameter()
        {
            var noBlankStrings = CreateLambda((string[] a) => a.All(i => i.Length != 0));

            var translated = noBlankStrings.ToReadableString(stgs => stgs.ShowLambdaParameterTypes);

            translated.ShouldBe("(string[] a) => a.All((string i) => i.Length != 0)");
        }

        [Fact]
        public void ShouldTranslateAStaticCallExpression()
        {
            var oneEqualsTwo = CreateLambda(() => ReferenceEquals("1", "2"));

            var translated = oneEqualsTwo.ToReadableString();

            translated.ShouldBe("() => object.ReferenceEquals(\"1\", \"2\")");
        }

        [Fact]
        public void ShouldTranslateAStaticCallExpressionOnAGenericType()
        {
            var doSomething = CreateLambda(() => GenericHelper<Dictionary<DateTime, string>>.DoSomething());

            var translated = doSomething.Body.ToReadableString();

            translated.ShouldBe("GenericHelper<Dictionary<DateTime, string>>.DoSomething()");
        }

        [Fact]
        public void ShouldTranslateAnInstanceMemberExpression()
        {
            var getDateDay = CreateLambda((DateTime d) => d.Day);

            var translated = getDateDay.ToReadableString();

            translated.ShouldBe("d => d.Day");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/101
        [Fact]
        public void ShouldTranslateAnExtensionExpressionMemberExpression()
        {
            var extension = new ExtensionExpression(typeof(PropertiesHelper));

            var extensionMemberAccess = Property(
                extension,
                nameof(PropertiesHelper.PublicInstance));

            var translated = extensionMemberAccess.ToReadableString();

            translated.ShouldBe($"{extension}.PublicInstance");
        }

        [Fact]
        public void ShouldTranslateAStaticMemberExpression()
        {
            var getToday = CreateLambda(() => DateTime.Today);

            var translated = getToday.ToReadableString();

            translated.ShouldBe("() => DateTime.Today");
        }

        [Fact]
        public void ShouldTranslateAStaticMemberExpressionUsingTheDeclaringType()
        {
            var getDefaultIndexedProperty = CreateLambda(() => IndexedProperty.Default);

            var translated = getDefaultIndexedProperty.Body.ToReadableString();

            translated.ShouldBe("IndexedProperty.Default");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/35
        [Fact]
        public void ShouldTranslateAnInstancePropertyGetterCall()
        {
            var publicInstanceGetter = typeof(PropertiesHelper)
                .GetPublicInstanceProperty(nameof(PropertiesHelper.PublicInstance))
                .GetGetter();

            publicInstanceGetter.ShouldNotBeNull();

            var variable = Variable(typeof(PropertiesHelper), "helper");
            var getterAccess = Call(variable, publicInstanceGetter);

            var translated = getterAccess.ToReadableString();

            translated.ShouldBe("helper.PublicInstance");
        }

        [Fact]
        public void ShouldTranslateAnInstancePropertySetterCall()
        {
            var publicInstanceSetter = typeof(PropertiesHelper)
                .GetPublicInstanceProperty(nameof(PropertiesHelper.PublicInstance))
                .GetSetter();

            publicInstanceSetter.ShouldNotBeNull();

            var variable = Variable(typeof(PropertiesHelper), "helper");
            var setterCall = Call(variable, publicInstanceSetter, Constant(123));

            var translated = setterCall.ToReadableString();

            translated.ShouldBe("helper.PublicInstance = 123");
        }

        [Fact]
        public void ShouldTranslateAStaticPropertyGetterCall()
        {
            var publicStaticGetter = typeof(PropertiesHelper)
                .GetPublicStaticProperty(nameof(PropertiesHelper.PublicStatic))
                .GetGetter();

            publicStaticGetter.ShouldNotBeNull();

            var getterAccess = Call(publicStaticGetter);

            var translated = getterAccess.ToReadableString();

            translated.ShouldBe("PropertiesHelper.PublicStatic");
        }

        [Fact]
        public void ShouldTranslateAStaticPropertySetterCall()
        {
            var publicStaticSetter = typeof(PropertiesHelper)
                .GetPublicStaticProperty(nameof(PropertiesHelper.PublicStatic))
                .GetSetter();

            publicStaticSetter.ShouldNotBeNull();

            var setterCall = Call(publicStaticSetter, Constant(456));

            var translated = setterCall.ToReadableString();

            translated.ShouldBe("PropertiesHelper.PublicStatic = 456");
        }

        [Fact]
        public void ShouldTranslateANonPublicInstancePropertyGetterCall()
        {
            var publicInstanceGetter = typeof(PropertiesHelper)
                .GetNonPublicInstanceProperty(nameof(PropertiesHelper.NonPublicInstance))
                .GetGetter(nonPublic: true);

            publicInstanceGetter.ShouldNotBeNull();

            var variable = Variable(typeof(PropertiesHelper), "helper");
            var getterAccess = Call(variable, publicInstanceGetter);

            var translated = getterAccess.ToReadableString();

            translated.ShouldBe("helper.NonPublicInstance");
        }

        [Fact]
        public void ShouldTranslateANonPublicInstancePropertySetterCall()
        {
            var nonPublicInstanceSetter = typeof(PropertiesHelper)
                .GetNonPublicInstanceProperty(nameof(PropertiesHelper.NonPublicInstance))
                .GetSetter(nonPublic: true);

            nonPublicInstanceSetter.ShouldNotBeNull();

            var variable = Variable(typeof(PropertiesHelper), "helper");
            var setterCall = Call(variable, nonPublicInstanceSetter, Constant(123));

            var translated = setterCall.ToReadableString();

            translated.ShouldBe("helper.NonPublicInstance = 123");
        }

        [Fact]
        public void ShouldTranslateANonPublicStaticPropertyGetterCall()
        {
            var nonPublicStaticGetter = typeof(PropertiesHelper)
                .GetNonPublicStaticProperty(nameof(PropertiesHelper.NonPublicStatic))
                .GetGetter(nonPublic: true);

            nonPublicStaticGetter.ShouldNotBeNull();

            var getterAccess = Call(nonPublicStaticGetter);

            var translated = getterAccess.ToReadableString();

            translated.ShouldBe("PropertiesHelper.NonPublicStatic");
        }

        [Fact]
        public void ShouldTranslateANonPublicStaticPropertySetterCall()
        {
            var nonPublicStaticSetter = typeof(PropertiesHelper)
                .GetNonPublicStaticProperty(nameof(PropertiesHelper.NonPublicStatic))
                .GetSetter(nonPublic: true);

            nonPublicStaticSetter.ShouldNotBeNull();

            var setterCall = Call(nonPublicStaticSetter, Constant(456));

            var translated = setterCall.ToReadableString();

            translated.ShouldBe("PropertiesHelper.NonPublicStatic = 456");
        }

        [Fact]
        public void ShouldTranslateAParamsArrayArgument()
        {
            var splitString = CreateLambda((string str) => str.Split('x', 'y', 'z'));

            var translated = splitString.ToReadableString();

            translated.ShouldBe("str => str.Split('x', 'y', 'z')");
        }

        [Fact]
        public void ShouldTranslateAnEmptyParamsArrayArgument()
        {
            var combineStrings = CreateLambda((string str) => ParamsHelper.OptionalParams(str));

            var translated = combineStrings.ToReadableString();

            translated.ShouldBe("str => ParamsHelper.OptionalParams(str)");
        }

        [Fact]
        public void ShouldTranslateMultiParameterCallParamsArrayArgument()
        {
            var stringJoinMethod = typeof(string)
                .GetPublicStaticMethods(nameof(string.Join))
                .First(m =>
                    (m.GetParameters().Length == 2) &&
                    (m.GetParameters()[0].ParameterType == typeof(string)) &&
                    (m.GetParameters()[1].ParameterType == typeof(string[])));

            var stringEmpty = Field(null, typeof(string).GetPublicStaticField(nameof(string.Empty)));
            var expressions = new Expression[] { Constant("Value["), Constant("0"), Constant("]") };
            var newStringArray = NewArrayInit(typeof(string), expressions);

            var stringJoinCall = Call(stringJoinMethod, stringEmpty, newStringArray);

            var translated = stringJoinCall.ToReadableString();

            // string.Join(string, string[]) in .NET 3.5 doesn't take a params array:
#if NET35
            const string EXPECTED = @"
string.Join(string.Empty, new[] { ""Value["", ""0"", ""]"" })";
#else
            const string EXPECTED = @"
string.Join(
    string.Empty,
    ""Value["",
    ""0"",
    ""]"")";
#endif
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnIndexedPropertyAccessExpression()
        {
            var getPropertyIndex = CreateLambda((IndexedProperty p, int index) => p[index]);

            var translated = getPropertyIndex.Body.ToReadableString();

            translated.ShouldBe("p[index]");
        }

        [Fact]
        public void ShouldTranslateAManualIndexedPropertyAccessExpression()
        {
            var indexedProperty = Variable(typeof(IndexedProperty), "p");
            var property = indexedProperty.Type.GetPublicInstanceProperties().First();
            var firstElement = Constant(1, typeof(int));

            var indexerAccess = MakeIndex(indexedProperty, property, new[] { firstElement });

            var translated = indexerAccess.ToReadableString();

            translated.ShouldBe("p[1]");
        }

#if FEATURE_PROPERTY_INDEX_DEFAULTS
        [Fact]
        public void ShouldTranslateANamedIndexedPropertyAccessExpression()
        {
            var instance = Variable(typeof(PublicNamedIndex<int>), "instance");
            var indexProperty = instance.Type.GetPublicInstanceProperty("Value");
            var indexAccess = Property(instance, indexProperty);

            var translated = indexAccess.ToReadableString();

            translated.ShouldBe("instance.get_Value(1, null)");
        }
#endif
        [Fact]
        public void ShouldTranslateANamedIndexedPropertyAccessExpressionWithArguments()
        {
            var instance = Variable(typeof(PublicNamedIndex<int>), "instance");
            var indexProperty = instance.Type.GetPublicInstanceProperty("Value");
            var index1 = Constant(123);
            var index2 = Constant(456, typeof(int?));
            var indexAccess = Property(instance, indexProperty, index1, index2);

            var translated = indexAccess.ToReadableString();

            translated.ShouldBe("instance.get_Value(123, 456)");
        }

        [Fact]
        public void ShouldTranslateAStringIndexAccessExpression()
        {
            var getFirstCharacter = CreateLambda((string str) => str[0]);

            var translated = getFirstCharacter.Body.ToReadableString();

            translated.ShouldBe("str[0]");
        }

        [Fact]
        public void ShouldTranslateAnArrayIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((int[] items) => items[0]);

            var translated = getFirstItem.Body.ToReadableString();

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAnIDictionaryIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((IDictionary<int, string> items) => items[0]);

            var translated = getFirstItem.Body.ToReadableString();

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateADictionaryIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((Dictionary<long, string> items) => items[0]);

            var translated = getFirstItem.Body.ToReadableString();

            translated.ShouldBe("items[0L]");
        }

        [Fact]
        public void ShouldTranslateACollectionIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((Collection<string> items) => items[0]);

            var translated = getFirstItem.Body.ToReadableString();

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAnIListIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((IList<string> items) => items[0]);

            var translated = getFirstItem.Body.ToReadableString();

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAListIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((List<string> items) => items[0]);

            var translated = getFirstItem.Body.ToReadableString();

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithExplicitGenericArgumentIncluded()
        {
            var getFirstItem = CreateLambda((IndexedProperty ip) => ip.GetFirst<string>());

            var translated = getFirstItem.Body.ToReadableString();

            translated.ShouldBe("ip.GetFirst<string>()");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithPartlyImpliedTypeParameters()
        {
            var convertIntToString = CreateLambda((int i) => new ValueConverter().Convert<int, string>(i));

            var translated = convertIntToString.Body.ToReadableString();

            translated.ShouldBe("new ValueConverter().Convert<int, string>(i)");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithoutImplicitGenericArgumentIncluded()
        {
            var setFirstItem = CreateLambda((IndexedProperty ip, string str) => ip.SetFirst(str));

            var translated = setFirstItem.Body.ToReadableString();

            translated.ShouldBe("ip.SetFirst(str)");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithRequestedGenericArgumentsIncluded()
        {
            var setFirstItem = CreateLambda((IndexedProperty ip, string str) => ip.SetFirst(str));

            var translated = setFirstItem.Body.ToReadableString(stgs => stgs.UseExplicitGenericParameters);

            translated.ShouldBe("ip.SetFirst<string>(str)");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithGenericArgumentGivenByDefaultOperator()
        {
            var setFirstItem = CreateLambda((IndexedProperty ip) => ip.SetFirst(default(string)));

            var translated = setFirstItem.Body.ToReadableString();

            translated.ShouldBe("ip.SetFirst(default(string))");
        }

        [Fact]
        public void ShouldTranslateAnOutParameterMethodCallWithoutGenericArgumentIncluded()
        {
            string result;

            var convertIntToString = CreateLambda((int i) => new ValueConverter().TryConvert(i, out result));

            var translated = convertIntToString.Body.ToReadableString();

            translated.ShouldBe("new ValueConverter().TryConvert(i, out result)");
        }

        [Fact]
        public void ShouldTranslateANegatedMethodCall()
        {
            var listDoesNotContainZero = CreateLambda((List<int> l)
                => !l.Contains(0, EqualityComparer<int>.Default));

            var translated = listDoesNotContainZero.Body.ToReadableString();

            translated.ShouldBe("!l.Contains(0, EqualityComparer<int>.Default)");
        }

        [Fact]
        public void ShouldNotIncludeCapturedInstanceNames()
        {
            var helper = new CapturedInstanceHelper(5);
            var translated = helper.GetComparisonTranslation(3);

            translated.ShouldBe("_i == comparator");
        }

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

        [Fact]
        public void ShouldIncludeOutParameterKeywords()
        {
            var helperVariable = Variable(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperVariable, tryGetMethod, one, valueVariable);

            var translated = tryGetCall.ToReadableString();

            translated.ShouldBe("ip.TryGet(1, out value)");
        }

        [Fact]
        public void ShouldIncludeAnOutParameterVariableDeclaration()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, object>>(tryGetBlock, helperParameter);

            var translated = tryGetLambda.ToReadableString();

            const string EXPECTED = @"
ip =>
{
    object value;
    ip.TryGet(1, out value);

    return value;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotDuplicateAnOutParameterVariableDeclarationType()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, object>>(tryGetBlock, helperParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs.ShowLambdaParameterTypes);

            const string EXPECTED = @"
(IndexedProperty ip) =>
{
    object value;
    ip.TryGet(1, out value);

    return value;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAnInlineOutParameterVariable()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, object>>(tryGetBlock, helperParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs
                .DeclareOutputParametersInline);

            const string EXPECTED = @"
ip =>
{
    ip.TryGet(1, out var value);

    return value;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAnExplicitlyTypedInlineOutParameterVariable()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, object>>(tryGetBlock, helperParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs
                .DeclareOutputParametersInline
                .UseExplicitTypeNames);

            const string EXPECTED = @"
ip =>
{
    ip.TryGet(1, out object value);

    return value;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAnExplicitlyTypedFullyQualifiedInlineOutParameterVariable()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(List<int>), "value");

            var tryGetMethod = typeof(IndexedProperty)
                .GetPublicInstanceMethod("TryGetGeneric")
                .MakeGenericMethod(valueVariable.Type);

            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, List<int>>>(tryGetBlock, helperParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs
                .UseFullyQualifiedTypeNames
                .UseExplicitTypeNames
                .DeclareOutputParametersInline);

            const string EXPECTED = @"
ip =>
{
    ip.TryGetGeneric(1, out System.Collections.Generic.List<int> value);

    return value;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeAnInlineOutParameterDiscard()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var intParameter = Parameter(typeof(int), "i");
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, intParameter, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall);

            var tryGetLambda = Lambda<Func<IndexedProperty, int, bool>>(
                tryGetBlock,
                helperParameter,
                intParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs
                .DeclareOutputParametersInline
                .DiscardUnusedParameters);

            translated.ShouldBe("(ip, i) => ip.TryGet(i, out _)");
        }

        [Fact]
        public void ShouldNotDiscardAPreviouslyUsedOutParameter()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var valueVariable = Variable(typeof(object), "value");
            var assignValue = Assign(valueVariable, Constant(123, typeof(object)));

            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var zero = Constant(0);
            var tryGetCall = Call(helperParameter, tryGetMethod, zero, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, assignValue, tryGetCall);

            var tryGetLambda = Lambda<Func<IndexedProperty, bool>>(
                tryGetBlock,
                helperParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs
                .DeclareOutputParametersInline
                .DiscardUnusedParameters);

            const string EXPECTED = @"
ip =>
{
    var value = 123;

    return ip.TryGet(0, out value);
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldOnlyDeclareAnOutParameterVariableInlineOnce()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, object>>(tryGetBlock, helperParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs.DeclareOutputParametersInline);

            const string EXPECTED = @"
ip =>
{
    ip.TryGet(1, out var value);
    ip.TryGet(1, out value);

    return value;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldNotDeclareAnInitialisedOutParameterVariableInline()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var valueAssignment = Assign(valueVariable, Default(valueVariable.Type));
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, valueAssignment, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, object>>(tryGetBlock, helperParameter);

            var translated = tryGetLambda.ToReadableString(stgs => stgs.DeclareOutputParametersInline);

            const string EXPECTED = @"
ip =>
{
    var value = default(object);
    ip.TryGet(1, out value);

    return value;
}";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldIncludeRefParameterKeywords()
        {
            var helperVariable = Variable(typeof(IndexedProperty), "ip");
            var three = Constant(3);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("RefGet");
            var tryGetCall = Call(helperVariable, tryGetMethod, three, valueVariable);

            var translated = tryGetCall.ToReadableString();

            translated.ShouldBe("ip.RefGet(3, ref value)");
        }

        [Fact]
        public void ShouldTranslateACustomEnumerableAddInitialiser()
        {
            var customAdder = CreateLambda((int intOne, int intTwo, int intThree)
                => new CustomAdder { { intOne, intTwo, intThree } });

            var translated = customAdder.Body.ToReadableString();

            const string EXPECTED = @"
new CustomAdder
{
    { intOne, intTwo, intThree }
}";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateImplicitOperatorUse()
        {
            var adderToString = CreateLambda<string>(() => new CustomAdder());

            var stringVariable = Variable(typeof(string), "str");
            var assignment = Assign(stringVariable, adderToString.Body);

            var translated = assignment.ToReadableString();

            translated.ShouldBe("str = new CustomAdder()");
        }

        [Fact]
        public void ShouldTranslateImplicitMethodOperatorUse()
        {
            var stringVariable = Variable(typeof(string), "str");
            var stringOperator = typeof(CustomAdder).GetImplicitOperator(o => o.To<string>());
            var adderInstance = New(typeof(CustomAdder).GetPublicInstanceConstructor());
            var operatorCall = Call(stringOperator, adderInstance);
            var assignment = Assign(stringVariable, operatorCall);

            var translated = assignment.ToReadableString();

            translated.ShouldBe("str = new CustomAdder()");
        }

        [Fact]
        public void ShouldTranslateExplicitOperatorUse()
        {
            var adderToString = CreateLambda(() => (int)new CustomAdder());

            var intVariable = Variable(typeof(int), "i");
            var assignment = Assign(intVariable, adderToString.Body);

            var translated = assignment.ToReadableString();

            translated.ShouldBe("i = (int)new CustomAdder()");
        }

        [Fact]
        public void ShouldTranslateExplicitMethodOperatorUse()
        {
            var intVariable = Variable(typeof(int), "i");
            var intOperator = typeof(CustomAdder).GetExplicitOperator(o => o.To<int>());
            var adderInstance = New(typeof(CustomAdder).GetPublicInstanceConstructor());
            var operatorCall = Call(intOperator, adderInstance);
            var assignment = Assign(intVariable, operatorCall);

            var translated = assignment.ToReadableString();

            translated.ShouldBe("i = (int)new CustomAdder()");
        }
    }

    #region Helper Classes

    internal class PropertiesHelper
    {
        public static int PublicStatic { get; set; }

        public int PublicInstance { get; set; }

        internal static int NonPublicStatic { get; set; }

        internal int NonPublicInstance { get; set; }

        public virtual int NonPublicInstanceSetter { get; internal set; }
    }

    internal class FieldsHelper
    {
        public static int PublicStatic;

        public int PublicInstance;
    }

    internal class IndexedProperty
    {
        public static readonly object Default = new IndexedProperty(new object[0]);

        private readonly object[] _values;

        public IndexedProperty(object[] values)
        {
            _values = values;
        }

        public object this[int index] => _values[index];

        public bool TryGet(int index, out object value)
        {
            value = _values.ElementAtOrDefault(index);
            return value != null;
        }

        public bool TryGetGeneric<T>(int index, out T value)
            where T : class
        {
            value = _values.ElementAtOrDefault(index) as T;
            return value != null;
        }

        public void RefGet(int index, ref object value)
        {
            if (value == null)
            {
                value = _values.ElementAtOrDefault(index);
            }
        }

        public T GetFirst<T>() => (T)_values[0];

        public void SetFirst<T>(T item) => _values[0] = item;
    }

    internal class ValueConverter
    {
        public TResult Convert<TValue, TResult>(TValue value)
        {
            return (TResult)(object)value;
        }

        public bool TryConvert<TValue, TResult>(TValue value, out TResult result)
        {
            result = (value != null) ? Convert<TValue, TResult>(value) : default(TResult);
            return true;
        }
    }

    internal class CapturedInstanceHelper
    {
        private readonly int _i;

        public CapturedInstanceHelper(int i)
        {
            _i = i;
        }

        public string GetComparisonTranslation(int comparator)
        {
            var comparison = TestClassBase.CreateLambda(() => _i == comparator);

            return comparison.Body.ToReadableString();
        }
    }

    internal static class ParamsHelper
    {
        public static string OptionalParams(string value, params string[] stringsToAdd)
        {
            return value + string.Join(string.Empty, stringsToAdd);
        }
    }

    // ReSharper disable once UnusedTypeParameter
    internal static class GenericHelper<T>
    {
        public static void DoSomething()
        {
        }
    }

    internal class CustomAdder : IEnumerable
    {
        public static implicit operator string(CustomAdder adder) => adder.ToString();
        public static explicit operator int(CustomAdder adder) => adder.GetHashCode();

        public void Add(int intOne, int intTwo, int intThree)
        {
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    internal class ExtensionExpression : Expression
    {
        public ExtensionExpression(Type type)
        {
            Type = type;
        }

        public override string ToString() => "Extension_Expr";

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
    }

    #endregion
}