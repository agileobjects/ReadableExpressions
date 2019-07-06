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
    public class WhenTranslatingConstructorInfos
    {
        [Fact]
        public void ShouldTranslateANullCtorInfo()
        {
            var translated = MethodDefinitionTranslator.Translate(default(ConstructorInfo));

            translated.ShouldBe("[Constructor not found]");
        }

        [Fact]
        public void ShouldTranslateAParameterlessCtorInfo()
        {
            var ctor = typeof(Helper)
                .GetPublicInstanceConstructor();

            var translated = MethodDefinitionTranslator.Translate(ctor);

            translated.ShouldBe("WhenTranslatingConstructorInfos.Helper()");
        }

        [Fact]
        public void ShouldTranslateASingleParameterCtorInfo()
        {
            var ctor = typeof(Helper)
                .GetPublicInstanceConstructor(typeof(int));

            var translated = MethodDefinitionTranslator.Translate(ctor);

            const string EXPECTED =
@"WhenTranslatingConstructorInfos.Helper
(
    int value
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateATwoParameterCtorInfo()
        {
            var ctor = typeof(Helper)
                .GetPublicInstanceConstructor(typeof(DateTime), typeof(int));

            var translated = MethodDefinitionTranslator.Translate(ctor);

            const string EXPECTED =
@"WhenTranslatingConstructorInfos.Helper
(
    DateTime date,
    int days
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnOpenGenericCtorInfo()
        {
            var ctor = typeof(Helper<>)
                .GetPublicInstanceConstructor();

            var translated = MethodDefinitionTranslator.Translate(ctor);

            translated.ShouldBe("WhenTranslatingConstructorInfos.Helper<T>()");
        }

        [Fact]
        public void ShouldTranslateAClosedGenericCtorInfo()
        {
            var ctor = typeof(Helper<Dictionary<int, string>>)
                .GetPublicInstanceConstructor();

            var translated = MethodDefinitionTranslator.Translate(ctor);

            translated.ShouldBe("WhenTranslatingConstructorInfos.Helper<Dictionary<int, string>>()");
        }

        [Fact]
        public void ShouldTranslateAParameterlessTwoOpenGenericsCtorInfo()
        {
            var method = typeof(Helper<,>)
                .GetPublicInstanceConstructor();

            var translated = MethodDefinitionTranslator.Translate(method);

            translated.ShouldBe("WhenTranslatingConstructorInfos.Helper<T1, T2>()");
        }

        [Fact]
        public void ShouldTranslateAThreeParametersTwoGenericsCtorInfo()
        {
            var method = typeof(Helper<DateTime, TimeSpan>)
                .GetPublicInstanceConstructor(typeof(int), typeof(Func<int, DateTime>), typeof(TimeSpan));

            var translated = MethodDefinitionTranslator.Translate(method);

            const string EXPECTED =
@"WhenTranslatingConstructorInfos.Helper<DateTime, TimeSpan>
(
    int value,
    Func<int, DateTime> func,
    TimeSpan value2
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateAnOutParameter()
        {
            var ctor = typeof(Helper)
                .GetPublicInstanceConstructor(typeof(DateTime).MakeByRefType());

            var translated = MethodDefinitionTranslator.Translate(ctor);

            const string EXPECTED =
@"WhenTranslatingConstructorInfos.Helper
(
    out DateTime value
)";
            translated.ShouldBe(EXPECTED);
        }

        [Fact]
        public void ShouldTranslateARefParameter()
        {
            var ctor = typeof(Helper)
                .GetPublicInstanceConstructor(typeof(object).MakeByRefType());

            var translated = MethodDefinitionTranslator.Translate(ctor);

            const string EXPECTED =
@"WhenTranslatingConstructorInfos.Helper
(
    ref object value
)";
            translated.ShouldBe(EXPECTED);
        }

        #region Helper Classes

        // ReSharper disable UnusedMember.Local
        private class Helper
        {
            public Helper()
            {
                Console.WriteLine("Constructed!");
            }

            public Helper(out DateTime value)
            {
                value = DateTime.Now;
            }

            public Helper(ref object value)
            {
                Console.WriteLine(value);
            }

            public Helper(int value)
            {
                Console.WriteLine(value);
            }

            public Helper(DateTime date, int days)
            {
                Console.WriteLine(date);
                Console.WriteLine(days);
            }
        }

        private class Helper<T>
        {
            public Helper()
            {
                Console.WriteLine(typeof(T));
            }
        }

        private class Helper<T1, T2>
        {
            public Helper()
            {
                Console.WriteLine("Constructed!");
            }

            public Helper(int value, Func<int, T1> func, T2 value2)
            {
                Console.WriteLine(value);
                Console.WriteLine(func(value));
                Console.WriteLine(value2);
            }
        }
        // ReSharper restore UnusedMember.Local

        #endregion
    }
}
