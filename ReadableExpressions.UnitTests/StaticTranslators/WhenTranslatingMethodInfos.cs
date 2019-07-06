namespace AgileObjects.ReadableExpressions.UnitTests.StaticTranslators
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NetStandardPolyfills;
    using Translations.StaticTranslators;
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
            var translated = MethodDefinitionTranslator.Translate(default(MethodInfo));

            translated.ShouldBe("[Method not found]");
        }

        [Fact]
        public void ShouldTranslateAParameterlessMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterless));

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
"string WhenTranslatingMethodInfos.Helper.InstanceParameterless()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateASingleParameterMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceOneParameter));

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
@"int WhenTranslatingMethodInfos.Helper.InstanceOneParameter
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

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
@"DateTime WhenTranslatingMethodInfos.Helper.InstanceTwoParameters
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

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
"Type WhenTranslatingMethodInfos.Helper.InstanceParameterlessSingleGeneric<T>()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAClosedGenericMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceParameterlessSingleGeneric))
                .MakeGenericMethod(typeof(Dictionary<,>));

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
"Type WhenTranslatingMethodInfos.Helper.InstanceParameterlessSingleGeneric<Dictionary<TKey, TValue>>()";

            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAThreeParametersTwoOpenGenericsMethodInfo()
        {
            var method = typeof(Helper)
                .GetPublicInstanceMethod(nameof(Helper.InstanceThreeParametersTwoGenerics));

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
@"void WhenTranslatingMethodInfos.Helper.InstanceThreeParametersTwoGenerics<T1, T2>
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

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
                @"string WhenTranslatingMethodInfos.Helper.StaticOutParameter
(
    out int value
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateARefParameter()
        {
            var method = typeof(Helper)
                .GetPublicStaticMethod(nameof(Helper.StaticRefParameter))
                .MakeGenericMethod(typeof(List<List<int>>));

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
                @"void WhenTranslatingMethodInfos.Helper.StaticRefParameter<List<List<int>>>
(
    ref List<List<int>> value
)";
            translated.ShouldBe(EXPECTED);
        }

        private class Helper
        {
            public string InstanceParameterless() => null;

            public int InstanceOneParameter(int value) => value * 2;

            public DateTime InstanceTwoParameters(DateTime date, int days) => date.AddDays(days);

            public void InstanceThreeParametersTwoGenerics<T1, T2>(int value, Func<int, T1> func, T2 value2)
            {
                Console.WriteLine(func(value));
                Console.WriteLine(value2);
            }

            public Type InstanceParameterlessSingleGeneric<T>() => typeof(T);

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
    }
}
