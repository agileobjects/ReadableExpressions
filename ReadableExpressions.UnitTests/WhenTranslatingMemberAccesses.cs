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
#else
    using Expression = Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

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
        public void ShouldTranslateAnIndexedPropertyAccessExpression()
        {
            var getPropertyIndex = CreateLambda((IndexedProperty p, int index) => p[index]);

            var translated = ToReadableString(getPropertyIndex.Body);

            translated.ShouldBe("p[index]");
        }

        [Fact]
        public void ShouldTranslateAManualIndexedPropertyAccessExpression()
        {
            var indexedProperty = Expression.Variable(typeof(IndexedProperty), "p");
            var property = indexedProperty.Type.GetProperties().First();
            var firstElement = Expression.Constant(1, typeof(int));

            var indexerAccess = Expression.MakeIndex(indexedProperty, property, new[] { firstElement });

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

            translated.ShouldBe("(_i == comparator)");
        }

        [Fact]
        public void ShouldIncludeOutParameterKeywords()
        {
            var helperVariable = Expression.Variable(typeof(IndexedProperty), "ip");
            var one = Expression.Constant(1);
            var valueVariable = Expression.Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("TryGet");
            var tryGetCall = Expression.Call(helperVariable, tryGetMethod, one, valueVariable);

            var translated = ToReadableString(tryGetCall);

            translated.ShouldBe("ip.TryGet(1, out value)");
        }

        [Fact]
        public void ShouldIncludeRefParameterKeywords()
        {
            var helperVariable = Expression.Variable(typeof(IndexedProperty), "ip");
            var three = Expression.Constant(3);
            var valueVariable = Expression.Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetPublicInstanceMethod("RefGet");
            var tryGetCall = Expression.Call(helperVariable, tryGetMethod, three, valueVariable);

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
            var adderToString = CreateLambda(() => new CustomAdder());

            var stringVariable = Expression.Variable(typeof(string), "str");
            var assignment = Expression.Assign(stringVariable, adderToString.Body);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("str = new CustomAdder()");
        }

        [Fact]
        public void ShouldTranslateImplicitMethodOperatorUse()
        {
            var stringVariable = Expression.Variable(typeof(string), "str");
            var stringOperator = typeof(CustomAdder).GetImplicitOperator(o => o.To<string>());
            var adderInstance = Expression.New(typeof(CustomAdder).GetPublicInstanceConstructor());
            var operatorCall = Expression.Call(stringOperator, adderInstance);
            var assignment = Expression.Assign(stringVariable, operatorCall);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("str = new CustomAdder()");
        }

        [Fact]
        public void ShouldTranslateExplicitOperatorUse()
        {
            var adderToString = CreateLambda(() => (int)new CustomAdder());

            var intVariable = Expression.Variable(typeof(int), "i");
            var assignment = Expression.Assign(intVariable, adderToString.Body);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("i = (int)new CustomAdder()");
        }

        [Fact]
        public void ShouldTranslateExplicitMethodOperatorUse()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intOperator = typeof(CustomAdder).GetExplicitOperator(o => o.To<int>());
            var adderInstance = Expression.New(typeof(CustomAdder).GetPublicInstanceConstructor());
            var operatorCall = Expression.Call(intOperator, adderInstance);
            var assignment = Expression.Assign(intVariable, operatorCall);

            var translated = ToReadableString(assignment);

            translated.ShouldBe("i = (int)new CustomAdder()");
        }
    }

    #region Helper Classes

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