namespace AgileObjects.ReadableExpressions.UnitTests
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
        public void ShouldTranslateAPropertyAccess()
        {
            var arrayLengthAccessSiteBinder = Binder.GetMember(
                CSharpBinderFlags.None,
                "Length",
                typeof(WhenTranslatingDynamicOperations),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var dynamicParameter = Expression.Parameter(typeof(object), "obj");

            var dynamicLengthAccess = Expression.Dynamic(
                arrayLengthAccessSiteBinder,
                typeof(object),
                dynamicParameter);

            var dynamicLengthLambda = Expression
                .Lambda<Func<object, object>>(dynamicLengthAccess, dynamicParameter);

            dynamicLengthLambda.Compile();

            var translated = dynamicLengthLambda.ToReadableString();

            Assert.AreEqual("obj => obj.Length", translated);
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
    }
}
