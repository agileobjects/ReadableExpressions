namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if !NET35
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenGettingAParentExpressionNode : TestClassBase
    {
        [Fact]
        public void ShouldReturnAMemberAccessParent()
        {
            var personViewModelName = CreateLambda((Type t) => t.Name);

            var namePropertyParent = personViewModelName.Body.GetParentOrNull() as ParameterExpression;

            namePropertyParent.ShouldNotBeNull().Name.ShouldBe("t");
        }

        [Fact]
        public void ShouldReturnANestedMemberAccessParent()
        {
            var typeNameLength = CreateLambda((Type t) => t.Name.Length);

            var typeNameLengthParent = typeNameLength.Body.GetParentOrNull() as MemberExpression;

            typeNameLengthParent.ShouldNotBeNull().Member.Name.ShouldBe("Name");
        }

        [Fact]
        public void ShouldReturnAMemberMethodCallParent()
        {
            var typeNameSubstring = CreateLambda((Type t) => t.Name.Substring(0, 3));

            var typeNameSubstringParent = typeNameSubstring.Body.GetParentOrNull() as MemberExpression;

            typeNameSubstringParent.ShouldNotBeNull().Member.Name.ShouldBe("Name");
        }

        [Fact]
        public void ShouldReturnAnExtensionMethodCallParent()
        {
            var typesToArray = CreateLambda((IEnumerable<Type> ts) => ts.ToArray());

            var typesToArrayPropertyParent = typesToArray.Body.GetParentOrNull() as ParameterExpression;

            typesToArrayPropertyParent.ShouldNotBeNull();
            typesToArrayPropertyParent.Name.ShouldBe("ts");
        }
    }
}
