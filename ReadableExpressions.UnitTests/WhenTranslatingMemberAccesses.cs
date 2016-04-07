namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingMemberAccesses
    {
        [TestMethod]
        public void ShouldTranslateAnArrayLengthExpression()
        {
            Expression<Func<string[], int>> getArrayLength = a => a.Length;

            var translated = getArrayLength.ToReadableString();

            Assert.AreEqual("a => a.Length", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceCallExpression()
        {
            Expression<Func<object, string>> objectToString = o => o.ToString();

            var translated = objectToString.ToReadableString();

            Assert.AreEqual("o => o.ToString()", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceCallExpressionStaticMemberArgument()
        {
            Expression<Func<int, string>> intToFormattedString = i => i.ToString(CultureInfo.CurrentCulture);

            var translated = intToFormattedString.ToReadableString();

            Assert.AreEqual("i => i.ToString(CultureInfo.CurrentCulture)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceCallExpressionParameterArgument()
        {
            Expression<Func<int, CultureInfo, string>> intToFormattedString = (i, ci) => i.ToString(ci);

            var translated = intToFormattedString.ToReadableString();

            Assert.AreEqual("(i, ci) => i.ToString(ci)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAParameterlessExtensionMethodCall()
        {
            Expression<Func<string[], bool>> arrayIsEmpty = a => a.Any();

            var translated = arrayIsEmpty.ToReadableString();

            Assert.AreEqual("a => a.Any()", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnExtensionMethodCallWithSimpleParameters()
        {
            Expression<Func<string[], IEnumerable<string>>> notTheFirstTwo = a => a.Skip(2);

            var translated = notTheFirstTwo.ToReadableString();

            Assert.AreEqual("a => a.Skip(2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnExtensionMethodCallWithALambdaParameter()
        {
            Expression<Func<string[], bool>> noBlankStrings = a => a.All(i => i.Length != 0);

            var translated = noBlankStrings.ToReadableString();

            Assert.AreEqual("a => a.All(i => i.Length != 0)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAStaticCallExpression()
        {
            // ReSharper disable once ReferenceEqualsWithValueType
            Expression<Func<bool>> oneEqualsTwo = () => ReferenceEquals(1, 2);

            var translated = oneEqualsTwo.ToReadableString();

            Assert.AreEqual("() => Object.ReferenceEquals(1, 2)", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnInstanceMemberExpression()
        {
            Expression<Func<DateTime, int>> getDateDay = d => d.Day;

            var translated = getDateDay.ToReadableString();

            Assert.AreEqual("d => d.Day", translated);
        }

        [TestMethod]
        public void ShouldTranslateAStaticMemberExpression()
        {
            Expression<Func<DateTime>> getToday = () => DateTime.Today;

            var translated = getToday.ToReadableString();

            Assert.AreEqual("() => DateTime.Today", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIndexedPropertyAccessExpression()
        {
            Expression<Func<IndexedProperty, int, object>> getPropertyIndex = (p, index) => p[index];

            var translated = getPropertyIndex.Body.ToReadableString();

            Assert.AreEqual("p[index]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAManualIndexedPropertyAccessExpression()
        {
            var indexedProperty = Expression.Variable(typeof(IndexedProperty), "p");
            var property = indexedProperty.Type.GetProperties().First();
            var firstElement = Expression.Constant(1, typeof(int));

            var indexerAccess = Expression.MakeIndex(indexedProperty, property, new[] { firstElement });

            var translated = indexerAccess.ToReadableString();

            Assert.AreEqual("p[1]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAStringIndexAccessExpression()
        {
            Expression<Func<string, char>> getFirstCharacter = str => str[0];

            var translated = getFirstCharacter.Body.ToReadableString();

            Assert.AreEqual("str[0]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnArrayIndexAccessExpression()
        {
            Expression<Func<int[], int>> getFirstItem = items => items[0];

            var translated = getFirstItem.Body.ToReadableString();

            Assert.AreEqual("items[0]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIDictionaryIndexAccessExpression()
        {
            Expression<Func<IDictionary<int, string>, string>> getFirstItem = items => items[0];

            var translated = getFirstItem.Body.ToReadableString();

            Assert.AreEqual("items[0]", translated);
        }

        [TestMethod]
        public void ShouldTranslateADictionaryIndexAccessExpression()
        {
            Expression<Func<Dictionary<long, string>, string>> getFirstItem = items => items[0];

            var translated = getFirstItem.Body.ToReadableString();

            Assert.AreEqual("items[0L]", translated);
        }

        [TestMethod]
        public void ShouldTranslateACollectionIndexAccessExpression()
        {
            Expression<Func<Collection<string>, string>> getFirstItem = items => items[0];

            var translated = getFirstItem.Body.ToReadableString();

            Assert.AreEqual("items[0]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAnIListIndexAccessExpression()
        {
            Expression<Func<IList<string>, string>> getFirstItem = items => items[0];

            var translated = getFirstItem.Body.ToReadableString();

            Assert.AreEqual("items[0]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAListIndexAccessExpression()
        {
            Expression<Func<List<string>, string>> getFirstItem = items => items[0];

            var translated = getFirstItem.Body.ToReadableString();

            Assert.AreEqual("items[0]", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMethodCallWithGenericArgumentIncluded()
        {
            Expression<Func<IndexedProperty, string>> getFirstItem = ip => ip.GetFirst<string>();

            var translated = getFirstItem.Body.ToReadableString();

            Assert.AreEqual("ip.GetFirst<string>()", translated);
        }

        [TestMethod]
        public void ShouldTranslateAMethodCallWithoutGenericArgumentIncluded()
        {
            Expression<Action<IndexedProperty, string>> setFirstItem = (ip, str) => ip.SetFirst(str);

            var translated = setFirstItem.Body.ToReadableString();

            Assert.AreEqual("ip.SetFirst(str)", translated);
        }

        [TestMethod]
        public void ShouldNotIncludeCapturedInstanceNames()
        {
            var helper = new CapturedInstanceHelper(5);
            var translated = helper.GetComparisonTranslation(3);

            Assert.AreEqual("(_i == comparator)", translated);
        }

        [TestMethod]
        public void ShouldIncludeOutParameterKeywords()
        {
            var helperVariable = Expression.Variable(typeof(IndexedProperty), "ip");
            var one = Expression.Constant(1);
            var valueVariable = Expression.Variable(typeof(object), "value");
            var tryGetMethod = typeof(IndexedProperty).GetMethod("TryGet");
            var tryGetCall = Expression.Call(helperVariable, tryGetMethod, one, valueVariable);

            var translated = tryGetCall.ToReadableString();

            Assert.AreEqual("ip.TryGet(1, out value)", translated);
        }

        #region Helper Classes

        // ReSharper disable once ClassNeverInstantiated.Local
        private class IndexedProperty
        {
            private readonly object[] _values;

            public IndexedProperty(object[] values)
            {
                _values = values;
            }

            public object this[int index] => _values[index];

            // ReSharper disable once UnusedMember.Local
            public bool TryGet(int index, out object value)
            {
                value = _values.ElementAtOrDefault(index);
                return value != null;
            }

            public T GetFirst<T>() => (T)_values[0];

            public void SetFirst<T>(T item) => _values[0] = item;
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
                Expression<Func<bool>> comparison = () => _i == comparator;

                return comparison.Body.ToReadableString();
            }
        }

        #endregion
    }
}