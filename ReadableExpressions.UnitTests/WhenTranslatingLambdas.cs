namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class WhenTranslatingLambdas
    {
        [Fact]
        public void ShouldTranslateAParameterlessLambda()
        {
            Expression<Func<int>> returnOneThousand = () => 1000;

            var translated = returnOneThousand.ToReadableString();

            Assert.Equal("() => 1000", translated);
        }

        [Fact]
        public void ShouldTranslateASingleParameterLambda()
        {
            Expression<Func<int, int>> returnArgumentPlusOneTen = i => i + 10;

            var translated = returnArgumentPlusOneTen.ToReadableString();

            Assert.Equal("i => i + 10", translated);
        }

        [Fact]
        public void ShouldTranslateAMultipleParameterLambda()
        {
            Expression<Func<string, string, int>> convertStringsToInt = (str1, str2) => int.Parse(str1) + int.Parse(str2);

            var translated = convertStringsToInt.ToReadableString();

            Assert.Equal("(str1, str2) => int.Parse(str1) + int.Parse(str2)", translated);
        }

        [Fact]
        public void ShouldTranslateALambdaInvocation()
        {
            Expression<Action> writeLine = () => Console.WriteLine();
            var writeLineInvocation = Expression.Invoke(writeLine);

            var translated = writeLineInvocation.ToReadableString();

            Assert.Equal("(() => Console.WriteLine()).Invoke()", translated);
        }

        // see http://stackoverflow.com/questions/3716492/what-does-expression-quote-do-that-expression-constant-can-t-already-do
        [Fact]
        public void ShouldTranslateAQuotedLambda()
        {
            Expression<Func<int, double>> intToDouble = i => i;

            var quotedLambda = Expression.Quote(intToDouble);

            var translated = quotedLambda.ToReadableString();

            Assert.Equal("i => (double)i", translated);
        }

        [Fact]
        public void ShouldTranslateAQuotedLambdaQueryableProjection()
        {
            Expression<Func<IQueryable<int>, IQueryable<object>>> project =
                items => items.Select(item => new { Number = item });

            var translated = project.Body.ToReadableString();

            Assert.Equal("items.Select(item => new { Number = item })", translated);
        }

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

            Assert.Equal(EXPECTED.TrimStart(), translated);
        }

        [Fact]
        public void ShouldTranslateANestedQuotedLambda()
        {
            var intA = Expression.Parameter(typeof(int), "a");
            var intB = Expression.Parameter(typeof(int), "b");
            var addition = Expression.Add(intA, intB);
            var additionInnerLambda = Expression.Lambda(addition, intB);
            var quotedInnerLambda = Expression.Quote(additionInnerLambda);
            var additionOuterLambda = Expression.Lambda(quotedInnerLambda, intA);

            var translated = additionOuterLambda.ToReadableString();

            Assert.Equal("a => b => a + b", translated);
        }

        [Fact]
        public void ShouldTranslateQuotedLambdaWithAnAnnotation()
        {
            Expression<Func<int, double>> intToDouble = i => i;

            var quotedLambda = Expression.Quote(intToDouble);

            var translated = quotedLambda.ToReadableString(o => o.ShowQuotedLambdaComments);

            const string EXPECTED = @"
    // Quoted to induce a closure:
    i => (double)i";

            Assert.Equal(EXPECTED, translated);
        }

        [Fact]
        public void ShouldTranslateRuntimeVariables()
        {
            var intVariable1 = Expression.Variable(typeof(int), "i1");
            var intVariable2 = Expression.Variable(typeof(int), "i2");
            var runtimeVariables = Expression.RuntimeVariables(intVariable1, intVariable2);

            var translated = runtimeVariables.ToReadableString();

            Assert.Equal("(i1, i2)", translated);
        }

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
    }
}
