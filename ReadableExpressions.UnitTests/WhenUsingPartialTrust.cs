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
            var translated = assignment.ToReadableString();

            Assert.Equal("i = 0", translated);
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

            Assert.Equal("obj => obj.Length", translated);
        }

        public void TestIntTypeEqualExpressionTranslation()
        {
            var intVariable = Expression.Variable(typeof(int), "i");
            var intIsLong = Expression.TypeEqual(intVariable, typeof(long));
            var translated = intIsLong.ToReadableString();

            Assert.Equal("(i TypeOf typeof(long))", translated);
        }

        public void TestObjectTypeEqualExpressionTranslation()
        {
            var objectVariable = Expression.Variable(typeof(object), "o");
            var objectIsString = Expression.TypeEqual(objectVariable, typeof(string));
            var translated = objectIsString.ToReadableString();

            Assert.Equal("(o is string)", translated);
        }
    }
}
