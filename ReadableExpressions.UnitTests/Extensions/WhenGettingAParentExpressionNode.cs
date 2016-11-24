namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ReadableExpressions.Extensions;

    public class WhenGettingAParentExpressionNode
    {
        [TestMethod]
        public void ShouldReturnAMemberAccessParent()
        {
            Expression<Func<Type, string>> personViewModelName = t => t.Name;

            var namePropertyParent = personViewModelName.Body.GetParentOrNull() as ParameterExpression;

            Assert.IsNotNull(namePropertyParent);
            Assert.AreEqual("t", namePropertyParent.Name);
        }

        [TestMethod]
        public void ShouldReturnANestedMemberAccessParent()
        {
            Expression<Func<Type, string>> typeAssemblyImageVersion = t => t.Assembly.ImageRuntimeVersion;

            var typeAssemblyImageVersionParent = typeAssemblyImageVersion.Body.GetParentOrNull() as MemberExpression;

            Assert.IsNotNull(typeAssemblyImageVersionParent);
            Assert.AreEqual("Assembly", typeAssemblyImageVersionParent.Member.Name);
        }

        [TestMethod]
        public void ShouldReturnAMemberMethodCallParent()
        {
            Expression<Func<Type, string>> typeAssemblyToString = p => p.Assembly.ToString();

            var assemblyToStringPropertyParent = typeAssemblyToString.Body.GetParentOrNull() as MemberExpression;

            Assert.IsNotNull(assemblyToStringPropertyParent);
            Assert.AreEqual("Assembly", assemblyToStringPropertyParent.Member.Name);
        }

        [TestMethod]
        public void ShouldReturnAnExtensionMethodCallParent()
        {
            Expression<Func<IEnumerable<Type>, Type[]>> typesToArray = ts => ts.ToArray();

            var typesToArrayPropertyParent = typesToArray.Body.GetParentOrNull() as ParameterExpression;

            Assert.IsNotNull(typesToArrayPropertyParent);
            Assert.AreEqual("ts", typesToArrayPropertyParent.Name);
        }
    }
}
