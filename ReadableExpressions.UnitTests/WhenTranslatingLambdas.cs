namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.Linq;
#if !NET35
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq.Expressions;
    using Xunit;
#else
    using Microsoft.Scripting.Ast;
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenTranslatingLambdas : TestClassBase
    {
        [Fact]
        public void ShouldTranslateAParameterlessLambda()
        {
            var returnOneThousand = CreateLambda(() => 1000);

            var translated = ToReadableString(returnOneThousand);

            translated.ShouldBe("() => 1000");
        }

        [Fact]
        public void ShouldTranslateASingleParameterLambda()
        {
            var returnArgumentPlusOneTen = CreateLambda((int i) => i + 10);

            var translated = ToReadableString(returnArgumentPlusOneTen);

            translated.ShouldBe("i => i + 10");
        }

        [Fact]
        public void ShouldTranslateAMultipleParameterLambda()
        {
            var convertStringsToInt = CreateLambda((string str1, string str2) => int.Parse(str1) + int.Parse(str2));

            var translated = ToReadableString(convertStringsToInt);

            translated.ShouldBe("(str1, str2) => int.Parse(str1) + int.Parse(str2)");
        }

        [Fact]
        public void ShouldTranslateALambdaInvocation()
        {
            var writeLine = CreateLambda(() => Console.WriteLine());
            var writeLineInvocation = Expression.Invoke(writeLine);

            var translated = ToReadableString(writeLineInvocation);

            translated.ShouldBe("(() => Console.WriteLine()).Invoke()");
        }

        // see http://stackoverflow.com/questions/3716492/what-does-expression-quote-do-that-expression-constant-can-t-already-do
        [Fact]
        public void ShouldTranslateAQuotedLambda()
        {
            var intToDouble = CreateLambda((int i) => (double)i);

            var quotedLambda = Expression.Quote(intToDouble);

            var translated = ToReadableString(quotedLambda);

            translated.ShouldBe("i => (double)i");
        }

        [Fact]
        public void ShouldTranslateAQuotedLambdaQueryableProjection()
        {
            var project = CreateLambda((IQueryable<int> items) => items.Select(item => new { Number = item }));

            var translated = ToReadableString(project.Body);

            translated.ShouldBe("items.Select(item => new { Number = item })");
        }

#if !NET35
        [Fact]
        public void ShouldTranslateADbSetQueryExpression()
        {
            var productQuery = new TestDbContext()
                .Products
                .AsNoTracking()
                .Where(p => p.Id > 10)
                .Select(p => new Product
                {
                    Id = p.Id,
                    Name = p.Name
                });

            var translated = productQuery.Expression.ToReadableString();

            const string EXPECTED = @"
ObjectQuery<WhenTranslatingLambdas.Product>
    .MergeAs(MergeOption.NoTracking)
    .Where(p => p.Id > 10)
    .Select(p => new WhenTranslatingLambdas.Product
    {
        Id = p.Id,
        Name = p.Name
    })";

            translated.ShouldBe(EXPECTED.TrimStart());
        }
#endif

        [Fact]
        public void ShouldTranslateANestedQuotedLambda()
        {
            var intA = Expression.Parameter(typeof(int), "a");
            var intB = Expression.Parameter(typeof(int), "b");
            var addition = Expression.Add(intA, intB);
            var additionInnerLambda = Expression.Lambda(addition, intB);
            var quotedInnerLambda = Expression.Quote(additionInnerLambda);
            var additionOuterLambda = Expression.Lambda(quotedInnerLambda, intA);

            var translated = ToReadableString(additionOuterLambda);

            translated.ShouldBe("a => b => a + b");
        }

        [Fact]
        public void ShouldTranslateQuotedLambdaWithAnAnnotation()
        {
            var intToDouble = CreateLambda((int i) => (double)i);

            var quotedLambda = Expression.Quote(intToDouble);

            var translated = ToReadableString(quotedLambda, o => o.ShowQuotedLambdaComments);

            const string EXPECTED = @"
// Quoted to induce a closure:
i => (double)i";

            translated.ShouldBe(EXPECTED);
        }

        // See https://github.com/agileobjects/ReadableExpressions/issues/31
        [Fact]
        public void ShouldTranslateUnnamedLambdaParameters()
        {
            var stringsParameter = Expression.Parameter(typeof(string[]));
            var linqSelect = CreateLambda((string[] ints) => ints.Select(int.Parse));
            var linqSelectWithUnnamed = Expression.Lambda(linqSelect.Body, stringsParameter);
            var quoted = Expression.Quote(linqSelectWithUnnamed);

            var translated = ToReadableString(quoted);

            translated.ShouldBe("stringArray => ints.Select(int.Parse)");
        }

        [Fact]
        public void ShouldTranslateRuntimeVariables()
        {
            var intVariable1 = Expression.Variable(typeof(int), "i1");
            var intVariable2 = Expression.Variable(typeof(int), "i2");
            var runtimeVariables = Expression.RuntimeVariables(intVariable1, intVariable2);

            var translated = ToReadableString(runtimeVariables);

            translated.ShouldBe("(i1, i2)");
        }

#if !NET35
        #region Helper Methods

        private class TestDbContext : DbContext
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DbSet<Product> Products { get; set; }
        }

        private class Product
        {
            [Key]
            public int Id { get; set; }

            public string Name { get; set; }
        }

        #endregion
#endif
    }
}
