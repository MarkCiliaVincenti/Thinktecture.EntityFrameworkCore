using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Thinktecture.EntityFrameworkCore.Data;

namespace Thinktecture.EntityFrameworkCore.BulkOperations;

[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
internal class BulkInsertOrUpdateContext : ISqliteBulkOperationContext
{
   private readonly IReadOnlyList<IProperty> _keyProperties;
   private readonly IReadOnlyList<PropertyWithNavigations> _propertiesToInsert;
   private readonly IReadOnlyList<PropertyWithNavigations> _propertiesToUpdate;
   private readonly IReadOnlyList<PropertyWithNavigations> _externalPropertiesToInsert;
   private readonly IReadOnlyList<PropertyWithNavigations> _externalPropertiesToUpdate;
   private readonly DbContext _ctx;
   private readonly IEntityDataReaderFactory _readerFactory;

   public IReadOnlyList<PropertyWithNavigations> Properties { get; }
   public bool HasExternalProperties => _externalPropertiesToInsert.Count != 0 || _externalPropertiesToUpdate.Count != 0;

   /// <inheritdoc />
   public IEntityDataReader<T> CreateReader<T>(IEnumerable<T> entities)
   {
      return _readerFactory.Create(_ctx, entities, Properties, HasExternalProperties);
   }

   public SqliteAutoIncrementBehavior AutoIncrementBehavior { get; }
   public SqliteConnection Connection { get; }

   public BulkInsertOrUpdateContext(
      DbContext ctx,
      IEntityDataReaderFactory factory,
      SqliteConnection connection,
      IReadOnlyList<IProperty> keyProperties,
      IReadOnlyList<PropertyWithNavigations> propertiesToInsert,
      IReadOnlyList<PropertyWithNavigations> propertiesForUpdate,
      SqliteAutoIncrementBehavior autoIncrementBehavior)
   {
      _ctx = ctx;
      _readerFactory = factory;
      Connection = connection;
      _keyProperties = keyProperties;
      AutoIncrementBehavior = autoIncrementBehavior;

      var (ownPropertiesToInsert, externalPropertiesToInsert) = propertiesToInsert.SeparateProperties();
      _propertiesToInsert = ownPropertiesToInsert;
      _externalPropertiesToInsert = externalPropertiesToInsert;

      var (ownPropertiesToUpdate, externalPropertiesToUpdate) = propertiesForUpdate.SeparateProperties();
      _propertiesToUpdate = ownPropertiesToUpdate;
      _externalPropertiesToUpdate = externalPropertiesToUpdate;

      Properties = ownPropertiesToInsert.Union(ownPropertiesToUpdate).Union(keyProperties.Select(p => new PropertyWithNavigations(p, Array.Empty<Navigation>()))).ToList();
   }

   /// <inheritdoc />
   public SqliteCommandBuilder CreateCommandBuilder()
   {
      return SqliteCommandBuilder.InsertOrUpdate(_propertiesToInsert, _propertiesToUpdate, _keyProperties);
   }

   public IReadOnlyList<ISqliteOwnedTypeBulkOperationContext> GetChildren(IReadOnlyList<object> entities)
   {
      if (!HasExternalProperties)
         return Array.Empty<ISqliteOwnedTypeBulkOperationContext>();

      var childCtx = new List<ISqliteOwnedTypeBulkOperationContext>();

      var propertiesToUpdateData = _externalPropertiesToUpdate.GroupExternalProperties(entities).ToList();

      foreach (var (navigation, ownedEntities, propertiesToInsert) in _externalPropertiesToInsert.GroupExternalProperties(entities))
      {
         var propertiesToUpdateTuple = propertiesToUpdateData.FirstOrDefault(d => d.Item1 == navigation);
         var propertiesToUpdate = propertiesToUpdateTuple.Item3;

         if (propertiesToUpdate is null || propertiesToUpdate.Count == 0)
            throw new Exception($"The owned type property '{navigation.DeclaringEntityType.Name}.{navigation.Name}' is selected for bulk-insert-or-update but there are no properties for performing the update.");

         propertiesToUpdateData.Remove(propertiesToUpdateTuple);

         var ownedTypeCtx = new OwnedTypeBulkInsertOrUpdateContext(_ctx, _readerFactory, Connection, propertiesToInsert, propertiesToUpdate, navigation.TargetEntityType, ownedEntities, AutoIncrementBehavior);
         childCtx.Add(ownedTypeCtx);
      }

      if (propertiesToUpdateData.Count != 0)
      {
         var updateWithoutInsert = propertiesToUpdateData[0].Item1;
         throw new Exception($"{propertiesToUpdateData.Count} owned type property/properties including '{updateWithoutInsert.DeclaringEntityType.Name}.{updateWithoutInsert.Name}' is selected for bulk-insert-or-update but there are no properties for performing the insert.");
      }

      return childCtx;
   }

   private class OwnedTypeBulkInsertOrUpdateContext : BulkInsertOrUpdateContext, ISqliteOwnedTypeBulkOperationContext
   {
      public IEntityType EntityType { get; }
      public IEnumerable<object> Entities { get; }

      public OwnedTypeBulkInsertOrUpdateContext(
         DbContext ctx,
         IEntityDataReaderFactory factory,
         SqliteConnection sqlCon,
         IReadOnlyList<PropertyWithNavigations> propertiesToInsert,
         IReadOnlyList<PropertyWithNavigations> propertiesToUpdate,
         IEntityType entityType,
         IEnumerable<object> entities,
         SqliteAutoIncrementBehavior autoIncrementBehavior)
         : base(ctx, factory, sqlCon, GetKeyProperties(entityType), propertiesToInsert, propertiesToUpdate, autoIncrementBehavior)
      {
         EntityType = entityType;
         Entities = entities;
      }

      private static IReadOnlyList<IProperty> GetKeyProperties(IEntityType entityType)
      {
         var properties = entityType.FindPrimaryKey()?.Properties;

         if (properties is null || properties.Count == 0)
            throw new Exception($"The entity type '{entityType.Name}' needs a primary key to be able to perform bulk-insert-or-update.");

         return properties;
      }
   }
}
