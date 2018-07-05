#if !NET35
namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq.Expressions;
    using System.Security;
    using System.Security.Policy;
    using Microsoft.CSharp.RuntimeBinder;
    using Xunit;

    public class WhenUsingPartialTrust
    {
        [Fact]
        public void ShouldTranslateASimpleAssignment()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestSimpleAssignmentTranslation();
            });
        }

        [Fact]
        public void ShouldTranslateADynamicExpression()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestDynamicExpressionTranslation();
            });
        }

        [Fact]
        public void ShouldTranslateAValueTypeTypeEqualExpression()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestIntTypeEqualExpressionTranslation();
            });
        }

        [Fact]
        public void ShouldTranslateAnObjectTypeEqualExpression()
        {
            ExecuteInPartialTrust(helper =>
            {
                helper.TestObjectTypeEqualExpressionTranslation();
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

                // ReSharper disable once AssignNullToNotNullAttribute
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
        public void TestSimpleAssignmentTranslation()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var assignment = Expression.Assign(intVariable, Expression.Constant(0));
            var translated = TestClassBase.ToReadableString(assignment);

            translated.ShouldBe("i = 0");
        }

        public void TestDynamicExpressionTranslation()
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

            translated.ShouldBe("obj => obj.Length");
        }

        public void TestIntTypeEqualExpressionTranslation()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intIsLong = Expression.TypeEqual(intVariable, typeof(long));
            var translated = TestClassBase.ToReadableString(intIsLong);

            translated.ShouldBe("(i TypeOf typeof(long))");
        }

        public void TestObjectTypeEqualExpressionTranslation()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectIsString = Expression.TypeEqual(objectVariable, typeof(string));
            var translated = TestClassBase.ToReadableString(objectIsString);

            translated.ShouldBe("(o is string)");
        }
    }
}
#endif
