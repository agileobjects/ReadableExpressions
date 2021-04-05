#if FEATURE_DYNAMIC
namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Globalization;
    using Common;
    using Microsoft.CSharp.RuntimeBinder;
    using Xunit;
    using static System.Linq.Expressions.Expression;

    public class WhenTranslatingDynamicOperations : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAPropertyReadAccess()
        {
            var lengthGetterSiteBinder = Binder.GetMember(
                CSharpBinderFlags.None,
                "Length",
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Parameter(typeof(object), "obj");

            var dynamicLengthGetter = Dynamic(
                lengthGetterSiteBinder,
                typeof(object),
                dynamicParameter);

            var dynamicLengthLambda = Lambda<Func<object, object>>(dynamicLengthGetter, dynamicParameter);

            dynamicLengthLambda.Compile();

            var translated = dynamicLengthLambda.ToReadableString();

            translated.ShouldBe("obj => obj.Length");
        }

        [Fact]
        public void ShouldTranslateAPropertyWriteAccess()
        {
            var positionSetterSiteBinder = Binder.SetMember(
                CSharpBinderFlags.ResultDiscarded,
                "Position",
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Parameter(typeof(object), "obj");
            var positionParameter = Parameter(typeof(long), "position");

            var dynamicPositionSetter = Dynamic(
                positionSetterSiteBinder,
                typeof(object),
                dynamicParameter,
                positionParameter);

            var dynamicPositionLambda = Lambda<Action<object, long>>(
                dynamicPositionSetter,
                dynamicParameter,
                positionParameter);

            dynamicPositionLambda.Compile();

            var translated = dynamicPositionLambda.ToReadableString();

            translated.ShouldBe("(obj, position) => obj.Position = position");
        }

        [Fact]
        public void ShouldTranslateAParameterlessMethodCall()
        {
            var objectToStringCallSiteBinder = Binder.InvokeMember(
                CSharpBinderFlags.InvokeSimpleName,
                "ToString",
                null,
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Parameter(typeof(object), "obj");

            var dynamicToStringCall = Dynamic(
                objectToStringCallSiteBinder,
                typeof(object),
                dynamicParameter);

            var dynamicToStringLambda = Lambda<Func<object, object>>(dynamicToStringCall, dynamicParameter);

            dynamicToStringLambda.Compile();

            var translated = dynamicToStringLambda.ToReadableString();

            translated.ShouldBe("obj => obj.ToString()");
        }

        [Fact]
        public void ShouldTranslateACallToAMissingMethod()
        {
            // Just because the method doesn't exist doesn't mean
            // you can't build a dynamic call to it and that that
            // call shouldn't be translated...

            var objectYellHurrahCallSiteBinder = Binder.InvokeMember(
                CSharpBinderFlags.InvokeSimpleName,
                "YellHurrah",
                null,
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Parameter(typeof(object), "obj");

            var dynamicYellHurrahCall = Dynamic(
                objectYellHurrahCallSiteBinder,
                typeof(object),
                dynamicParameter);

            var dynamicYellHurrahLambda = Lambda<Func<object, object>>(dynamicYellHurrahCall, dynamicParameter);

            dynamicYellHurrahLambda.Compile();

            var translated = dynamicYellHurrahLambda.ToReadableString();

            translated.ShouldBe("obj => obj.YellHurrah()");
        }

        [Fact]
        public void ShouldTranslateAParameterisedMethodCall()
        {
            var objectToStringCallSiteBinder = Binder.InvokeMember(
                CSharpBinderFlags.InvokeSimpleName,
                "ToString",
                null,
                typeof(WhenTranslatingDynamicOperations),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                });

            var dynamicParameter = Parameter(typeof(object), "obj");
            var cultureInfoParameter = Parameter(typeof(CultureInfo), "ci");

            var dynamicToStringCall = Dynamic(
                objectToStringCallSiteBinder,
                typeof(object),
                dynamicParameter,
                cultureInfoParameter);

            var dynamicToStringLambda = Lambda<Func<object, CultureInfo, object>>(
                dynamicToStringCall,
                dynamicParameter,
                cultureInfoParameter);

            dynamicToStringLambda.Compile();

            var translated = dynamicToStringLambda.ToReadableString();

            translated.ShouldBe("(obj, ci) => obj.ToString(ci)");
        }

        [Fact]
        public void ShouldTranslateAGenericParameterisedMethodCall()
        {
            var valueConverterConvertCallSiteBinder = Binder.InvokeMember(
                CSharpBinderFlags.InvokeSimpleName,
                "Convert",
                new[] { typeof(string), typeof(int) },
                typeof(WhenTranslatingDynamicOperations),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                });

            var dynamicParameter = Parameter(typeof(ValueConverter), "valueConverter");
            var valueParameter = Parameter(typeof(string), "value");

            var dynamicConvertCall = Dynamic(
                valueConverterConvertCallSiteBinder,
                typeof(int),
                dynamicParameter,
                valueParameter);

            var dynamicConvertLambda = Lambda<Func<ValueConverter, string, int>>(
                dynamicConvertCall,
                dynamicParameter,
                valueParameter);

            dynamicConvertLambda.Compile();

            var translated = dynamicConvertLambda.ToReadableString();

            translated.ShouldBe("(valueConverter, value) => valueConverter.Convert<string, int>(value)");
        }

        [Fact]
        public void ShouldTranslateACallWithTooFewParameters()
        {
            var valueConverterConvertCallSiteBinder = Binder.InvokeMember(
                CSharpBinderFlags.InvokeSimpleName,
                "Convert",
                new[] { typeof(int), typeof(string) },
                typeof(WhenTranslatingDynamicOperations),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                });

            var dynamicParameter = Parameter(typeof(ValueConverter), "valueConverter");

            var dynamicConvertCall = Dynamic(
                valueConverterConvertCallSiteBinder,
                typeof(string),
                dynamicParameter);

            var dynamicConvertLambda = Lambda<Func<ValueConverter, string>>(
                dynamicConvertCall,
                dynamicParameter);

            dynamicConvertLambda.Compile();

            var translated = dynamicConvertLambda.ToReadableString();

            // The method type parameter can't be figured out from the arguments and return type, so are missing:
            translated.ShouldBe("valueConverter => valueConverter.Convert()");
        }

        [Fact]
        public void ShouldTranslateAParameterlessCallWithGenericParameters()
        {
            var typePrinterPrintCallSiteBinder = Binder.InvokeMember(
                CSharpBinderFlags.InvokeSimpleName,
                "Print",
                new[] { typeof(DateTime) },
                typeof(WhenTranslatingDynamicOperations),
                new[]
                {
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
                });

            var dynamicParameter = Parameter(typeof(TypePrinter), "typePrinter");

            var dynamicPrintCall = Dynamic(
                typePrinterPrintCallSiteBinder,
                typeof(void),
                dynamicParameter);

            var dynamicPrintLambda = Lambda<Action<TypePrinter>>(
                dynamicPrintCall,
                dynamicParameter);

            dynamicPrintLambda.Compile();

            var translated = dynamicPrintLambda.ToReadableString();

            // The method type parameter can't be figured out from the arguments and return type, so are missing:
            translated.ShouldBe("typePrinter => typePrinter.Print()");
        }

        // ReSharper disable once UnusedMember.Local
        private class ValueConverter
        {
            // ReSharper disable once UnusedMember.Local
            public TResult Convert<TValue, TResult>(TValue value)
            {
                return (TResult)(object)value;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private class TypePrinter
        {
            // ReSharper disable once UnusedMember.Local
            public void Print<T>()
            {
                Console.WriteLine(typeof(T));
            }
        }
    }
}
#endif