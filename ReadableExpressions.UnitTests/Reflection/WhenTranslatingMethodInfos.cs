namespace AgileObjects.ReadableExpressions.UnitTests.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Common;
    using NetStandardPolyfills;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingMethodInfos
    {
        [Fact]
        public void ShouldTranslateANullMethodInfo()
        {
            var translated = default(MethodInfo).ToReadableString();

            translated.ShouldBe("[Method not found]");
        }

        [Fact]
        public void ShouldTranslateAPublicInstancePropertyGetter()
        {
            var getter = typeof(Helper)
                .GetPublicInstanceProperty(nameof(Helper.PublicInstanceProperty))
                .GetGetter();

            var translated = getter.ToReadableString();

            const string EXPECTED =
                "public int WhenTranslatingMethodInfos.Helper.PublicInstanceProperty { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAPublicInstancePropertySetter()
        {
            var setter = typeof(Helper)
                .GetPublicInstanceProperty(nameof(Helper.PublicInstanceProperty))
                .GetSetter();

            var translated = setter.ToReadableString();

            const string EXPECTED =
                "public int WhenTranslatingMethodInfos.Helper.PublicInstanceProperty { set; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnInternalInstancePropertyGetter()
        {
            var getter = typeof(Helper)
                .GetNonPublicInstanceProperty(nameof(Helper.InternalInstanceProperty))
                .GetGetter(nonPublic: true);

            var translated = getter.ToReadableString();

            const string EXPECTED =
                "internal string WhenTranslatingMethodInfos.Helper.InternalInstanceProperty { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAProtectedInstancePropertyGetter()
        {
            var getter = typeof(Helper)
                .GetNonPublicInstanceProperty("ProtectedInstanceProperty")
                .GetGetter(nonPublic: true);

            var translated = getter.ToReadableString();

            const string EXPECTED =
                "protected string WhenTranslatingMethodInfos.Helper.ProtectedInstanceProperty { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAProtectedInternalInstancePropertyGetter()
        {
            var getter = typeof(Helper)
                .GetNonPublicInstanceProperty(nameof(Helper.ProtectedInternalInstanceProperty))
                .GetGetter(nonPublic: true);

            var translated = getter.ToReadableString();

            const string EXPECTED =
                "protected internal string WhenTranslatingMethodInfos.Helper.ProtectedInternalInstanceProperty { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAPrivateInstancePropertyGetter()
        {
            var getter = typeof(Helper)
                .GetNonPublicInstanceProperty("PrivateInstanceProperty")
                .GetGetter(nonPublic: true);

            var translated = getter.ToReadableString();

            const string EXPECTED =
                "private string WhenTranslatingMethodInfos.Helper.PrivateInstanceProperty { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAParameterlessMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterless));

            var translated = method.ToReadableString();

            const string EXPECTED =
"public string WhenTranslatingMethodInfos.Helper.InstanceParameterless()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAVirtualParameterlessMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceVirtualParameterless));

            var translated = method.ToReadableString();

            const string EXPECTED =
                "public virtual string WhenTranslatingMethodInfos.Helper.InstanceVirtualParameterless()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnAbstractInstancePropertyGetter()
        {
            var getter = typeof(AbstractHelper)
                .GetPublicInstanceProperty(nameof(AbstractHelper.PublicInstanceProperty))
                .GetGetter();

            var translated = getter.ToReadableString();

            const string EXPECTED =
                "public abstract int WhenTranslatingMethodInfos.AbstractHelper.PublicInstanceProperty { get; }";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnAbstractParameterlessMethodInfo()
        {
            var method = typeof(AbstractHelper)
                .GetPublicInstanceMethod(nameof(AbstractHelper.InstanceAbstractParameterless));

            var translated = method.ToReadableString();

            const string EXPECTED =
                "public abstract string WhenTranslatingMethodInfos.AbstractHelper.InstanceAbstractParameterless()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnAbstractClassParameterlessMethodInfo()
        {
            var method = typeof(AbstractHelper)
                .GetPublicInstanceMethod(nameof(AbstractHelper.InstanceParameterless));

            var translated = method.ToReadableString();

            const string EXPECTED =
                "public string WhenTranslatingMethodInfos.AbstractHelper.InstanceParameterless()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateASingleParameterMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceOneParameter));

            var translated = method.ToReadableString();

            const string EXPECTED =
@"public int WhenTranslatingMethodInfos.Helper.InstanceOneParameter
(
    int value
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateATwoParameterMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceTwoParameters));

            var translated = method.ToReadableString();

            const string EXPECTED =
@"public DateTime WhenTranslatingMethodInfos.Helper.InstanceTwoParameters
(
    DateTime date,
    int days
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnOpenGenericMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterlessSingleGeneric));

            var translated = method.ToReadableString();

            const string EXPECTED =
"public Type WhenTranslatingMethodInfos.Helper.InstanceParameterlessSingleGeneric<T>()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAClosedGenericMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterlessSingleGeneric))
                .MakeGenericMethod(typeof(Dictionary<,>));

            var translated = method.ToReadableString();

            const string EXPECTED =
"public Type WhenTranslatingMethodInfos.Helper.InstanceParameterlessSingleGeneric<Dictionary<TKey, TValue>>()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnOpenNewableClassConstraintGenericMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterlessNewableClassConstraintGeneric));

            var translated = method.ToReadableString();

            const string EXPECTED = @"
public T WhenTranslatingMethodInfos.Helper.InstanceParameterlessNewableClassConstraintGeneric<T>()
    where T : class, new()";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnOpenStructConstraintGenericMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterlessStructConstraintGeneric));

            var translated = method.ToReadableString();

            const string EXPECTED = @"
public T WhenTranslatingMethodInfos.Helper.InstanceParameterlessStructConstraintGeneric<T>()
    where T : struct";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnOpenTypeConstraintGenericMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterlessTypeConstraintGeneric));

            var translated = method.ToReadableString();

            const string EXPECTED = @"
public TItem WhenTranslatingMethodInfos.Helper.InstanceParameterlessTypeConstraintGeneric<TItem>()
    where TItem : IList<TItem>";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAThreeParametersTwoOpenGenericsMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceThreeParametersTwoGenerics));

            var translated = method.ToReadableString();

            const string EXPECTED =
@"public void WhenTranslatingMethodInfos.Helper.InstanceThreeParametersTwoGenerics<T1, T2>
(
    int value,
    Func<int, T1> func,
    T2 value2
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnOutParameter()
        {
            var method = typeof(Helper)
                .GetPublicStaticMethod(nameof(Helper.StaticOutParameter));

            var translated = method.ToReadableString();

            const string EXPECTED = @"
public static string WhenTranslatingMethodInfos.Helper.StaticOutParameter
(
    out int value
)";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateARefParameter()
        {
            var method = typeof(Helper)
                .GetPublicStaticMethod(nameof(Helper.StaticRefParameter))
                .MakeGenericMethod(typeof(List<List<int>>));

            var translated = method.ToReadableString();

            const string EXPECTED = @"
public static void WhenTranslatingMethodInfos.Helper.StaticRefParameter<List<List<int>>>
(
    ref List<List<int>> value
)";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAParamsParameter()
        {
            var method = typeof(ParamsHelper)
                .GetPublicStaticMethod(nameof(ParamsHelper.OptionalParams));

            var translated = method.ToReadableString();

            const string EXPECTED = @"
public static string ParamsHelper.OptionalParams
(
    string value,
    params string[] stringsToAdd
)";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/61
        [Fact]
        public void ShouldTranslateAnOpenGenericQueryableExtensionMethod()
        {
            var method = typeof(Queryable)
                .GetPublicStaticMethods("SelectMany")
                .First(m =>
                    m.GetParameters().Length == 3 &&
                    m.GetParameters()[1]
                        .ParameterType
                        .GetGenericTypeArguments()[0]
                        .GetGenericTypeArguments().Length == 3);

            var translated = method.ToReadableString();

            const string EXPECTED = @"
public static IQueryable<TResult> Queryable.SelectMany<TSource, TCollection, TResult>
(
    this IQueryable<TSource> source,
    Expression<Func<TSource, int, IEnumerable<TCollection>>> collectionSelector,
    Expression<Func<TSource, TCollection, TResult>> resultSelector
)";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnImplicitOperator()
        {
            var toStringImplicitOperator = typeof(CustomAdder).GetImplicitOperator(o => o.To<string>());

            var translated = toStringImplicitOperator.ToReadableString();

            const string EXPECTED = @"
public static implicit operator string
(
    WhenTranslatingMethodInfos.CustomAdder adder
)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAnExplicitOperator()
        {
            var toIntExplicitOperator = typeof(CustomAdder).GetExplicitOperator(o => o.To<int>());

            var translated = toIntExplicitOperator.ToReadableString();

            const string EXPECTED = @"
public static explicit operator int
(
    WhenTranslatingMethodInfos.CustomAdder adder
)";

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        [Fact]
        public void ShouldTranslateAStaticProperty()
        {
            var publicInstanceProperty = typeof(PropertiesHelper)
                .GetPublicStaticProperty(nameof(PropertiesHelper.PublicStatic));

            publicInstanceProperty.ShouldNotBeNull();

            var translated = publicInstanceProperty.ToReadableString();

            translated.ShouldBe("public static int PropertiesHelper.PublicStatic { get; set; }");
        }

        [Fact]
        public void ShouldTranslateAnInstanceProperty()
        {
            var nonPublicInstanceProperty = typeof(PropertiesHelper)
                .GetPublicInstanceProperty(nameof(PropertiesHelper.NonPublicInstanceSetter));

            nonPublicInstanceProperty.ShouldNotBeNull();

            var translated = nonPublicInstanceProperty.ToReadableString();

            translated.ShouldBe(
                "public virtual int PropertiesHelper.NonPublicInstanceSetter { get; internal set; }");
        }

        [Fact]
        public void ShouldTranslateAnInstancePropertyGetter()
        {
            var publicInstanceGetter = typeof(PropertiesHelper)
                .GetPublicInstanceProperty(nameof(PropertiesHelper.PublicInstance))
                .GetGetter();

            publicInstanceGetter.ShouldNotBeNull();

            var translated = publicInstanceGetter.ToReadableString();

            translated.ShouldBe("public int PropertiesHelper.PublicInstance { get; }");
        }

        [Fact]
        public void ShouldTranslateAStaticPropertySetter()
        {
            var publicStaticSetter = typeof(PropertiesHelper)
                .GetNonPublicStaticProperty(nameof(PropertiesHelper.NonPublicStatic))
                .GetSetter(nonPublic: true);

            publicStaticSetter.ShouldNotBeNull();

            var translated = publicStaticSetter.ToReadableString();

            translated.ShouldBe("internal static int PropertiesHelper.NonPublicStatic { set; }");
        }

        [Fact]
        public void ShouldTranslateAnNonPublicInstancePropertySetter()
        {
            var nonPublicInstanceSetter = typeof(PropertiesHelper)
                .GetPublicInstanceProperty(nameof(PropertiesHelper.NonPublicInstanceSetter))
                .GetSetter(nonPublic: true);

            nonPublicInstanceSetter.ShouldNotBeNull();

            var translated = nonPublicInstanceSetter.ToReadableString();

            translated.ShouldBe(
                "public virtual int PropertiesHelper.NonPublicInstanceSetter { internal set; }");
        }

        #region Helper Classes

        // ReSharper disable once ClassWithVirtualMembersNeverInherited.Local
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class Helper
        {
            public Helper()
            {
                ProtectedInstanceProperty = PrivateInstanceProperty = "hello";
            }

            public int PublicInstanceProperty { get; set; }

            internal string InternalInstanceProperty { get; set; }

            protected internal string ProtectedInternalInstanceProperty { get; set; }

            protected string ProtectedInstanceProperty { get; set; }

            private string PrivateInstanceProperty { get; set; }

            public string InstanceParameterless() => null;

            public virtual string InstanceVirtualParameterless() => null;

            public int InstanceOneParameter(int value) => value * 2;

            public DateTime InstanceTwoParameters(DateTime date, int days) => date.AddDays(days);

            public void InstanceThreeParametersTwoGenerics<T1, T2>(int value, Func<int, T1> func, T2 value2)
            {
                Console.WriteLine(func(value));
                Console.WriteLine(value2);
            }

            public Type InstanceParameterlessSingleGeneric<T>() => typeof(T);

            public T InstanceParameterlessNewableClassConstraintGeneric<T>()
                where T : class, new()
            {
                return new T();
            }

            public T InstanceParameterlessStructConstraintGeneric<T>()
                where T : struct
            {
                return new T();
            }

            public TItem InstanceParameterlessTypeConstraintGeneric<TItem>()
                where TItem : IList<TItem>
            {
                return default(TItem);
            }

            public static string StaticOutParameter(out int value)
            {
                value = 1;
                return value.ToString();
            }

            public static void StaticRefParameter<T>(ref T value)
                where T : class
            {
                Console.WriteLine(value);
                value = default(T);
            }
        }
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        private abstract class AbstractHelper
        {
            public abstract int PublicInstanceProperty { get; }

            public string InstanceParameterless() => null;

            public abstract string InstanceAbstractParameterless();
        }

        internal class CustomAdder
        {
            public static implicit operator string(CustomAdder adder) => adder.ToString();
            public static explicit operator int(CustomAdder adder) => adder.GetHashCode();
        }

        #endregion
    }
}
