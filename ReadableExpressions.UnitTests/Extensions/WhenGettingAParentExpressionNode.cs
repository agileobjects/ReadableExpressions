namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
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

            namePropertyParent.ShouldNotBeNull();
            namePropertyParent.Name.ShouldBe("t");
        }

        [Fact]
        public void ShouldReturnANestedMemberAccessParent()
        {
            var typeAssemblyImageVersion = CreateLambda((Type t) => t.Assembly.ImageRuntimeVersion);

            var typeAssemblyImageVersionParent = typeAssemblyImageVersion.Body.GetParentOrNull() as MemberExpression;

            typeAssemblyImageVersionParent.ShouldNotBeNull();
            typeAssemblyImageVersionParent.Member.Name.ShouldBe("Assembly");
        }

        [Fact]
        public void ShouldReturnAMemberMethodCallParent()
        {
            var typeAssemblyToString = CreateLambda((Type t) => t.Assembly.ToString());

            var assemblyToStringPropertyParent = typeAssemblyToString.Body.GetParentOrNull() as MemberExpression;

            assemblyToStringPropertyParent.ShouldNotBeNull();
            assemblyToStringPropertyParent.Member.Name.ShouldBe("Assembly");
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
