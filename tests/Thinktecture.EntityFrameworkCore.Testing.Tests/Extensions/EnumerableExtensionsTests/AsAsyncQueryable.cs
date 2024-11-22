namespace Thinktecture.Extensions.EnumerableExtensionsTests;

public class AsAsyncQueryable
{
   [Fact]
   public void Should_throw_if_collection_is_null()
   {
      Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>)null!).AsAsyncQueryable());
   }

   [Fact]
   public async Task Should_create_empty_queryable_if_collection_is_emtpy()
   {
      var query = Enumerable.Empty<int>().AsAsyncQueryable();

      (await query.ToListAsync()).Should().BeEmpty();
      query.ToList().Should().BeEmpty();
   }

   [Fact]
   public async Task Should_create_queryable_from_collection()
   {
      var query = new[] { 1, 2, 3 }.AsAsyncQueryable();

      (await query.ToListAsync()).Should().BeEquivalentTo(new[] { 1, 2, 3 });
      query.ToList().Should().BeEquivalentTo(new[] { 1, 2, 3 });
   }
}
