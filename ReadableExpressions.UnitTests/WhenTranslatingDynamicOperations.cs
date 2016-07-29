﻿namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenTranslatingDynamicOperations
    {
        [TestMethod]
        public void ShouldTranslateAPropertyReadAccess()
        {
            var lengthGetterSiteBinder = Binder.GetMember(
                CSharpBinderFlags.None,
                "Length",
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Expression.Parameter(typeof(object), "obj");

            var dynamicLengthGetter = Expression.Dynamic(
                lengthGetterSiteBinder,
                typeof(object),
                dynamicParameter);

            var dynamicLengthLambda = Expression
                .Lambda<Func<object, object>>(dynamicLengthGetter, dynamicParameter);

            dynamicLengthLambda.Compile();

            var translated = dynamicLengthLambda.ToReadableString();

            Assert.AreEqual("obj => obj.Length", translated);
        }

        [TestMethod]
        public void ShouldTranslateAPropertyWriteAccess()
        {
            var positionSetterSiteBinder = Binder.SetMember(
                CSharpBinderFlags.ResultDiscarded,
                "Position",
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Expression.Parameter(typeof(object), "obj");
            var positionParameter = Expression.Parameter(typeof(long), "position");

            var dynamicPositionSetter = Expression.Dynamic(
                positionSetterSiteBinder,
                typeof(object),
                dynamicParameter,
                positionParameter);

            var dynamicPositionLambda = Expression.Lambda<Action<object, long>>(
                dynamicPositionSetter,
                dynamicParameter,
                positionParameter);

            dynamicPositionLambda.Compile();

            var translated = dynamicPositionLambda.ToReadableString();

            Assert.AreEqual("(obj, position) => obj.Position = position", translated);
        }

        [TestMethod]
        public void ShouldTranslateAParameterlessMethodCall()
        {
            var objectToStringCallSiteBinder = Binder.InvokeMember(
                CSharpBinderFlags.InvokeSimpleName,
                "ToString",
                null,
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Expression.Parameter(typeof(object), "obj");

            var dynamicToStringCall = Expression.Dynamic(
                objectToStringCallSiteBinder,
                typeof(object),
                dynamicParameter);

            var dynamicToStringLambda = Expression
                .Lambda<Func<object, object>>(dynamicToStringCall, dynamicParameter);

            dynamicToStringLambda.Compile();

            var translated = dynamicToStringLambda.ToReadableString();

            Assert.AreEqual("obj => obj.ToString()", translated);
        }

        [TestMethod]
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

            var dynamicParameter = Expression.Parameter(typeof(object), "obj");

            var dynamicYellHurrahCall = Expression.Dynamic(
                objectYellHurrahCallSiteBinder,
                typeof(object),
                dynamicParameter);

            var dynamicYellHurrahLambda = Expression
                .Lambda<Func<object, object>>(dynamicYellHurrahCall, dynamicParameter);

            dynamicYellHurrahLambda.Compile();

            var translated = dynamicYellHurrahLambda.ToReadableString();

            Assert.AreEqual("obj => obj.YellHurrah()", translated);
        }

        [TestMethod]
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

            var dynamicParameter = Expression.Parameter(typeof(object), "obj");
            var cultureInfoParameter = Expression.Parameter(typeof(CultureInfo), "ci");

            var dynamicToStringCall = Expression.Dynamic(
                objectToStringCallSiteBinder,
                typeof(object),
                dynamicParameter,
                cultureInfoParameter);

            var dynamicToStringLambda = Expression.Lambda<Func<object, CultureInfo, object>>(
                dynamicToStringCall,
                dynamicParameter,
                cultureInfoParameter);

            dynamicToStringLambda.Compile();

            var translated = dynamicToStringLambda.ToReadableString();

            Assert.AreEqual("(obj, ci) => obj.ToString(ci)", translated);
        }

        [TestMethod]
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

            var dynamicParameter = Expression.Parameter(typeof(ValueConverter), "valueConverter");
            var valueParameter = Expression.Parameter(typeof(string), "value");

            var dynamicConvertCall = Expression.Dynamic(
                valueConverterConvertCallSiteBinder,
                typeof(int),
                dynamicParameter,
                valueParameter);

            var dynamicConvertLambda = Expression.Lambda<Func<ValueConverter, string, int>>(
                dynamicConvertCall,
                dynamicParameter,
                valueParameter);

            dynamicConvertLambda.Compile();

            var translated = dynamicConvertLambda.ToReadableString();

            Assert.AreEqual("(valueConverter, value) => valueConverter.Convert<string, int>(value)", translated);
        }

        [TestMethod]
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

            var dynamicParameter = Expression.Parameter(typeof(ValueConverter), "valueConverter");

            var dynamicConvertCall = Expression.Dynamic(
                valueConverterConvertCallSiteBinder,
                typeof(string),
                dynamicParameter);

            var dynamicConvertLambda = Expression.Lambda<Func<ValueConverter, string>>(
                dynamicConvertCall,
                dynamicParameter);

            dynamicConvertLambda.Compile();

            var translated = dynamicConvertLambda.ToReadableString();

            // The method type parameter can't be figured out from the arguments and return type, so are missing:
            Assert.AreEqual("valueConverter => valueConverter.Convert()", translated);
        }

        [TestMethod]
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

            var dynamicParameter = Expression.Parameter(typeof(TypePrinter), "typePrinter");

            var dynamicPrintCall = Expression.Dynamic(
                typePrinterPrintCallSiteBinder,
                typeof(void),
                dynamicParameter);

            var dynamicPrintLambda = Expression.Lambda<Action<TypePrinter>>(
                dynamicPrintCall,
                dynamicParameter);

            dynamicPrintLambda.Compile();

            var translated = dynamicPrintLambda.ToReadableString();

            // The method type parameter can't be figured out from the arguments and return type, so are missing:
            Assert.AreEqual("typePrinter => typePrinter.Print()", translated);
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
