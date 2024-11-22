using Microsoft.EntityFrameworkCore.Design;

namespace Thinktecture.TestDatabaseContext;

// ReSharper disable once UnusedMember.Global
public class TestDbContextDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
   public TestDbContext CreateDbContext(string[] args)
   {
      var options = new DbContextOptionsBuilder<TestDbContext>()
                    .UseSqlServer(TestContext.Instance.ConnectionString)
                    .AddSchemaRespectingComponents()
                    .Options;

      return new TestDbContext(options, null);
   }
}