namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
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

            var translated = ToReadableString(getArrayLength);

            translated.ShouldBe("a => a.Length");
        }

        [Fact]
        public void ShouldTranslateAnInstanceCallExpression()
        {
            var objectToString = CreateLambda((object o) => o.ToString());

            var translated = ToReadableString(objectToString);

            translated.ShouldBe("o => o.ToString()");
        }

        [Fact]
        public void ShouldTranslateAnInstanceCallExpressionStaticMemberArgument()
        {
            var intToFormattedString = CreateLambda((int i) => i.ToString(CultureInfo.CurrentCulture));

            var translated = ToReadableString(intToFormattedString);

            translated.ShouldBe("i => i.ToString(CultureInfo.CurrentCulture)");
        }

        [Fact]
        public void ShouldTranslateAnInstanceCallExpressionParameterArgument()
        {
            var intToFormattedString = CreateLambda((int i, CultureInfo ci) => i.ToString(ci));

            var translated = ToReadableString(intToFormattedString);

            translated.ShouldBe("(i, ci) => i.ToString(ci)");
        }

        [Fact]
        public void ShouldTranslateAParameterlessExtensionMethodCall()
        {
            var arrayIsEmpty = CreateLambda((string[] a) => a.Any());

            var translated = ToReadableString(arrayIsEmpty);

            translated.ShouldBe("a => a.Any()");
        }

        [Fact]
        public void ShouldTranslateAnExtensionMethodCallWithSimpleParameters()
        {
            var notTheFirstTwo = CreateLambda((string[] a) => a.Skip(2));

            var translated = ToReadableString(notTheFirstTwo);

            translated.ShouldBe("a => a.Skip(2)");
        }

        [Fact]
        public void ShouldTranslateAnExtensionMethodCallWithALambdaParameter()
        {
            var noBlankStrings = CreateLambda((string[] a) => a.All(i => i.Length != 0));

            var translated = ToReadableString(noBlankStrings);

            translated.ShouldBe("a => a.All(i => i.Length != 0)");
        }

        [Fact]
        public void ShouldTranslateAStaticCallExpression()
        {
            // ReSharper disable once ReferenceEqualsWithValueType
            var oneEqualsTwo = CreateLambda(() => ReferenceEquals(1, 2));

            var translated = ToReadableString(oneEqualsTwo);

            translated.ShouldBe("() => object.ReferenceEquals(1, 2)");
        }

        [Fact]
        public void ShouldTranslateAStaticCallExpressionOnAGenericType()
        {
            var doSomething = CreateLambda(() => GenericHelper<Dictionary<DateTime, string>>.DoSomething());

            var translated = ToReadableString(doSomething.Body);

            translated.ShouldBe("GenericHelper<Dictionary<DateTime, string>>.DoSomething()");
        }

        [Fact]
        public void ShouldTranslateAnInstanceMemberExpression()
        {
            var getDateDay = CreateLambda((DateTime d) => d.Day);

            var translated = ToReadableString(getDateDay);

            translated.ShouldBe("d => d.Day");
        }

        [Fact]
        public void ShouldTranslateAStaticMemberExpression()
        {
            var getToday = CreateLambda(() => DateTime.Today);

            var translated = ToReadableString(getToday);

            translated.ShouldBe("() => DateTime.Today");
        }

        [Fact]
        public void ShouldTranslateAStaticMemberExpressionUsingTheDeclaringType()
        {
            var getDefaultIndexedProperty = CreateLambda(() => IndexedProperty.Default);

            var translated = ToReadableString(getDefaultIndexedProperty.Body);

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

            var translated = ToReadableString(getterAccess);

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

            var translated = ToReadableString(setterCall);

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

            var translated = ToReadableString(getterAccess);

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

            var translated = ToReadableString(setterCall);

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

            var translated = ToReadableString(getterAccess);

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

            var translated = ToReadableString(setterCall);

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

            var translated = ToReadableString(getterAccess);

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

            var translated = ToReadableString(setterCall);

            translated.ShouldBe("PropertiesHelper.NonPublicStatic = 456");
        }

        [Fact]
        public void ShouldTranslateAParamsArrayArgument()
        {
            var splitString = CreateLambda((string str) => str.Split('x', 'y', 'z'));

            var translated = ToReadableString(splitString);

            translated.ShouldBe("str => str.Split('x', 'y', 'z')");
        }

        [Fact]
        public void ShouldTranslateAnEmptyParamsArrayArgument()
        {
            var combineStrings = CreateLambda((string str) => ParamsHelper.OptionalParams(str));

            var translated = ToReadableString(combineStrings);

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

            var translated = ToReadableString(getPropertyIndex.Body);

            translated.ShouldBe("p[index]");
        }

        [Fact]
        public void ShouldTranslateAManualIndexedPropertyAccessExpression()
        {
            var indexedProperty = Variable(typeof(IndexedProperty), "p");
            var property = indexedProperty.Type.GetProperties().First();
            var firstElement = Constant(1, typeof(int));

            var indexerAccess = MakeIndex(indexedProperty, property, new[] { firstElement });

            var translated = ToReadableString(indexerAccess);

            translated.ShouldBe("p[1]");
        }

        [Fact]
        public void ShouldTranslateAStringIndexAccessExpression()
        {
            var getFirstCharacter = CreateLambda((string str) => str[0]);

            var translated = ToReadableString(getFirstCharacter.Body);

            translated.ShouldBe("str[0]");
        }

        [Fact]
        public void ShouldTranslateAnArrayIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((int[] items) => items[0]);

            var translated = ToReadableString(getFirstItem.Body);

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAnIDictionaryIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((IDictionary<int, string> items) => items[0]);

            var translated = ToReadableString(getFirstItem.Body);

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateADictionaryIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((Dictionary<long, string> items) => items[0]);

            var translated = ToReadableString(getFirstItem.Body);

            translated.ShouldBe("items[0L]");
        }

        [Fact]
        public void ShouldTranslateACollectionIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((Collection<string> items) => items[0]);

            var translated = ToReadableString(getFirstItem.Body);

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAnIListIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((IList<string> items) => items[0]);

            var translated = ToReadableString(getFirstItem.Body);

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAListIndexAccessExpression()
        {
            var getFirstItem = CreateLambda((List<string> items) => items[0]);

            var translated = ToReadableString(getFirstItem.Body);

            translated.ShouldBe("items[0]");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithGenericArgumentIncluded()
        {
            var getFirstItem = CreateLambda((IndexedProperty ip) => ip.GetFirst<string>());

            var translated = ToReadableString(getFirstItem.Body);

            translated.ShouldBe("ip.GetFirst<string>()");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithPartlyImpliedTypeParameters()
        {
            var convertIntToString = CreateLambda((int i) => new ValueConverter().Convert<int, string>(i));

            var translated = ToReadableString(convertIntToString.Body);

            translated.ShouldBe("new ValueConverter().Convert<int, string>(i)");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithoutGenericArgumentIncluded()
        {
            var setFirstItem = CreateLambda((IndexedProperty ip, string str) => ip.SetFirst(str));

            var translated = ToReadableString(setFirstItem.Body);

            translated.ShouldBe("ip.SetFirst(str)");
        }

        [Fact]
        public void ShouldTranslateAMethodCallWithRequestedGenericArgumentsIncluded()
        {
            var setFirstItem = CreateLambda((IndexedProperty ip, string str) => ip.SetFirst(str));

            var translated = ToReadableString(setFirstItem.Body, c => c.UseExplicitGenericParameters);

            translated.ShouldBe("ip.SetFirst<string>(str)");
        }

        [Fact]
        public void ShouldTranslateAnOutParameterMethodCallWithoutGenericArgumentIncluded()
        {
            string result;

            var convertIntToString = CreateLambda((int i) => new ValueConverter().TryConvert(i, out result));

            var translated = ToReadableString(convertIntToString.Body);

            translated.ShouldBe("new ValueConverter().TryConvert(i, out result)");
        }

        [Fact]
        public void ShouldTranslateANegatedMethodCall()
        {
            var listDoesNotContainZero = CreateLambda((List<int> l)
                => !l.Contains(0, EqualityComparer<int>.Default));

            var translated = ToReadableString(listDoesNotContainZero.Body);

            translated.ShouldBe("!l.Contains(0, EqualityComparer<int>.Default)");
        }

        [Fact]
        public void ShouldNotIncludeCapturedInstanceNames()
        {
            var helper = new CapturedInstanceHelper(5);
            var translated = helper.GetComparisonTranslation(3);

            translated.ShouldBe("_i == comparator");
        }

        [Fact]
        public void ShouldIncludeOutParameterKeywords()
        {
            var helperVariable = Variable(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperVariable, tryGetMethod, one, valueVariable);

            var translated = ToReadableString(tryGetCall);

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

            var translated = ToReadableString(tryGetLambda);

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
        public void ShouldIncludeAnInlineOutParameterVariable()
        {
            var helperParameter = Parameter(typeof(IndexedProperty), "ip");
            var one = Constant(1);
            var valueVariable = Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Call(helperParameter, tryGetMethod, one, valueVariable);
            var tryGetBlock = Block(new[] { valueVariable }, tryGetCall, valueVariable);
            var tryGetLambda = Lambda<Func<IndexedProperty, object>>(tryGetBlock, helperParameter);

            var translated = ToReadableString(tryGetLambda, s => s.DeclareOutputParametersInline);

            const string EXPECTED = @"
ip =>
{
    ip.TryGet(1, out var value);

    return value;
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

            var translated = ToReadableString(tryGetLambda, s => s.DeclareOutputParametersInline);

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

            var translated = ToReadableString(tryGetLambda, s => s.DeclareOutputParametersInline);

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

            var translated = ToReadableString(tryGetCall);

            translated.ShouldBe("ip.RefGet(3, ref value)");
        }

        [Fact]
        public void ShouldTranslateACustomEnumerableAddInitialiser()
        {
            var customAdder = CreateLambda((int intOne, int intTwo, int intThree)
                => new CustomAdder { { intOne, intTwo, intThree } });

            var translated = ToReadableString(customAdder.Body);

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

            var translated = ToReadableString(assignment);

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

            var translated = ToReadableString(assignment);

            translated.ShouldBe("str = new CustomAdder()");
        }

        [Fact]
        public void ShouldTranslateExplicitOperatorUse()
        {
            var adderToString = CreateLambda(() => (int)new CustomAdder());

            var intVariable = Variable(typeof(int), "i");
            var assignment = Assign(intVariable, adderToString.Body);

            var translated = ToReadableString(assignment);

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

            var translated = ToReadableString(assignment);

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

            return TestClassBase.ToReadableString(comparison.Body);
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

    #endregion
}