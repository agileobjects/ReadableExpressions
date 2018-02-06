namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ReadableExpressions.Extensions;
    using Xunit;

    public class WhenGettingAParentExpressionNode
    {
        [Fact]
        public void ShouldReturnAMemberAccessParent()
        {
            Expression<Func<Type, string>> personViewModelName = t => t.Name;

            var namePropertyParent = personViewModelName.Body.GetParentOrNull() as ParameterExpression;

            Assert.NotNull(namePropertyParent);
            Assert.Equal("t", namePropertyParent.Name);
        }

        [Fact]
        public void ShouldReturnANestedMemberAccessParent()
        {
            Expression<Func<Type, string>> typeAssemblyImageVersion = t => t.Assembly.ImageRuntimeVersion;

            var typeAssemblyImageVersionParent = typeAssemblyImageVersion.Body.GetParentOrNull() as MemberExpression;

            Assert.NotNull(typeAssemblyImageVersionParent);
            Assert.Equal("Assembly", typeAssemblyImageVersionParent.Member.Name);
        }

        [Fact]
        public void ShouldReturnAMemberMethodCallParent()
        {
            Expression<Func<Type, string>> typeAssemblyToString = p => p.Assembly.ToString();

            var assemblyToStringPropertyParent = typeAssemblyToString.Body.GetParentOrNull() as MemberExpression;

            Assert.NotNull(assemblyToStringPropertyParent);
            Assert.Equal("Assembly", assemblyToStringPropertyParent.Member.Name);
        }

        [Fact]
        public void ShouldReturnAnExtensionMethodCallParent()
        {
            Expression<Func<IEnumerable<Type>, Type[]>> typesToArray = ts => ts.ToArray();

            var typesToArrayPropertyParent = typesToArray.Body.GetParentOrNull() as ParameterExpression;

            Assert.NotNull(typesToArrayPropertyParent);
            Assert.Equal("ts", typesToArrayPropertyParent.Name);
        }
    }
}
