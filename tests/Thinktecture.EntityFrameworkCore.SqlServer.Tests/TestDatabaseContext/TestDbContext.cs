using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Thinktecture.EntityFrameworkCore;
using Thinktecture.EntityFrameworkCore.TempTables;

namespace Thinktecture.TestDatabaseContext
{
   public class TestDbContext : DbContext, IDbDefaultSchema
   {
      /// <inheritdoc />
      public string? Schema { get; }

#nullable disable
      public DbSet<TestEntity> TestEntities { get; set; }
      public DbSet<TestEntityWithAutoIncrement> TestEntitiesWithAutoIncrement { get; set; }
      public DbSet<TestEntityWithRowVersion> TestEntitiesWithRowVersion { get; set; }
      public DbSet<TestEntityWithShadowProperties> TestEntitiesWithShadowProperties { get; set; }
      public DbSet<TestEntityWithSqlDefaultValues> TestEntitiesWithDefaultValues { get; set; }
      public DbSet<TestEntityWithDotnetDefaultValues> TestEntitiesWithDotnetDefaultValues { get; set; }
      public DbSet<TestEntityOwningInlineEntity> TestEntitiesOwningInlineEntity { get; set; }
      public DbSet<TestEntityOwningOneSeparateEntity> TestEntitiesOwningOneSeparateEntity { get; set; }
      public DbSet<TestEntityOwningManyEntities> TestEntitiesOwningManyEntities { get; set; }
      public IQueryable<TestViewEntity> TestView => Set<TestViewEntity>();
#nullable enable

      public Action<ModelBuilder>? ConfigureModel { get; set; }

      public TestDbContext(DbContextOptions<TestDbContext> options, IDbDefaultSchema? schema)
         : base(options)
      {
         Schema = schema?.Schema;
      }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);

         TestEntity.Configure(modelBuilder);

         modelBuilder.Entity<TestViewEntity>(builder => builder.ToView("TestView"));

         modelBuilder.Entity<TestEntityWithAutoIncrement>().Property(e => e.Id).UseIdentityColumn();

         modelBuilder.Entity<TestEntityWithRowVersion>()
                     .Property(e => e.RowVersion)
                     .IsRowVersion()
                     .HasConversion(new NumberToBytesConverter<long>());

         TestEntityWithShadowProperties.Configure(modelBuilder);
         TestEntityWithSqlDefaultValues.Configure(modelBuilder);
         modelBuilder.Entity<TestEntityWithSqlDefaultValues>(builder => builder.Property(e => e.Id).HasDefaultValueSql("newid()"));

         TestEntityWithDotnetDefaultValues.Configure(modelBuilder);
         TestEntityOwningInlineEntity.Configure(modelBuilder);
         TestEntityOwningOneSeparateEntity.Configure(modelBuilder);
         TestEntityOwningManyEntities.Configure(modelBuilder);

         ConfigureModel?.Invoke(modelBuilder);

         modelBuilder.Entity<InformationSchemaColumn>().HasNoKey().ToView("<<InformationSchemaColumn>>");
         modelBuilder.Entity<InformationSchemaTableConstraint>().HasNoKey().ToView("<<InformationSchemaTableConstraint>>");
         modelBuilder.Entity<InformationSchemaConstraintColumn>().HasNoKey().ToView("<<InformationSchemaConstraintColumn>>");
         modelBuilder.Entity<InformationSchemaKeyColumn>().HasNoKey().ToView("<<InformationSchemaKeyColumn>>");
      }

      public IQueryable<InformationSchemaColumn> GetTempTableColumns<T>()
         where T : class
      {
         var type = typeof(T);
         var entityType = Model.GetEntityType(type);
         return GetTempTableColumns(entityType);
      }

      public IQueryable<InformationSchemaColumn> GetTempTableColumns(IEntityType entityType)
      {
         var tableName = entityType.GetTableName();

         return GetTempTableColumns(tableName);
      }

      public IQueryable<InformationSchemaColumn> GetTempTableColumns(string tableName)
      {
         if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

         if (!tableName.StartsWith("#", StringComparison.Ordinal))
            tableName = $"#{tableName}";

         return Set<InformationSchemaColumn>().FromSqlInterpolated($@"
SELECT
   *
FROM
   tempdb.INFORMATION_SCHEMA.COLUMNS WITH (NOLOCK)
WHERE
   OBJECT_ID(TABLE_CATALOG + '..' + TABLE_NAME) = OBJECT_ID({"tempdb.." + tableName})");
      }

      public IQueryable<InformationSchemaTableConstraint> GetTempTableConstraints<T>()
      {
         var tableName = this.GetEntityType<T>().GetTableName();

         if (!tableName.StartsWith("#", StringComparison.Ordinal))
            tableName = $"#{tableName}";

         return Set<InformationSchemaTableConstraint>().FromSqlInterpolated($@"
SELECT
   *
FROM
   tempdb.INFORMATION_SCHEMA.TABLE_CONSTRAINTS WITH (NOLOCK)
WHERE
   OBJECT_ID(TABLE_CATALOG + '..' + TABLE_NAME) = OBJECT_ID({"tempdb.." + tableName})");
      }

      public IQueryable<InformationSchemaConstraintColumn> GetTempTableConstraintsColumns<T>()
      {
         var tableName = this.GetEntityType<T>().GetTableName();

         if (!tableName.StartsWith("#", StringComparison.Ordinal))
            tableName = $"#{tableName}";

         return Set<InformationSchemaConstraintColumn>().FromSqlInterpolated($@"
SELECT
   *
FROM
   tempdb.INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WITH (NOLOCK)
WHERE
   OBJECT_ID(TABLE_CATALOG + '..' + TABLE_NAME) = OBJECT_ID({"tempdb.." + tableName})");
      }

      public IQueryable<InformationSchemaKeyColumn> GetTempTableKeyColumns<T>()
      {
         var tableName = this.GetEntityType<T>().GetTableName();

         return GetTempTableKeyColumns(tableName);
      }

      public IQueryable<InformationSchemaKeyColumn> GetTempTableKeyColumns<TColumn1, TColumn2>()
      {
         var tableName = this.GetEntityType<TempTable<TColumn1, TColumn2>>().GetTableName();

         return GetTempTableKeyColumns(tableName);
      }

      private IQueryable<InformationSchemaKeyColumn> GetTempTableKeyColumns(string tableName)
      {
         if (!tableName.StartsWith("#", StringComparison.Ordinal))
            tableName = $"#{tableName}";

         return Set<InformationSchemaKeyColumn>().FromSqlInterpolated($@"
SELECT
   *
FROM
   tempdb.INFORMATION_SCHEMA.KEY_COLUMN_USAGE WITH (NOLOCK)
WHERE
   OBJECT_ID(TABLE_CATALOG + '..' + TABLE_NAME) = OBJECT_ID({"tempdb.." + tableName})");
      }
   }
}
