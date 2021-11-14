namespace AgileObjects.ReadableExpressions.UnitTests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Common;
    using ReadableExpressions.Extensions;
#if !NET35
    using Xunit;
    using static System.Linq.Expressions.Expression;
#else
    using static Microsoft.Scripting.Ast.Expression;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenGettingFriendlyTypeNames : TestClassBase
    {
        [Fact]
        public void ShouldNameAnArray()
        {
            var intArrayVariable = Variable(typeof(int[]), "ints");
            var assignNull = Assign(intArrayVariable, Default(intArrayVariable.Type));
            var assignNullBlock = Block(new[] { intArrayVariable }, assignNull);

            var translated = assignNullBlock.ToReadableString();

            translated.ShouldBe("var ints = default(int[]);");
        }

        [Fact]
        public void ShouldNameAStaticMethodTypeName()
        {
            var characterToNumeric = CreateLambda((char c) => char.GetNumericValue(c));

            var translated = characterToNumeric.ToReadableString();

            translated.ShouldBe("c => char.GetNumericValue(c)");
        }

        [Fact]
        public void ShouldNameAnAnonymousType()
        {
            var anon = new { One = 1, Two = "two" };

            var friendlyName = anon.GetType().GetFriendlyName();

            friendlyName.ShouldBe("AnonymousType<int, string>");
        }

        // See https://github.com/agileobjects/ReadableExpressions/pull/25
        [Fact]
        public void ShouldNameAnAnonymousTypeWithAConfiguredNameFactory()
        {
            var anon = new { One = 1, Two = "two" };

            var friendlyName = anon.GetType().GetFriendlyName(c => c.NameAnonymousTypesUsing(t => "object"));

            friendlyName.ShouldBe("object");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/6
        [Fact]
        public void ShouldNameMultiplyNestedTypes()
        {
            var nestedType = Constant(typeof(OuterClass.InnerClass.Nested), typeof(Type));

            var translated = nestedType.ToReadableString();

            translated.ShouldBe("typeof(OuterClass.InnerClass.Nested)");
        }

        [Fact]
        public void ShouldNameListsOfNestedTypes()
        {
            var newNestedTypeList = New(typeof(List<>).MakeGenericType(typeof(OuterClass.InnerClass)));

            var translated = newNestedTypeList.ToReadableString();

            translated.ShouldBe("new List<OuterClass.InnerClass>()");
        }

        [Fact]
        public void ShouldNameGenericTypes()
        {
            var genericListEnumeratorType = Constant(typeof(HashSet<decimal[]>), typeof(Type));

            var translated = genericListEnumeratorType.ToReadableString();

            translated.ShouldBe("typeof(HashSet<decimal[]>)");
        }

        [Fact]
        public void ShouldNameGenericNestedTypes()
        {
            var genericListEnumeratorType = Constant(typeof(List<string>.Enumerator), typeof(Type));

            var translated = genericListEnumeratorType.ToReadableString();

            translated.ShouldBe("typeof(List<string>.Enumerator)");
        }

        [Fact]
        public void ShouldNameNestedGenericTypes()
        {
            var genericListEnumeratorType = Constant(typeof(GenericTestHelper<int>), typeof(Type));

            var translated = genericListEnumeratorType.ToReadableString();

            translated.ShouldBe("typeof(WhenGettingFriendlyTypeNames.GenericTestHelper<int>)");
        }

        [Fact]
        public void ShouldNameGenericMultiplyNestedTypes()
        {
            var nestedGenericType = Constant(
                typeof(OuterGeneric<int>.InnerGeneric<long>.Nested),
                typeof(Type));

            var translated = nestedGenericType.ToReadableString();

            translated.ShouldBe("typeof(OuterGeneric<int>.InnerGeneric<long>.Nested)");
        }

        [Fact]
        public void ShouldNameGenericGenericTypeArguments()
        {
            var nestedGenericType = Constant(
                typeof(Dictionary<int, Dictionary<string, List<byte>>>),
                typeof(Type));

            var translated = nestedGenericType.ToReadableString();

            translated.ShouldBe("typeof(Dictionary<int, Dictionary<string, List<byte>>>)");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/94
        [Fact]
        public void ShouldNamePartClosedGenericTypeArrays()
        {
            var name = typeof(GenericTestHelper<>)
                .GetField("Keys").FieldType
                .GetFriendlyName();

            name.ShouldBe("KeyValuePair<T, int>[]");
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/97
        [Fact]
        public void ShouldNameLinqExpressionVariables()
        {
            Expression<Func<IEnumerable<int>>> linqQuery = () =>
                from s in new[] { "1", "2", "3", "4", "5" }
                let n = int.Parse(s)
                where n > 3
                group s by s into result
                select result.Count();

            const string EXPECTED = @"
() => new[] { ""1"", ""2"", ""3"", ""4"", ""5"" }
    .Select(s => new { s, n = int.Parse(s) })
    .Where(_ => _.n > 3)
    .GroupBy(_ => _.s, _ => _.s)
    .Select(result => result.Count())";

            var translated = linqQuery.ToReadableString();

            translated.ShouldBe(EXPECTED.TrimStart());
        }

        #region Helper Classes

        // ReSharper disable once UnusedTypeParameter
        private class GenericTestHelper<T>
        {
            public KeyValuePair<T, int>[] Keys;
        }

        #endregion
    }
}
