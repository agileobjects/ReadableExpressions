#if FEATURE_EF
namespace AgileObjects.ReadableExpressions.UnitTests
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Linq;
    using Xunit;

    public class WhenTranslatingQueries : TestClassBase
    {
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
ObjectQuery<WhenTranslatingQueries.Product>
    .MergeAs(MergeOption.NoTracking)
    .Where(p => p.Id > 10)
    .Select(p => new WhenTranslatingQueries.Product
    {
        Id = p.Id,
        Name = p.Name
    })";
            translated.ShouldBe(EXPECTED.TrimStart());
        }

        #region Helper Members

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
#endif