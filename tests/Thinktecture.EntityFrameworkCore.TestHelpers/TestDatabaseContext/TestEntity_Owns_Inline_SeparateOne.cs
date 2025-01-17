

// ReSharper disable InconsistentNaming
#pragma warning disable 8618
namespace Thinktecture.TestDatabaseContext;

public class TestEntity_Owns_Inline_SeparateOne
{
   public Guid Id { get; set; }

   public OwnedEntity_Owns_SeparateOne InlineEntity { get; set; }

   public static void Configure(ModelBuilder modelBuilder)
   {
      modelBuilder.Entity<TestEntity_Owns_Inline_SeparateOne>(builder => builder.OwnsOne(e => e.InlineEntity,
                                                                                         navigationBuilder => navigationBuilder.OwnsOne(e => e.SeparateEntity,
                                                                                                                                        innerBuilder => innerBuilder.ToTable("InlineEntities_SeparateOne"))));
   }
}
