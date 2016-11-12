namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using System.Security;
    using System.Security.Policy;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WhenUsingPartialTrust
    {
        [TestMethod]
        public void ShouldTranslateASimpleAssignment()
        {
            ExecuteInPartialTrust(helper =>
            {
                var translated = helper.TranslateSimpleAssignment();

                Assert.AreEqual("i = 0", translated);
            });
        }

        [TestMethod]
        public void ShouldTranslateADynamicExpression()
        {
            ExecuteInPartialTrust(helper =>
            {
                var translated = helper.TranslateDynamicExpression();

                Assert.AreEqual("obj => obj.Length", translated);
            });
        }

        [TestMethod]
        public void ShouldTranslateAValueTypeTypeEqualExpression()
        {
            ExecuteInPartialTrust(helper =>
            {
                var translated = helper.TranslateIntTypeEqualExpression();

                Assert.AreEqual("(i TypeOf typeof(long))", translated);
            });
        }

        [TestMethod]
        public void ShouldTranslateAnObjectTypeEqualExpression()
        {
            ExecuteInPartialTrust(helper =>
            {
                var translated = helper.TranslateObjectTypeEqualExpression();

                Assert.AreEqual("(o is string)", translated);
            });
        }

        private static void ExecuteInPartialTrust(Action<TranslationHelper> testAction)
        {
            AppDomain partialTrustDomain = null;

            try
            {
                var evidence = new Evidence();
                evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

                var permissions = new NamedPermissionSet(
                    "PartialTrust",
                    SecurityManager.GetStandardSandbox(evidence));

                partialTrustDomain = AppDomain.CreateDomain(
                    "PartialTrust",
                    evidence,
                    new AppDomainSetup { ApplicationBase = "." },
                    permissions);

                var helperType = typeof(TranslationHelper);

                var helper = (TranslationHelper)partialTrustDomain
                    .CreateInstanceAndUnwrap(helperType.Assembly.FullName, helperType.FullName);

                testAction.Invoke(helper);
            }
            finally
            {
                if (partialTrustDomain != null)
                {
                    AppDomain.Unload(partialTrustDomain);
                }
            }
        }
    }

    public class TranslationHelper : MarshalByRefObject
    {
        public string TranslateSimpleAssignment()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var assignment = Expression.Assign(intVariable, Expression.Constant(0));

            return assignment.ToReadableString();
        }

        public string TranslateDynamicExpression()
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

            return dynamicLengthLambda.ToReadableString();
        }

        public string TranslateIntTypeEqualExpression()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intIsLong = Expression.TypeEqual(intVariable, typeof(long));

            return intIsLong.ToReadableString();
        }

        public string TranslateObjectTypeEqualExpression()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectIsString = Expression.TypeEqual(objectVariable, typeof(string));

            return objectIsString.ToReadableString();
        }
    }
}
